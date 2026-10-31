using System;
using Core.Nodes.Attributes;

namespace Core.Attributes {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    public class InjectAttribute : InjectAttributeBase
    {
    }
}