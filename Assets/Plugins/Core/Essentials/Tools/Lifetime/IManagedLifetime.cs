using System;

namespace Core.Tools {
    public interface IManagedLifetime : ILifetime, IDisposable {
        void Clear();
    }
}