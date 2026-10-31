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

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public static class EntityExtensions {
        internal static Entity Create(int id, World world) {
            var newEntity = new Entity { internalID = id };

            newEntity.World = world;

            newEntity.previousArchetypeId = -1;
            newEntity.currentArchetypeId = 0;

            newEntity.currentArchetype = newEntity.World.archetypes.data[0];

            return newEntity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity AddComponentIf<T1, T2>(this Entity entity, bool useFirstType) where T1 : struct, IComponent where T2 : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(AddComponentIf)} on null or disposed entity");
            }
#endif
            if (useFirstType) {
                var cache = entity.World.GetCache<T1>();
                cache.AddComponent(entity);
            }
            else {
                var cache = entity.World.GetCache<T2>();
                cache.AddComponent(entity);
            }

            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddDefaultComponent(this Entity entity, Type type) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(AddDefaultComponent)} on null or disposed entity");
            }
#endif
            if (!CommonTypeIdentifier.typeAssociation.TryGetValue(type, out var typeDefinition)) {
                throw new Exception($"[MORPEH] You are trying {nameof(AddDefaultComponent)} of unknown type");
            }

            typeDefinition.entityAddDefaultObject(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddComponent<T>(this Entity entity) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(AddComponent)} on null or disposed entity");
            }
#endif
            var cache = entity.World.GetCache<T>();

            return ref cache.AddComponent(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddComponent<T>(this Entity entity, out bool exist) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(AddComponent)} on null or disposed entity");
            }
#endif
            var cache = entity.World.GetCache<T>();

            return ref cache.AddComponent(entity, out exist);
        }

        public static ref T GetComponent<T>(this Entity entity) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(GetComponent)} on null or disposed entity");
            }
#endif
            var cache = entity.World.GetCache<T>();

            return ref cache.GetComponent(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetComponent<T>(this Entity entity, out bool exist) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(GetComponent)} on null or disposed entity");
            }
#endif
            var cache = entity.World.GetCache<T>();

            return ref cache.TryGetComponent(entity, out exist);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T EnsureComponent<T>(this Entity entity) where T : struct, IComponent {
            if (!Has<T>(entity)) {
                return ref entity.AddComponent<T>();
            }

            return ref entity.GetComponent<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetComponent<T>(this Entity entity, in T value) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(SetComponent)} on null or disposed entity");
            }
#endif
            var cache = entity.World.GetCache<T>();

            cache.SetComponent(entity, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponent(this Entity entity, Type type) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(RemoveComponent)} on null or disposed entity");
            }
#endif
            if (!CommonTypeIdentifier.typeAssociation.TryGetValue(type, out var typeDefinition)) {
                throw new Exception($"[MORPEH] You are trying {nameof(RemoveComponent)} of unknown type");
            }

            typeDefinition.entityRemoveComponent(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveComponent<T>(this Entity entity) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(RemoveComponent)} on null or disposed entity");
            }
#endif
            var cache = entity.World.GetCache<T>();

            return cache.RemoveComponent(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(this Entity entity) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(Has)} on null or disposed entity");
            }
#endif

            var cache = entity.World.GetCache<T>();
            return cache.Has(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo(this Entity from, Entity to, bool overwrite = true) {
#if MORPEH_DEBUG
            if (from.IsNullOrDisposed() || to.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(CopyTo)} on null or disposed entities");
            }
#endif

            foreach (var typeId in from.currentArchetype.typeIds) {
                if (from.World.typedCaches.TryGetValue(typeId, out var typedCache)) {
                    typedCache.CopyComponent(from, to, overwrite);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyIfExistTo(this Entity from, Entity to, Type type, bool overwrite = true) {
#if MORPEH_DEBUG
            if (from.IsNullOrDisposed() || to.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(CopyIfExistTo)} on null or disposed entities");
            }
#endif
            if (!CommonTypeIdentifier.typeAssociation.TryGetValue(type, out var descriptor)) {
                throw new Exception($"[MORPEH] You are trying {nameof(CopyIfExistTo)} unknown Type");
            }

            if (!from.World.typedCaches.TryGetValue(descriptor.id, out var typedCache)) {
                return;
            }

            typedCache.CopyComponent(from, to, overwrite);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddTransfer(this Entity entity, int typeId) {
            if (entity.previousArchetypeId == -1) {
                entity.previousArchetypeId = entity.currentArchetypeId;
            }

            entity.currentArchetype.AddTransfer(typeId, out entity.currentArchetypeId, out entity.currentArchetype);
            if (entity.isDirty == true) {
                return;
            }

            entity.World.dirtyEntities.Set(entity.internalID);
            entity.isDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveTransfer(this Entity entity, int typeId) {
            if (entity.previousArchetypeId == -1) {
                entity.previousArchetypeId = entity.currentArchetypeId;
            }

            entity.currentArchetype.RemoveTransfer(typeId, out entity.currentArchetypeId, out entity.currentArchetype);
            if (entity.isDirty == true) {
                return;
            }

            entity.World.dirtyEntities.Set(entity.internalID);
            entity.isDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ApplyTransfer(this Entity entity) {
            if (entity.currentArchetypeId == 0) {
                entity.World.RemoveEntity(entity);
            }

            if (entity.previousArchetypeId != entity.currentArchetypeId) {
                if (entity.previousArchetypeId >= 0) {
                    entity.World.archetypes.data[entity.previousArchetypeId].Remove(entity);
                }

                if (entity.isDisposed) {
                    entity.DisposeInternal();
                }
                else {
                    entity.currentArchetype.Add(entity);
                }
                entity.previousArchetypeId = -1;
            }

            entity.isDirty = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Dispose(this Entity entity) {
            if (entity.isDisposed) {
#if MORPEH_DEBUG
                Debug.LogError($"[MORPEH] You're trying to {nameof(Dispose)} disposed entity.");
#endif
                return;
            }

            try {
                entity.OnDispose?.Invoke(entity);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
            finally {
                entity.OnDispose = null;
            }

            var archetype = entity.currentArchetype;
            var caches = archetype.world.typedCaches;
            foreach (var typeId in archetype.typeIds) {
                if (caches.TryGetValue(typeId, out var cache)) {
                    cache.Clean(entity);
                }
            }

            if (entity.previousArchetypeId < 0) {
                entity.previousArchetypeId = entity.currentArchetypeId;
            }

            entity.currentArchetypeId = -1;

            if (entity.isDirty == false) {
                entity.World.dirtyEntities.Set(entity.internalID);
                entity.isDirty = true;
            }

            entity.isDisposed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void DisposeInternal(this Entity entity) {
            entity.World.ApplyRemoveEntity(entity.internalID);

            entity.DisposeFast();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void DisposeFast(this Entity entity) {
            entity.previousArchetypeId = -1;
            entity.currentArchetypeId = -1;

            entity.World = null;
            entity.currentArchetype = null;

            entity.internalID = -1;

            entity.isDirty = false;
            entity.isDisposed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDisposed([NotNull] this Entity entity) => entity.isDisposed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrDisposed([CanBeNull] this Entity entity) => entity == null || entity.isDisposed;
    }
}