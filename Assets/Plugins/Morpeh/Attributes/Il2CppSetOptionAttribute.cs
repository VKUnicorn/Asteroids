// ReSharper disable once CheckNamespace
namespace Unity.IL2CPP.CompilerServices {
    using System;

    [Serializable] // Skip Obfuscation
    public enum Option {
        NullChecks = 1,
        ArrayBoundsChecks = 2,
        DivideByZeroChecks = 3
    }

    [Serializable] // Skip Obfuscation
    // Related info: https://leopotam.com/20/ and https://docs.unity3d.com/Manual/IL2CPP.html
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class Il2CppSetOptionAttribute : Attribute {
        public Option Option { get; }
        public object Value { get; }

        public Il2CppSetOptionAttribute(Option option, object value) {
            this.Option = option;
            this.Value = value;
        }
    }

    // You can derive from this class to avoid writing repeating Il2CppSetOption properties boilerplate code on classes
    // Hovever, it's impossible to create a basic struct, bacause you can not inherit structs, also you can not
    // define those on interface, bacause structs\classes does not intherit interface attributes
    [Serializable] // Skip Obfuscation
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class DeriveIl2CppOptions { }
}