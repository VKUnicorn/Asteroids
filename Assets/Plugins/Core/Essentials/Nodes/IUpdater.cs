using System;

namespace Core {
    public interface IUpdater {
        IDisposable SubscribeUpdate(IUpdatable u);
        IDisposable SubscribeFixedUpdate(IFixedUpdatable u);
        
        void RegisterUpdate(IUpdatable u);
        void RegisterFixedUpdate(IFixedUpdatable u);
        bool UnregisterUpdate(IUpdatable u);
        bool UnregisterFixedUpdate(IFixedUpdatable u);
    }
}