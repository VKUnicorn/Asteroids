using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Bindings {
    [RequireComponent(typeof(Button))]
    public class ButtonCommandBinding : BaseBinding<CommandProperty> {
        protected override void Awake() {
            base.Awake();
            GetComponent<Button>().onClick.AddListener(() => Property?.Execute(Unit.Default));
        }
    }
}
