using System;
using System.Collections.Generic;
using Morpeh;
using Nodes.Battle.Systems;

namespace Nodes.Battle {
    public class BattleSimulation : IDisposable {
        private readonly List<BaseSystem> _systems = new () {
            new UnitStatsModifierUpdateSystem(),
        };

        public BattleSimulationContext Context { get; }

        public BattleSimulation(BattleSimulationContext context) {
            Context = context;

            foreach (var system in _systems) {
                system.Initialize(context);
            }
        }

        public void Simulate(int ticks) {
            for (var i = 0; i < ticks; i++) {
                DoTick();
            }
        }

        public void UpdateOutputs() => Context.Output.WriteOutputs();

        private void DoTick() {
            var world = Context.Input.World;
            var events = Context.Input.Events;
            Context.Clock.DoTick();
            events.FrameStart();
            world.UpdateFilters();
            foreach (var system in _systems) {
                system.OnUpdate();
                world.UpdateFilters();
            }

            events.FrameComplete();
        }

        public void Dispose() => Context.Dispose();
    }
}
