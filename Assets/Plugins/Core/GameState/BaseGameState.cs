using System;
using System.Collections.Generic;
using Core.Attributes;
using Core.UI;

namespace Core.GameState {
    /// <summary>
    /// Registered in DI, so nodes and UI controllers can [Inject] it.
    /// One instance per context type, shared by all states using it
    /// </summary>
    public interface IContext { }

    internal interface IContextedGameState {
        IContext ContextData { get; set; }
    }

    public abstract class ContextedGameState<TContext> : BaseGameState, IContextedGameState where TContext : class, IContext, new() {
        public TContext ContextData { get; private set; } = new();

        IContext IContextedGameState.ContextData {
            get => ContextData;
            set => ContextData = (TContext)value;
        }
    }

    public abstract class BaseGameState {
        private readonly List<(Type Type, bool SetActive)> _changes = new();

        [Inject] protected IGameStateModel States;

        /// <summary>
        /// List UI and nodes that must be active in this state
        /// </summary>
        protected abstract void BuildActivationSchema();

        /// <summary>
        /// List UI and nodes that should be inactive in next state (may be overriden by the next state)
        /// </summary>
        protected abstract void BuildDeactivationSchema();

        protected internal virtual void OnBeforeActivate() { }
        protected internal virtual void OnAfterActivate() { }
        protected internal virtual void OnBeforeDeactivate() { }
        protected internal virtual void OnAfterDeactivate() { }

        protected void ActivateUI<T>(bool setActive) where T : UIBaseController => _changes.Add((typeof(T), setActive));

        protected void ActivateNode<T>(bool setActive) where T : Node => _changes.Add((typeof(T), setActive));

        internal List<(Type Type, bool SetActive)> CollectActivationChanges() {
            _changes.Clear();
            BuildActivationSchema();
            return _changes;
        }

        internal List<(Type Type, bool SetActive)> CollectDeactivationChanges() {
            _changes.Clear();
            BuildDeactivationSchema();
            return _changes;
        }
    }
}
