using System;

namespace Core.Tools {
    // Dispose is deliberately non-terminal: the scope is reused across its owner's dispose cycles.
    public sealed class StableLifetime : IManagedLifetime {
        private readonly Lifetime _inner = new();

        public int Count => _inner.Count;

        public void Register(IDisposable item) => _inner.Register(item);
        public void Unregister(IDisposable item) => _inner.Unregister(item);
        public void Clear() => _inner.Clear();
        public void Dispose() => _inner.Clear();
    }
}
