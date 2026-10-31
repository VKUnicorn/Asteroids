using System;

namespace Core {
    public static class NodeExtensions_Inject {
        public static T Resolve<T>(this Node node) where T : class {
            if (node.InternalState.Value < NodeStateEnum.Constructed)
                throw new Exception("Tree construction is not completed.");
            
            return node.Container.Resolve<T>(node);
        }

        public static bool TryResolve<T>(this Node node, out T service) where T : class {
            if (node.InternalState.Value < NodeStateEnum.Constructed)
                throw new Exception("Tree construction is not completed.");
            
            return node.Container.TryResolve(node, out service);
        }
    }
}