
using Core;
using Core.GameState;
using Core.UI;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using R3;
using UnityEngine;

namespace Nodes {
    public class RootNode : RootUpdaterNode {
        private readonly AppCtx _ctx;
        private bool _isReloading;

        public bool IsLoaded { get; private set; }

        public RootNode(AppCtx ctx) {
            _ctx = ctx;
        }

        protected override void OnConstruct() {
            AddNode(new GameStateManagerNode(this));
            AddNode(new UIManagerNode(_ctx.UIRoot));
        }

        protected override void OnAfterActivate() {
            ReloadGameAsync().Forget();
        }

        public async UniTask ReloadGameAsync() {
            if (_isReloading) {
                Debug.LogWarning("[Root] Reload already in progress, request dropped");
                return;
            }

            _isReloading = true;
            try {
                await UniTask.Yield();

                /*
                await AddNode(_game = new GameNode(_ctx)).InitNodes();
                state.Reset<LoaderState>();
                */
                IsLoaded = true;
            }
            finally {
                _isReloading = false;
            }
        }
    }
}