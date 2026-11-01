using System;
using Config;
using Core;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Nodes {
    [Serializable]
    public class AppCtx {
        public Transform UIRoot;
        public GameConfig Config;
        public Camera UICamera;
        public Camera WorldCamera;
        /*
        [Required] public WorldRoot WorldRoot;
        */
    }

    public class App : MonoBehaviour {
        private static App _instance;
        public static App Instance => _instance;

        public static bool IsQuitting { get; private set; }

        [SerializeField] private AppCtx Ctx;
        private static RootNode _root;

        private void Start() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnEditorApplicationOnplayModeStateChanged;
#endif
            _instance = this;

            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            ReloadAsync().Forget();
        }

        public async UniTask ReloadAsync() {
            if (_root != null) {
                await _root.DisposeAsync();
                await UniTask.Yield();
            }

            _root = new RootNode(Ctx);
            _root.InitNodes().Forget();
        }

        private void OnApplicationQuit() => IsQuitting = true;

#if UNITY_EDITOR
        private void OnEditorApplicationOnplayModeStateChanged(UnityEditor.PlayModeStateChange state) {
            IsQuitting = state != UnityEditor.PlayModeStateChange.EnteredPlayMode;
        }
#endif

#if UNITY_EDITOR
        // ReSharper disable once AsyncVoidMethod
        private async void OnDestroy() {
            UnityEditor.EditorApplication.playModeStateChanged -= OnEditorApplicationOnplayModeStateChanged;
            await _root.DisposeAsync();
            await UniTask.Yield();
        }
#endif // UNITY_EDITOR
    }
}