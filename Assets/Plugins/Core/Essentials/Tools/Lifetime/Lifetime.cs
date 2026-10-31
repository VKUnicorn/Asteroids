using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;

namespace Core.Tools {
    public sealed class Lifetime : IManagedLifetime {
        public static ILifetime Eternal { get; } = new EternalLifetime();
        public static ILifetime Disposed { get; } = new DisposedLifetime();

        private readonly object _gate = new ();
        private List<IDisposable> _items;
        private bool _disposed;

        public Lifetime(int initialCapacity = 0) {
            _items = new List<IDisposable>(initialCapacity);
        }

        public bool IsDisposed => Volatile.Read(ref _disposed);

        public int Count {
            get {
                lock (_gate) {
                    return _items?.Count ?? 0;
                }
            }
        }

        public void Register(IDisposable item) {
            if (item == null) {
                throw new ArgumentNullException(nameof(item));
            }

            lock (_gate) {
                if (!_disposed) {
                    _items.Add(item);
                    return;
                }
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogError($"Disposed lifetime was used to register an '{item.GetType().Name}' item. Item will be disposed immediately.");
#endif
            item.Dispose();
        }

        public void Unregister(IDisposable item) {
            lock (_gate) {
                if (!_disposed) {
                    _items.Remove(item);
                }
            }
        }

        // Items are disposed outside the gate: a disposing item may call back into
        // Register/Unregister/Clear (ManagedAsset does).
        public void Clear() {
            IDisposable[] snapshot;
            int count;
            lock (_gate) {
                if (_disposed) {
                    return;
                }

                count = _items.Count;
                if (count == 0) {
                    return;
                }

                snapshot = ArrayPool<IDisposable>.Shared.Rent(count);
                _items.CopyTo(snapshot);
                _items.Clear();
            }

            try {
                for (var i = 0; i < count; i++) {
                    DisposeIsolated(snapshot[i]);
                }
            }
            finally {
                ArrayPool<IDisposable>.Shared.Return(snapshot, clearArray: true);
            }
        }

        public void Dispose() {
            List<IDisposable> items;
            lock (_gate) {
                if (_disposed) {
                    return;
                }

                _disposed = true;
                items = _items;
                _items = null;
            }

            foreach (var item in items) {
                DisposeIsolated(item);
            }

            items.Clear();
        }

        // One throwing Dispose must not leak the items after it.
        private static void DisposeIsolated(IDisposable item) {
            try {
                item.Dispose();
            }
            catch (Exception e) {
                UnityEngine.Debug.LogException(e);
            }
        }

        private sealed class EternalLifetime : ILifetime {
            public void Register(IDisposable item) {}
            public void Unregister(IDisposable item) {}
        }

        private sealed class DisposedLifetime : ILifetime {
            public void Register(IDisposable item) {
                item.Dispose();
            }

            public void Unregister(IDisposable item) {}
        }
    }
}
