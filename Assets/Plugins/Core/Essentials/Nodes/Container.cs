using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Core.Attributes;
using R3;

namespace Core {
    public class Container : IDisposable {
        private static readonly Type[] _injectAttributeTypes = {typeof(InjectAttribute) };
        private static readonly Dictionary<Type, (List<FieldInfo>, List<(MethodInfo, ParameterInfo[])>)> _getInjectMetaCache = new (128);
        private static readonly Dictionary<Type, List<FieldInfo>> _fieldCacheByType = new (512);
        private static readonly Dictionary<Type, List<(MethodInfo, ParameterInfo[])>> _methodCacheByType = new (512);

        public event Action Editor_OnServicesChanged = delegate {};
        public event Action Editor_OnDependenciesChanged = delegate {};
        private readonly CompositeDisposable _disposables = new ();
        private readonly Dictionary<Type, List<Registration>> _services = new ();
        private readonly Dictionary<Type, int> _dependencies = new ();

        private readonly Container _parentContainer;
        private bool _constructed;

        public Node Owner { get; }

        public Container(Node owner, Container parent) {
            Owner = owner;
            _parentContainer = parent;
        }

        public Registration Register<T>(object service) where T : class {
            var key = typeof(T);
            if (_services.ContainsKey(key) == false) {
                _services[key] = new List<Registration>();
            }

            var registration = new Registration(service);
            _services[key].Add(registration);
            Editor_OnServicesChanged();
            return registration;
        }

        public Registration Register(Type key, object service) {
            if (_services.ContainsKey(key) == false) {
                _services[key] = new List<Registration>();
            }

            var registration = new Registration(service);
            _services[key].Add(registration);
            Editor_OnServicesChanged();
            return registration;
        }

        public void SetConstructed(bool constructed) {
            _constructed = constructed;
        }

        public void Inject(object target, IInjectTarget constraint = null) {
            ThrowIfWrongState();

            var targetType = target.GetType();
            var constraintType = constraint?.GetType();
            var (fields, methods) = GetInjectMeta(targetType);

            foreach (var field in fields) {
                if (TryGetListType(field.FieldType, out var listElementType)) {
                    var list = Activator.CreateInstance(field.FieldType);
                    ResolveAll(constraintType, list, listElementType);
                    field.SetValue(target, list);
                }
                else {
                    var resolved = Resolve(field.FieldType, constraintType);
                    field.SetValue(target, resolved);
                }
            }

            foreach (var (methodInfo, parameters) in methods) {
                var invokeParams = new object[parameters.Length];
                for (var index = 0; index < parameters.Length; index++) {
                    var param = parameters[index];
                    if (TryGetListType(param.ParameterType, out var listElementType)) {
                        var list = Activator.CreateInstance(param.ParameterType);
                        ResolveAll(constraintType, list, listElementType);
                        invokeParams[index] = list;
                    }
                    else {
                        var resolved = Resolve(param.ParameterType, constraintType);
                        invokeParams[index] = resolved;
                    }
                }

                methodInfo.Invoke(target, invokeParams);
            }
        }

        public T Resolve<T>(IInjectTarget target = null, Container initialContainer = null) where T : class {
            ThrowIfWrongState();

            if (initialContainer == null) {
                initialContainer = this;
            }

            var key = typeof(T);
            IncrementDependencies(key);

            if (typeof(T) == typeof(Container)) {
                return this as T;
            }

            if (_services.TryGetValue(key, out var services) && services.Count != 0) {
                foreach (var registration in services) {
                    if (registration.CanBeInjectedInto(target?.GetType())) {
                        return (T)registration.Instance;
                    }
                }
            }

            if (_parentContainer != null) {
                return _parentContainer.Resolve<T>(target, initialContainer);
            }

            throw new Exception($"Service {typeof(T).Name} not found for requester '{target?.GetType().FullName}' in '{initialContainer.Owner.GetType().FullName}'");
        }

        public bool TryResolve<T>(IInjectTarget target, out T service) where T : class {
            ThrowIfWrongState();

            var key = typeof(T);
            IncrementDependencies(key);

            if (typeof(T) == typeof(Container)) {
                service = this as T;
                return true;
            }

            if (_services.TryGetValue(key, out var services)) {
                foreach (var registration in services) {
                    if (registration.CanBeInjectedInto(target.GetType())) {
                        service = registration.Instance as T;
                        return true;
                    }
                }
            }

            if (_parentContainer != null) {
                return _parentContainer.TryResolve(target, out service);
            }

            service = null;
            return false;
        }

        public HashSet<T> ResolveAll<T>(IInjectTarget target) where T : class {
            var services = new HashSet<T>();
            ResolveAll(target, services);
            return services;
        }

        public void ResolveAll<T>(IInjectTarget target, HashSet<T> services) where T : class {
            ThrowIfWrongState();

            var serviceType = typeof(T);
            IncrementDependencies(serviceType);

            if (_services.TryGetValue(serviceType, out var registrations)) {
                foreach (var registration in registrations) {
                    if (registration.CanBeInjectedInto(target.GetType()) == false) {
                        continue;
                    }

                    services.Add((T)registration.Instance);
                }
            }

            _parentContainer?.ResolveAll(target, services);
        }

        private object Resolve(Type type, Type constraint = null, Container initialContainer = null) {
            ThrowIfWrongState();

            if (initialContainer == null) {
                initialContainer = this;
            }

            var key = type;
            IncrementDependencies(key);

            if (type == typeof(Container)) {
                return this;
            }

            if (_services.TryGetValue(key, out var services) && services.Count != 0) {
                foreach (var registration in services) {
                    if (registration.CanBeInjectedInto(constraint)) {
                        return registration.Instance;
                    }
                }
            }

            if (_parentContainer != null) {
                return _parentContainer.Resolve(type, constraint, initialContainer);
            }

            throw new Exception($"Service {type.Name} not found for requester '{constraint?.FullName}' in '{initialContainer.Owner.GetType().FullName}' node container");
        }

        private void ResolveAll(Type targetType, object list, Type listElementType) {
            var iList = (System.Collections.IList)list;
            IncrementDependencies(listElementType);
            if (_services.TryGetValue(listElementType, out var regs)) {
                foreach (var r in regs) {
                    var inst = r.Instance;
                    if (!r.CanBeInjectedInto(targetType)) {
                        continue;
                    }

                    if (iList.Contains(inst)) {
                        continue;
                    }

                    iList.Add(inst);
                }
            }

            _parentContainer?.ResolveAll(targetType, list, listElementType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfWrongState() {
            if (_constructed == false) {
                throw new Exception("Error! Dependency injection can only be used after the 'OnConstruct()' stage!");
            }
        }

        private void IncrementDependencies(Type key) {
#if UNITY_EDITOR && !DISABLE_EDITOR_NODE_DEBUGGER
            if (_dependencies.TryAdd(key, 1) == false) {
                _dependencies[key]++;
            }

            Editor_OnDependenciesChanged();
#endif
        }

        public void Dispose() {
            _disposables.Dispose();
        }

        public class Registration {
            private Type[] _requesterWhitelist;
            public object Instance { get; }

            public Registration(object instance) {
                Instance = instance;
                _requesterWhitelist = null;
            }

            public Registration WhenInjectedInto(params Type[] requesterWhitelist) {
                _requesterWhitelist = requesterWhitelist;
                return this;
            }

            public bool CanBeInjectedInto(Type requester) {
                if (_requesterWhitelist == null || _requesterWhitelist.Length == 0) {
                    return true;
                }

                if (requester == null) {
                    return false;
                }

                foreach (var type in _requesterWhitelist) {
                    if (type.IsAssignableFrom(requester)) {
                        return true;
                    }
                }

                return false;
            }
        }

        #region Reflection Utils

        private static (List<FieldInfo>, List<(MethodInfo, ParameterInfo[])>) GetInjectMeta(Type type) {
            if (_getInjectMetaCache.TryGetValue(type, out var value)) {
                return value;
            }

            var fields = CollectFields(type);
            var methods = CollectMethods(type);
            _getInjectMetaCache[type] = (fields, methods);
            return (fields, methods);
        }

        private static List<FieldInfo> CollectFields(Type t) {
            if (_fieldCacheByType.TryGetValue(t, out var cachedFields)) {
                return cachedFields;
            }
            
            var concreteFields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            // Avoid resize of the list
            var fields = new List<FieldInfo>(concreteFields.Length);
            foreach (var fi in concreteFields) {
                if (HasAnyAttribute(fi, _injectAttributeTypes) == false) {
                    continue;
                }

                fields.Add(fi);
            }

            if (t.BaseType != null) {
                fields.AddRange(CollectFields(t.BaseType));
            }
            
            _fieldCacheByType[t] = fields;
            return fields;
        }

        private static List<(MethodInfo, ParameterInfo[])> CollectMethods(Type t) {
            if (_methodCacheByType.TryGetValue(t, out var cachedMethods)) {
                return cachedMethods;
            }
            
            var methods = new List<(MethodInfo, ParameterInfo[])>(4);
            var concreteMethods = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (var mi in concreteMethods) {
                if (HasAnyAttribute(mi, _injectAttributeTypes) == false) {
                    continue;
                }

                var parameters = mi.GetParameters();
                methods.Add((mi, parameters));
            }

            if (t.BaseType != null) {
                methods.AddRange(CollectMethods(t.BaseType));
            }

            _methodCacheByType[t] = methods;
            return methods;
        }

        private static bool HasAnyAttribute(MemberInfo member, params Type[] attributeTypes) {
            foreach (var attributeType in attributeTypes) {
                if (Attribute.IsDefined(member, attributeType, inherit: true)) {
                    return true;
                }
            }
            
            return false;
        }

        private static bool TryGetListType(Type type, out Type listElementType) {
            var isList = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
            if (isList) {
                listElementType = type.GenericTypeArguments[0];
                return true;
            }

            listElementType = null;
            return false;
        }

        #endregion

        #region Editor Debug

        public Dictionary<Type, List<object>> Editor_ServicesInfo() {
            var result = new Dictionary<Type, List<object>>();
            CollectServicesDebugInfo(Owner, result);
            return result;
        }

        private void CollectServicesDebugInfo(Node requester, Dictionary<Type, List<object>> dict) {
            foreach (var key in _services.Keys) {
                if (_services[key].Count == 0) {
                    continue;
                }

                foreach (var registration in _services[key]) {
                    if (registration.CanBeInjectedInto(requester.GetType()) == false) {
                        continue;
                    }

                    if (dict.ContainsKey(key) == false) {
                        dict[key] = new List<object>();
                    }

                    if (dict[key].Contains(registration.Instance)) {
                        continue;
                    }

                    dict[key].Add(registration.Instance);
                }
            }

            _parentContainer?.CollectServicesDebugInfo(requester, dict);
        }

        public Dictionary<Type, int> Editor_DependencyInfo() {
            var dictionary = new Dictionary<Type, int>(_dependencies.Count);
            foreach (var dependency in _dependencies) {
                dictionary[dependency.Key] = dependency.Value;
            }
            
            return dictionary;
        }

        #endregion
    }

    public interface IInjectTarget {}
}