using System;

namespace Core.GameState {
    public interface IGameStateModel {
        /// <summary>
        /// Switch is in progress or queued
        /// </summary>
        bool HasActiveTransition { get; }

        /// <summary>
        /// Add T to stack, switch
        /// </summary>
        void Set<T>() where T : BaseGameState;

        /// <summary>
        /// Mutate T context, add T to stack, switch
        /// </summary>
        void Set<T, TContext>(Action<TContext> contextMutator) where T : ContextedGameState<TContext> where TContext : class, IContext, new();

        /// <summary>
        /// Drop last state, switch back to stack.last
        /// </summary>
        void Back();

        /// <summary>
        /// Drop entire state stack, switch to new T state
        /// </summary>
        void Reset<T>() where T : BaseGameState;
    }
}
