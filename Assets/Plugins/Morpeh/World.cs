#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif

using JetBrains.Annotations;
using Morpeh.Collections;
using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Morpeh {
    using Il2Cpp = Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute;

    [Serializable]
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public sealed class World : IDisposable {
        public Filter Filter;
        internal bool isDisposed;
        internal FastList<Filter> filters;

        internal Entity[] entities;
        internal int entitiesCount;
        internal int entitiesLength;
        internal int entitiesCapacity;

        internal BitMap dirtyEntities;

        internal IntFastList freeEntityIDs;
        internal IntFastList nextFreeEntityIDs;

        internal IntHashMap<ComponentsCache> typedCaches;

        internal FastList<Archetype> archetypes;
        internal IntHashMap<IntFastList> archetypesByLength;
        internal IntFastList newArchetypes;
        internal IntFastList archetypeCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static World Create() => new World().Initialize();

        private World() => this.Ctor();

        //todo rework defines to conditionals
        public void Dispose() {
            isDisposed = true;
            foreach (var entity in this.entities) {
#if MORPEH_DEBUG
                try {
#endif
                entity?.DisposeFast();
#if MORPEH_DEBUG
                }
                catch (Exception e) {
                    Debug.LogError($"[MORPEH] Can not dispose entity with ID {entity?.ID}");
                    Debug.LogException(e);
                }
#endif
            }

            this.entities = null;
            this.entitiesLength = -1;
            this.entitiesCapacity = -1;

            this.freeEntityIDs.Clear();
            this.freeEntityIDs = null;
            this.nextFreeEntityIDs.Clear();
            this.nextFreeEntityIDs = null;
#if MORPEH_DEBUG
            try {
#endif
            this.Filter.Dispose();
#if MORPEH_DEBUG
            }
            catch (Exception e) {
                Debug.LogError("[MORPEH] Can not dispose root filter");
                Debug.LogException(e);
            }
#endif
            this.Filter = null;

            this.filters.Clear();
            this.filters = null;

            foreach (var cache in this.typedCaches) {
#if MORPEH_DEBUG
                try {
#endif
                typedCaches.GetValueByIndex(cache).Dispose();
#if MORPEH_DEBUG
                }
                catch (Exception e) {
                    Debug.LogError($"[MORPEH] Can not dispose cache id {cache}");
                    Debug.LogException(e);
                }
#endif
            }

            this.typedCaches.Clear();
            this.typedCaches.Dispose();
            this.typedCaches = null;

            foreach (var archetype in this.archetypes) {
#if MORPEH_DEBUG
                try {
#endif
                archetype.Dispose();
#if MORPEH_DEBUG
                }
                catch (Exception e) {
                    Debug.LogError($"[MORPEH] Can not dispose archetype id {archetype.id}");
                    Debug.LogException(e);
                }
#endif
            }

            this.archetypes.Clear();
            this.archetypes = null;

            foreach (var index in this.archetypesByLength) {
                this.archetypesByLength.GetValueByIndex(index).Clear();
            }

            this.archetypesByLength.Clear();
            this.archetypesByLength.Dispose();
            this.archetypesByLength = null;

            this.newArchetypes.Clear();
            this.newArchetypes = null;
        }
    }
}