using Core.Bindings;
using R3;

namespace Core.UI {
    public abstract class UIBaseModel : IBindingTarget {
        public readonly ReactiveProperty<bool> Opened = new ();
    }
}
