#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif

using Sirenix.OdinInspector;
using System;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Morpeh {
    using Il2Cpp = Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute;

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    [Serializable]
    public sealed class Entity {
        [NonSerialized]
        public World World;

        [SerializeField]
        internal int internalID;

        [SerializeField]
        internal bool isDirty;
        [SerializeField]
        internal bool isDisposed;

        [SerializeField]
        internal int previousArchetypeId;
        [SerializeField]
        internal int currentArchetypeId;

        [NonSerialized]
        internal Archetype currentArchetype;

        [ShowInInspector]
        public int ID => this.internalID;

        public Action<Entity> OnDispose;

        internal Entity() {
        }
    }
}