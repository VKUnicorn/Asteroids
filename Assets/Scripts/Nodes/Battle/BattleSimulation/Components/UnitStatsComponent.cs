using Morpeh;

namespace Nodes.Battle.Components {
    public struct UnitStatsComponent : IComponent {
        public IntStat Attack;
        public IntStat MaxHealth;
        public FloatStat MovementSpeed;

        public void ApplyOperation(UnitParameterEnum stat, float power, ParameterOperationEnum operation) {
            switch (stat) {
                case UnitParameterEnum.Attack: {
                    Attack.ApplyOperation(power, operation);
                    break;
                }
                case UnitParameterEnum.MaxHealth: {
                    MaxHealth.ApplyOperation(power, operation);
                    break;
                }
                case UnitParameterEnum.MovementSpeed: {
                    MovementSpeed.ApplyOperation(power, operation);
                    break;
                }
            }
        }
    }

    public enum UnitParameterEnum {
        Attack,
        MaxHealth,
        MovementSpeed,
    }
}
