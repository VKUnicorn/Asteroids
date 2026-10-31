#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Scripting;

namespace Morpeh {
    using Il2Cpp = Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute;

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    internal static class CommonTypeIdentifier {
        private static int counter;

        internal static Dictionary<int, InternalTypeDefinition> intTypeAssociation = new Dictionary<int, InternalTypeDefinition>();
        internal static Dictionary<Type, InternalTypeDefinition> typeAssociation = new Dictionary<Type, InternalTypeDefinition>();

#pragma warning disable 0612
        internal static InternalTypeDefinition Register<T>(int cacheSize, bool hasDispose) where T : struct, IComponent {
            var info = new InternalTypeDefinition {
                id = Interlocked.Increment(ref counter),
                type = typeof(T),
                cacheSize = cacheSize,
                hasDispose = hasDispose,
                entityAddDefaultObject = (entity) => entity.AddComponent<T>(),
                entityRemoveComponent = (entity) => entity.RemoveComponent<T>()
            };
            intTypeAssociation.Add(info.id, info);
            typeAssociation.Add(info.type, info);
            return info;
        }
#pragma warning restore 0612

        internal class InternalTypeDefinition {
            public int id;
            public Type type;
            public int cacheSize;
            public bool hasDispose;
            public Action<Entity> entityAddDefaultObject;
            public Action<Entity> entityRemoveComponent;
        }
    }

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    [Preserve]
    public static class TypeIdentifier<T> where T : struct, IComponent {
        internal static CommonTypeIdentifier.InternalTypeDefinition info;

        internal static CommonTypeIdentifier.InternalTypeDefinition Info {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (info == null) {
                    throw new Exception($"[MORPEH] component {typeof(T)} is not registered");
                }
                return info;
            }
        }

        public static void Register() => Register(Constants.DEFAULT_CACHE_COMPONENTS_CAPACITY, true);

        public static void Register(int cacheSize) => Register(cacheSize, true);

        public static void Register(bool hasDispose) => Register(Constants.DEFAULT_CACHE_COMPONENTS_CAPACITY, hasDispose);

        public static void Register(int cacheSize, bool hasDispose) {
            if (info != null) {
                return;
            }

            info = CommonTypeIdentifier.Register<T>(cacheSize, hasDispose);
        }
    }
}