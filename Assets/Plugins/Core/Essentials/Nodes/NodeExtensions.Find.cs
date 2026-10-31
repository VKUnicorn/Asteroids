using System;

namespace Core {
    public static class NodeExtensions_Find {
        public static T GetNodeInChildren<T>(this Node node) where T : Node => node.GetNodeInChildren(typeof(T)) as T;
        
        public static Node GetNodeInChildren(this Node node, Type nodeType) {
            if (node.GetType() == nodeType) {
                return node;
            }

            foreach (var child in node.Children) {
                var n = child.GetNodeInChildren(nodeType);
                if (n != null)
                    return n;
            }

            return null;
        }
    }
}