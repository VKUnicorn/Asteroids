using Core;
using Cysharp.Threading.Tasks;
using Morpeh;
using UnityEngine;

namespace Nodes.Battle {
    public class BattleSimulationNode : Node {
        private BattleSimulation _simulation;
        private BattlePlayer _player;

        protected override void OnConstruct() { }

        protected override UniTask OnActivate() {
            var input = new BattleSimulationInput {
                World = World.Create(),
            };

            var context = new BattleSimulationContext(input, Time.fixedDeltaTime);
            _player = new BattlePlayer(context);
            _simulation = new BattleSimulation(context);
            return UniTask.CompletedTask;
        }

        public override void Update(float deltaTime, float unscaledDeltaTime) => _simulation.UpdateOutputs();

        public override void FixedUpdate(float fixedDeltaTime, float unscaledFixedDeltaTime) => _simulation.Simulate(1);

        protected override UniTask OnDeactivate() {
            _player.Dispose();
            _player = null;
            _simulation.Dispose();
            _simulation = null;
            return UniTask.CompletedTask;
        }
    }
}
