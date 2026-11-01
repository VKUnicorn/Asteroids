using Core;
using Core.Attributes;
using Core.GameState;
using Cysharp.Threading.Tasks;
using Nodes.State;

namespace Nodes {
    public class LoaderNode : Node {
        [Inject] private IGameStateModel _state;

        protected override void OnConstruct() { }

        protected override void OnAfterActivate() => LoadAsync().Forget();

        private async UniTask LoadAsync() {
            // Loading steps go here
            await LoadNextSceneAsync();
        }

        private UniTask LoadNextSceneAsync() {
            _state.Reset<MainMenuState>();
            return UniTask.CompletedTask;
        }
    }
}
