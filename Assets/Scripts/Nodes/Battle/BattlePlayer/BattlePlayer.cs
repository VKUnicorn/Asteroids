using System;

namespace Nodes.Battle {
    /// <summary>
    /// View part of the battle, driven by the simulation output
    /// </summary>
    public class BattlePlayer : IDisposable {
        private readonly BattleSimulationContext _context;

        public BattlePlayer(BattleSimulationContext context) {
            _context = context;
        }

        public void Dispose() { }
    }
}
