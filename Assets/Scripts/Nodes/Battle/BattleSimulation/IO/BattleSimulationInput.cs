using Config;
using Morpeh;

namespace Nodes.Battle {
    public class BattleSimulationInput {
        public World World { get; set; }
        public WorldEvents Events { get; set; }
        public IGameConfig GameConfig { get; set; }
    }
}
