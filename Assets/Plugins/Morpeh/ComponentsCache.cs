#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif

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
    public abstract class ComponentsCache : IDisposable {
        internal int typeId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool RemoveComponent(Entity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract bool Clean(Entity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool Has(Entity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void CopyComponent(Entity from, Entity to, bool overwrite = true);

        public abstract void Dispose();
    }

    [Serializable]
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public sealed class ComponentsCache<T> : ComponentsCache where T : struct, IComponent {
        public IntHashMap<T> Components { private set; get; }

        // Invoking the empty IComponent default Dispose on a struct boxes it, so it is skipped unless the component defines its own.
        private readonly bool hasDispose;

        internal ComponentsCache() {
            var info = TypeIdentifier<T>.Info;
            this.typeId = info.id;
            this.hasDispose = info.hasDispose;
            this.Components = new IntHashMap<T>(Math.Max(1, info.cacheSize - 1));
            this.Components.Add(-1, default, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent(Entity entity) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(AddComponent)} on null or disposed entity {entity.internalID}");
            }
#endif
            if (this.Components.Add(entity.internalID, default, out var slotIndex)) {
                entity.AddTransfer(this.typeId);
                return ref this.Components.data[slotIndex];
            }
#if MORPEH_DEBUG
            Debug.LogError($"[MORPEH] You're trying to {nameof(AddComponent)} on entity {entity.internalID} a component that already exists! Use {nameof(GetComponent)} or {nameof(SetComponent)} instead!");
#endif
            return ref this.Components.data[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent(Entity entity, out bool exist) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(AddComponent)} on null or disposed entity {entity.internalID}");
            }
#endif
            if (this.Components.Add(entity.internalID, default, out var slotIndex)) {
                entity.AddTransfer(this.typeId);
                exist = false;
                return ref this.Components.data[slotIndex];
            }

            exist = true;
            return ref this.Components.data[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddComponent(Entity entity, in T value) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(AddComponent)} on null or disposed entity {entity.internalID}");
            }
#endif
            if (this.Components.Add(entity.internalID, value, out _)) {
                entity.AddTransfer(this.typeId);
                return true;
            }

#if MORPEH_DEBUG
            Debug.LogError($"[MORPEH] You're trying to {nameof(AddComponent)} on entity {entity.internalID} a component that already exists! Use {nameof(GetComponent)} or {nameof(SetComponent)} instead!");
#endif
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent(Entity entity) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(GetComponent)} on null or disposed entity {entity.internalID}");
            }

            if (!this.Components.Has(entity.internalID)) {
                throw new Exception($"[MORPEH] You're trying to {nameof(GetComponent)} on entity {entity.internalID} a component that doesn't exists!");
            }
#endif
            return ref this.Components.GetValueRefByKey(entity.internalID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T TryGetComponent(Entity entity, out bool exist) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(TryGetComponent)} on null or disposed entity {entity.internalID}");
            }
#endif
            return ref this.Components.TryGetValueRefByKey(entity.internalID, out exist);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent(Entity entity, in T value) {
            SetComponent(entity, value, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent(Entity entity, in T value, out int slotIndex) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(SetComponent)} on null or disposed entity {entity.internalID}");
            }
#endif

            if (this.Components.Set(entity.internalID, value, out slotIndex)) {
                entity.AddTransfer(this.typeId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T Empty() => ref this.Components.data[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool RemoveComponent(Entity entity) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(RemoveComponent)} on null or disposed entity {entity.internalID}");
            }
#endif

            if (this.Components.Remove(entity.internalID, out var lastValue)) {
                entity.RemoveTransfer(this.typeId);
                if (this.hasDispose) {
                    lastValue.Dispose();
                }
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override bool Clean(Entity entity) {
            if (this.Components.Remove(entity.internalID, out var lastValue)) {
                if (this.hasDispose) {
                    lastValue.Dispose();
                }
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Has(Entity entity) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(Has)} on null or disposed entity {entity.internalID}");
            }
#endif
            return this.Components.Has(entity.internalID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CopyComponent(Entity from, Entity to, bool overwrite = true) {
#if MORPEH_DEBUG
            if (from.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(CopyComponent)} FROM null or disposed entity {from.internalID}");
            }
            if (to.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying {nameof(CopyComponent)} TO null or disposed entity {to.internalID}");
            }
#endif

            if (this.Components.TryGetValue(from.internalID, out var component)) {
                if (overwrite) {
                    if (this.Components.Has(to.internalID)) {
                        this.Components.Set(to.internalID, component, out _);
                    }
                    else {
                        this.Components.Add(to.internalID, component, out _);
                        to.AddTransfer(this.typeId);
                    }
                }
                else {
                    if (this.Components.Has(to.internalID) == false) {
                        this.Components.Add(to.internalID, component, out _);
                        to.AddTransfer(this.typeId);
                    }
                }
            }
        }

        // LOCAL PATCH (keep on plugin update): data is indexed by slot up to lastIndex, while length only
        // counts live records, so walking to length disposed freed slots and skipped the live tail.
        // Same bounds as the map enumerator.
        public override void Dispose() {
            var components = this.Components;
            if (this.hasDispose) {
                for (int i = 0, disposed = 0; i < components.lastIndex && disposed < components.length; i++) {
                    if (components.slots[i].key - 1 < 0) {
                        continue;
                    }

                    components.data[i].Dispose();
                    ++disposed;
                }
            }

            this.Components = null;
        }
    }
}