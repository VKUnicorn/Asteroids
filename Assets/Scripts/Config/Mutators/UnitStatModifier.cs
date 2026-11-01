using System;
using Nodes.Battle.Components;

namespace Config {
    [Serializable]
    public class UnitStatModifier : IMutator {
        public UnitParameterEnum Stat;
        public ParameterOperationEnum Operation;
    }
}
