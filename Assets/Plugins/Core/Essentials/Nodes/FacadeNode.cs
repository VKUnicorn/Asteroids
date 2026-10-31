using System;
using System.Collections.Generic;
using Core.Tools.Extensions;

namespace Core {
    public abstract class FacadeNode : Node {
        public abstract IEnumerable<(Type, object)> Facades { get; }
    }
    
    public abstract class FacadeNode<T> : FacadeNode where T : class {
        private readonly (Type type, object facade)[] _facades = new (Type, object)[1];
        
        public sealed override IEnumerable<(Type, object)> Facades {
            get {
                if (_facades[0].Item2 == null) {
                    throw new Exception($"Facade of node {GetType().Name} is not registered. Use {typeof(T).GetShortName()}(facade)");
                }
                return _facades;
            }
        }

        protected void RegisterInParent(T facade) {
            _facades[0] = (typeof(T), facade);
        }
    }
}