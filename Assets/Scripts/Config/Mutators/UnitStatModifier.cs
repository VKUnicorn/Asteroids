using Nodes.Battle.Components;
using UnityEngine;

namespace Config {
    [CreateAssetMenu(menuName = "Config/Mutators/Unit Stat Modifier")]
    public class UnitStatModifier : Mutator {
        [SerializeField] private UnitParameterEnum _stat;
        [SerializeField] private ParameterOperationEnum _operation;

        public UnitParameterEnum Stat => _stat;
        public ParameterOperationEnum Operation => _operation;
    }
}
