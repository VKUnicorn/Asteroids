using Core.GameState;

namespace Nodes.State {
    public class LoaderState : BaseGameState {
        protected override void BuildActivationSchema() {
            ActivateNode<LoaderNode>(true);
        }

        protected override void BuildDeactivationSchema() {
            ActivateNode<LoaderNode>(false);
        }
    }
}
