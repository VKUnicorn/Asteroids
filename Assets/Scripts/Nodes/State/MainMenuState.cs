using Core.GameState;
using Nodes.UI.Controller;

namespace Nodes.State {
    public class MainMenuState : BaseGameState {
        protected override void BuildActivationSchema() {
            ActivateUI<UIMainMenuController>(true);
        }

        protected override void BuildDeactivationSchema() {
            ActivateUI<UIMainMenuController>(false);
        }
    }
}
