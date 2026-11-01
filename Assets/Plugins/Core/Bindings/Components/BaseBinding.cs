using UnityEngine;

namespace Core.Bindings {
    public abstract class BaseBinding<T> : MonoBehaviour where T : class, IProperty {
        [SerializeField] private string Path;

        protected T Property { get; private set; }

        /// <summary>
        /// Takes the property from the closest IBindingTarget up the hierarchy
        /// </summary>
        protected virtual void Awake() {
            foreach (var target in GetComponentsInParent<IBindingTarget>(true)) {
                if (target.GetProperty(Path) is T property) {
                    Property = property;
                    return;
                }
            }

            Debug.LogError($"[Binding] {typeof(T).Name} '{Path}' not found", this);
        }
    }
}
