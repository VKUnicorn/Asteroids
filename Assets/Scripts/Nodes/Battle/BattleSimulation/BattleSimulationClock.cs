namespace Nodes.Battle {
    public class BattleSimulationClock {
        public int Tick { get; private set; }
        public float Step { get; }
        public float Time => Tick * Step;

        public BattleSimulationClock(float step) {
            Step = step;
        }

        public void DoTick() => Tick++;
    }
}
