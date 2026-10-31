using System;
using Core.Tools.Singletons;
using UnityEngine;

namespace Core.Tools.UnityHelpers {
    public class UnityRuntimeHelper : MonoBehaviourSingleton<UnityRuntimeHelper>, IUnityLoopProvider, IKeepAliveMonoBehaviourSingleton {
        public event Action OnUpdate = delegate {  };
        public event Action OnFixedUpdate = delegate {  };
        public event Action OnGui = delegate {  };
        public event Action<bool> OnApplicationPauseEvent = delegate { };
        public event Action<bool> OnApplicationFocusEvent = delegate { };
        public event Action OnApplicationQuitEvent = delegate { };

        public bool IsPaused { get; private set; }
        public bool HasFocus { get; private set; } = true;

        protected override void Awake() {
            HasFocus = Application.isFocused;
            base.Awake();
        }

        private void Update() => OnUpdate.Invoke();
        private void FixedUpdate() => OnFixedUpdate.Invoke();
        private void OnGUI() => OnGui.Invoke();

        private void OnApplicationPause(bool pause) {
            IsPaused = pause;
            OnApplicationPauseEvent.Invoke(pause);
        }

        private void OnApplicationFocus(bool focus) {
            HasFocus = focus;
            OnApplicationFocusEvent.Invoke(focus);
        }

        protected override void OnApplicationQuit() {
            OnApplicationQuitEvent?.Invoke();
            base.OnApplicationQuit();
        }
    }
}
