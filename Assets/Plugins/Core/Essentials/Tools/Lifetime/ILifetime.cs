using System;

namespace Core.Tools {
    // Register on a dead lifetime must dispose the item immediately (async tails acquire after
    // the owner cleared), so registered items must tolerate a re-entrant Dispose.
    // The dead state itself is optional — an implementation may stay alive forever (StableLifetime).
    // Unregister detaches without disposing.
    public interface ILifetime {
        void Register(IDisposable item);
        void Unregister(IDisposable item);
    }
}