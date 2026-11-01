using Config;
using Morpeh;
using Nodes.Battle.Components;
using Nodes.Battle.Events;

namespace Nodes.Battle.Systems {
    /// <summary>
    /// Applies stat mutators to the target unit stats
    /// </summary>
    public class UnitStatsModifierUpdateSystem : BaseSystem {
        public override void OnUpdate() {
            foreach (var @event in Events.Get<MutatorActivationEvent>()) {
                if (@event.Mutator is not UnitStatModifier modifier || @event.TargetEntity.IsNullOrDisposed()) {
                    continue;
                }

                ref var stats = ref @event.TargetEntity.GetComponent<UnitStatsComponent>(out var exist);
                if (exist) {
                    stats.ApplyOperation(modifier.Stat, @event.Power, modifier.Operation);
                }
            }
        }
    }
}
