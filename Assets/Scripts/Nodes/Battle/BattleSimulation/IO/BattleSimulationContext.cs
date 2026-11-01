using System;

namespace Nodes.Battle {
    public class BattleSimulationContext : IDisposable {
        public readonly BattleSimulationInput Input;
        public readonly BattleSimulationOutput Output;
        public readonly BattleSimulationCache Cache;
        public readonly BattleSimulationClock Clock;

        public BattleSimulationContext(BattleSimulationInput input, float step) {
            Input = input;
            Output = new BattleSimulationOutput();
            Cache = new BattleSimulationCache();
            Clock = new BattleSimulationClock(step);
        }

        public void Dispose() => Input.World.Dispose();
    }
}
