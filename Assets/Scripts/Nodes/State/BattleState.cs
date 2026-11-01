using Core.GameState;
using Nodes.Battle;
using Nodes.UI.Controller;

namespace Nodes.State {
    public class BattleState : BaseGameState {
        protected override void BuildActivationSchema() {
            ActivateNode<BattleSimulationNode>(true);
            ActivateUI<UIBattleController>(true);
        }

        protected override void BuildDeactivationSchema() {
            ActivateUI<UIBattleController>(false);
            ActivateNode<BattleSimulationNode>(false);
        }
    }
}
