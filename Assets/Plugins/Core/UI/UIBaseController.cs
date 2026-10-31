namespace Core.UI {
    public abstract class UIBaseController {
        internal abstract UIBaseModel BaseModel { get; }

        /// <summary>
        /// Called once, after injection and before the first activation
        /// </summary>
        protected internal virtual void OnModelSet() { }
        protected internal virtual void OnActivate() { }
        protected internal virtual void OnDeactivate() { }

        internal void SetActive(bool active) {
            if (BaseModel.Opened.Value == active) {
                return;
            }

            if (active) {
                BaseModel.Opened.Value = true;
                OnActivate();
            }
            else {
                OnDeactivate();
                BaseModel.Opened.Value = false;
            }
        }
    }

    public abstract class UIBaseController<TModel> : UIBaseController where TModel : UIBaseModel, new() {
        protected TModel Model { get; } = new();

        internal override UIBaseModel BaseModel => Model;
    }
}
