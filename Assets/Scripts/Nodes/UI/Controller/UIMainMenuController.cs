using Core.Attributes;
using Core.GameState;
using Core.Tools;
using Core.UI;
using Nodes.State;
using Nodes.UI.Model;
using R3;

namespace Nodes.UI.Controller {
    public class UIMainMenuController : UIBaseController<UIMainMenuModel> {
        [Inject] private IGameStateModel _state;

        protected override void OnModelSet() {
            Model.Play.Subscribe(_ => {
                if (_state.HasActiveTransition) {
                    return;
                }

                _state.Set<BattleState>();
            }).AddTo(Lifetime);
        }
    }
}
