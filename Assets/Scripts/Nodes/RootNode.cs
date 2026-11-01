
using Core;
using Core.Attributes;
using Core.GameState;
using Core.UI;
using Cysharp.Threading.Tasks;
using Nodes.Battle;
using Nodes.State;
using Sirenix.OdinInspector;
using R3;
using UnityEngine;

namespace Nodes {
    public class RootNode : RootUpdaterNode {
        [Inject] private IGameStateModel _state;

        private readonly AppCtx _ctx;
        private bool _isReloading;

        public bool IsLoaded { get; private set; }

        public RootNode(AppCtx ctx) {
            _ctx = ctx;
        }

        protected override void OnConstruct() {
            AddNode(new GameStateManagerNode(this));
            AddNode(new UIManagerNode(_ctx.UIRoot));
            AddNode(new LoaderNode(), false);
            AddNode(new BattleSimulationNode(), false);
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

                // await AddNode(_game = new GameNode(_ctx)).InitNodes();
                _state.Reset<LoaderState>();
                IsLoaded = true;
            }
            finally {
                _isReloading = false;
            }
        }
    }
}