#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif

using System;

namespace Morpeh {
    public interface IComponent : IDisposable {
        void IDisposable.Dispose() { }
    }
    
    public interface IEvent : IDisposable {
        void IDisposable.Dispose() { }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class CacheSizeAttribute : Attribute {
        internal int size;

        public CacheSizeAttribute(int size) {
            this.size = size;
        }
    }
}

#if !ODIN_INSPECTOR
namespace Sirenix.OdinInspector {
    using System;
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class ShowInInspectorAttribute : Attribute { }
}
#endif