#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif

using JetBrains.Annotations;
using Morpeh.Collections;
using Morpeh;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using Unity.IL2CPP.CompilerServices;

namespace Morpeh {
    using Il2Cpp = Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute;

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public static partial class WorldExtensions {
        public static IEnumerable<Type> RegisteredComponentTypes => CommonTypeIdentifier.typeAssociation.Keys;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Ctor(this World world) {
            world.Filter = new Filter(world);
            world.filters = new FastList<Filter>();
            world.archetypeCache = new IntFastList();
            world.dirtyEntities = new BitMap();

            if (world.archetypes != null) {
                foreach (var archetype in world.archetypes) {
                    archetype.Ctor();
                }
            }

            world.InitializeGlobals();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static partial void InitializeGlobals(this World world);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static World Initialize(this World world) {
            world.freeEntityIDs = new IntFastList();
            world.nextFreeEntityIDs = new IntFastList();
            world.typedCaches = new IntHashMap<ComponentsCache>(Constants.DEFAULT_WORLD_CACHES_CAPACITY);

            world.entitiesLength = 0;
            world.entitiesCapacity = Constants.DEFAULT_WORLD_ENTITIES_CAPACITY;
            world.entities = new Entity[world.entitiesCapacity];

            world.archetypes = new FastList<Archetype> { new Archetype(0, Array.Empty<int>(), world) };
            world.archetypesByLength = new IntHashMap<IntFastList>();
            world.archetypesByLength.Add(0, new IntFastList { 0 }, out _);
            world.newArchetypes = new IntFastList();

            return world;
        }

        //TODO refactor allocations and fast sort(maybe without it?)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Archetype GetArchetype(this World world, int[] typeIds, int newTypeId, bool added, out int archetypeId) {
            Archetype archetype = null;
            archetypeId = -1;

            world.archetypeCache.Clear();
            for (int i = 0, length = typeIds.Length; i < length; i++) {
                var typeId = typeIds[i];
                if (typeId >= 0) {
                    world.archetypeCache.Add(typeIds[i]);
                }
            }

            if (added) {
                world.archetypeCache.Add(newTypeId);
            }
            else {
                world.archetypeCache.Remove(newTypeId);
            }

            world.archetypeCache.Sort();
            var typesLength = world.archetypeCache.length;

            if (world.archetypesByLength.TryGetValue(typesLength, out var archsl)) {
                for (var index = 0; index < archsl.length; index++) {
                    archetypeId = archsl.Get(index);
                    archetype = world.archetypes.data[archetypeId];
                    var check = true;
                    for (int i = 0, length = typesLength; i < length; i++) {
                        if (archetype.typeIds[i] != world.archetypeCache.Get(i)) {
                            check = false;
                            break;
                        }
                    }

                    if (check) {
                        return archetype;
                    }
                }
            }

            archetypeId = world.archetypes.length;
            var newArchetype = new Archetype(archetypeId, world.archetypeCache.ToArray(), world);
            world.archetypes.Add(newArchetype);
            if (world.archetypesByLength.TryGetValue(typesLength, out archsl)) {
                archsl.Add(archetypeId);
            }
            else {
                world.archetypesByLength.Add(typesLength, new IntFastList { archetypeId }, out _);
            }

            world.newArchetypes.Add(archetypeId);

            archetype = newArchetype;

            return archetype;
        }

        public static ComponentsCache<T> GetCache<T>(this World world) where T : struct, IComponent {
            var info = TypeIdentifier<T>.Info;
            if (world.typedCaches.TryGetValue(info.id, out var typedCache)) {
                return typedCache as ComponentsCache<T>;
            }

            var componentsCache = new ComponentsCache<T>();
            world.typedCaches.Add(info.id, componentsCache, out _);
            return componentsCache;
        }

        public static Entity CreateEntity(this World world) {
            int id;
            if (world.freeEntityIDs.length > 0) {
                id = world.freeEntityIDs.Get(0);
                world.freeEntityIDs.RemoveAtSwap(0, out _);
            }
            else {
                id = world.entitiesLength++;
            }

            if (world.entitiesLength >= world.entitiesCapacity) {
                var newCapacity = HashHelpers.ExpandCapacity(world.entitiesCapacity);
                Array.Resize(ref world.entities, newCapacity);
                world.entitiesCapacity = newCapacity;
            }

            world.entities[id] = EntityExtensions.Create(id, world);
            ++world.entitiesCount;

            return world.entities[id];
        }

        public static Entity CreateEntity(this World world, out int id) {
            if (world.freeEntityIDs.length > 0) {
                id = world.freeEntityIDs.Get(0);
                world.freeEntityIDs.RemoveAtSwap(0, out _);
            }
            else {
                id = world.entitiesLength++;
            }

            if (world.entitiesLength >= world.entitiesCapacity) {
                var newCapacity = world.entitiesCapacity << 1;
                Array.Resize(ref world.entities, newCapacity);
                world.entitiesCapacity = newCapacity;
            }

            world.entities[id] = EntityExtensions.Create(id, world);
            ++world.entitiesCount;
            return world.entities[id];
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity GetEntity(this World world, in int id) => world.entities[id];

        public static void RemoveEntity(this World world, Entity entity) {
            var id = entity.ID;
            if (world.entities[id] == entity) {
                entity.Dispose();
            }
        }

        internal static void ApplyRemoveEntity(this World world, int id) {
            world.nextFreeEntityIDs.Add(id);
            world.entities[id] = null;
            --world.entitiesCount;
        }

        public static void UpdateFilters(this World world) {
            foreach (var entityId in world.dirtyEntities) {
                world.entities[entityId]?.ApplyTransfer();
            }

            world.dirtyEntities.Clear();

            if (world.newArchetypes.length > 0) {
                for (var index = 0; index < world.filters.length; index++) {
                    world.filters.data[index].FindArchetypes(world.newArchetypes);
                }

                world.newArchetypes.Clear();
            }

            for (var index = 0; index < world.archetypes.length; index++) {
                var archetype = world.archetypes.data[index];
                if (archetype.isDirty) {
                    archetype.Process();
                }
            }

            for (int index = 0, length = world.filters.length; index < length; index++) {
                var filter = world.filters.data[index];
                if (filter.isDirty) {
                    filter.UpdateLength();
                }
            }

            if (world.nextFreeEntityIDs.length > 0) {
                world.freeEntityIDs.AddListRange(world.nextFreeEntityIDs);
                world.nextFreeEntityIDs.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsNullOrDisposed([CanBeNull] this World world) => world == null || world.isDisposed;
    }
}