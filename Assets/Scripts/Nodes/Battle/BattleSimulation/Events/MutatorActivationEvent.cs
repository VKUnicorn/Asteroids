using Config;
using Morpeh;

namespace Nodes.Battle.Events {
    public class MutatorActivationEvent : IEvent {
        public IMutator Mutator;
        public float Power;
        public Entity TargetEntity;
    }
}
