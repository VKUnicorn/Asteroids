using UnityEngine;

namespace Nodes.Battle.Components {
    /// <summary>
    /// Base value goes to Additive, Multiplicative is in percents
    /// </summary>
    public struct IntStat {
        public int Additive;
        public float Multiplicative;
        public int FinalValue => Mathf.RoundToInt(Additive * (100f + Multiplicative) * 0.01f);

        public IntStat(int baseValue) {
            Additive = baseValue;
            Multiplicative = 0f;
        }

        public void ApplyOperation(float power, ParameterOperationEnum operation) {
            switch (operation) {
                case ParameterOperationEnum.Add: {
                    Additive += Mathf.RoundToInt(power);
                    break;
                }
                case ParameterOperationEnum.Mul: {
                    Multiplicative += power;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Base value goes to Additive, Multiplicative is in percents
    /// </summary>
    public struct FloatStat {
        public float Additive;
        public float Multiplicative;
        public float FinalValue => Additive * (100f + Multiplicative) * 0.01f;

        public FloatStat(float baseValue) {
            Additive = baseValue;
            Multiplicative = 0f;
        }

        public void ApplyOperation(float power, ParameterOperationEnum operation) {
            switch (operation) {
                case ParameterOperationEnum.Add: {
                    Additive += power;
                    break;
                }
                case ParameterOperationEnum.Mul: {
                    Multiplicative += power;
                    break;
                }
            }
        }
    }

    public enum ParameterOperationEnum {
        Add,
        Mul,
    }
}
