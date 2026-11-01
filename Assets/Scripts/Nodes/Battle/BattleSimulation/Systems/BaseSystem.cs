using Morpeh;

namespace Nodes.Battle.Systems {
    public abstract class BaseSystem {
        protected BattleSimulationContext Context { get; private set; }
        protected World World => Context.Input.World;

        public void Initialize(BattleSimulationContext context) {
            Context = context;
            OnInitialize();
        }

        protected virtual void OnInitialize() { }
        public virtual void OnUpdate() { }
    }
}
