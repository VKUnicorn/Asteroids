using System;
using Cysharp.Threading.Tasks;

namespace Core.Tools.UnityHelpers {
    /// <summary>
    /// Currently used only for tests
    /// </summary>
    public class UniTaskRuntimeHelper : IUnityLoopProvider, IDisposable {
        public event Action OnUpdate = delegate {  };
        public event Action OnFixedUpdate = delegate {  };
        public event Action OnGui = delegate {  };
        public event Action<bool> OnApplicationPauseEvent = delegate { };
        public event Action<bool> OnApplicationFocusEvent = delegate { };
        public event Action OnApplicationQuitEvent = delegate { };
        
        private readonly PlayerLoopItem _updateAction;
        private readonly PlayerLoopItem _updateGuiAction;
        private readonly PlayerLoopItem _fixedUpdateAction;

        public UniTaskRuntimeHelper() {
            PlayerLoopHelper.AddAction(PlayerLoopTiming.Update, _updateAction = new PlayerLoopItem(() => OnUpdate()));
            PlayerLoopHelper.AddAction(PlayerLoopTiming.Update, _updateGuiAction = new PlayerLoopItem(() => OnGui()));
            PlayerLoopHelper.AddAction(PlayerLoopTiming.FixedUpdate, _fixedUpdateAction = new PlayerLoopItem(() => OnFixedUpdate()));
        }

        public void Dispose() {
            _updateAction.Dispose();
            _updateGuiAction.Dispose();
            _fixedUpdateAction.Dispose();
        }

        private class PlayerLoopItem : IPlayerLoopItem, IDisposable {
            private readonly Action _action;
            private bool _disposed;

            public PlayerLoopItem(Action action) {
                _action = action;
            }

            public bool MoveNext() {
                if (_disposed) {
                    return false;
                }

                _action.Invoke();
                return true;
            }

            public void Dispose() {
                _disposed = true;
            }
        }
    }
}