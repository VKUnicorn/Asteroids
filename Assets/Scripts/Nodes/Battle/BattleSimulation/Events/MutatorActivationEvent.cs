using Config;
using Morpeh;

namespace Nodes.Battle.Events {
    public class MutatorActivationEvent : IEvent {
        public Mutator Mutator;
        public float Power;
        public Entity TargetEntity;
    }
}
