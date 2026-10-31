using System;

namespace Core.Tools.UnityHelpers {
    public interface IUnityLoopProvider {
        event Action OnUpdate;
        event Action OnFixedUpdate;
        event Action OnGui;
        event Action<bool> OnApplicationPauseEvent;
        event Action<bool> OnApplicationFocusEvent;
        event Action OnApplicationQuitEvent;
    }
}