namespace Core.Bindings {
    public interface IBindingTarget {
        /// <summary>
        /// By default, a public field of this object with the given name
        /// </summary>
        IProperty GetProperty(string name) => GetType().GetField(name)?.GetValue(this) as IProperty;
    }
}
