using System;
using System.Collections.Generic;

namespace Core {
    public abstract class MultiFacadeNode : FacadeNode {
        private readonly Dictionary<Type, object> _facades = new Dictionary<Type, object>();
        
        public sealed override IEnumerable<(Type, object)> Facades {
            get {
                if (_facades.Count == 0) {
                    throw new Exception($"Facade of node {GetType().Name} is not registered. Use {nameof(RegisterInParent)}(facade)");
                }
                foreach (var typeObjectPair in _facades) {
                    yield return (typeObjectPair.Key, typeObjectPair.Value);
                }
            }
        }

        protected void RegisterInParent<T>(T facade) where T : class {
            _facades[typeof(T)] = facade;
        }

        protected void RegisterInParent(object facade, Type type) {
            _facades[type] = facade;
        }

        protected void RegisterInParentWithInterfaces(object facade) {
            RegisterInParent(facade, facade.GetType());
            foreach (var i in facade.GetType().GetInterfaces()) {
                RegisterInParent(facade, i);
            }
        }
    }
}