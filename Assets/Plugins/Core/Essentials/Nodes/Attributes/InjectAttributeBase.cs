using UnityEngine.Scripting;

namespace Core.Nodes.Attributes {
    public abstract class InjectAttributeBase : PreserveAttribute {
        public bool Optional { get; set; }

        public object Id { get; set; }
    }
}