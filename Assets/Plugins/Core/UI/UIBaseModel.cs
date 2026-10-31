using R3;

namespace Core.UI {
    public abstract class UIBaseModel {
        public readonly ReactiveProperty<bool> Opened = new();
    }
}
