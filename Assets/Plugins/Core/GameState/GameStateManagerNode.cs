using System;
using System.Collections.Generic;
using System.Linq;
using Core.Attributes;
using Core.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.GameState {
    public class GameStateManagerNode : FacadeNode<IGameStateModel>, IGameStateModel {
        [Inject] private IUIManager _ui;

        private readonly Node _root;
        private readonly Dictionary<Type, BaseGameState> _states = new ();
        private readonly List<BaseGameState> _stack = new ();
        private readonly Queue<(OperationEnum Operation, Type StateType)> _operations = new ();
        private bool _isSwitching;

        public GameStateManagerNode(Node root) {
            _root = root;
        }

        protected override void OnConstruct() {
            RegisterInParent(this);

            var contexts = new Dictionary<Type, IContext>();
            foreach (var type in FindStateTypes()) {
                var state = (BaseGameState)Activator.CreateInstance(type);
                _states.Add(type, state);

                if (state is not IContextedGameState contexted) {
                    continue;
                }

                var contextType = contexted.ContextData.GetType();
                if (contexts.TryGetValue(contextType, out var context)) {
                    contexted.ContextData = context;
                }
                else {
                    contexts.Add(contextType, contexted.ContextData);
                    _root.RegisterInChildren(contextType, contexted.ContextData);
                }
            }
        }

        protected override UniTask OnActivate() {
            foreach (var state in _states.Values) {
                Container.Inject(state, this);
            }

            return UniTask.CompletedTask;
        }

        public override void Update(float deltaTime, float unscaledDeltaTime) {
            if (_isSwitching || _operations.Count == 0) {
                return;
            }

            var (operation, stateType) = _operations.Dequeue();
            SwitchAsync(operation, stateType).Forget();
        }

        bool IGameStateModel.HasActiveTransition => _isSwitching || _operations.Count > 0;

        void IGameStateModel.Set<T>() => _operations.Enqueue((OperationEnum.Set, typeof(T)));

        void IGameStateModel.Set<T, TContext>(Action<TContext> contextMutator) {
            contextMutator?.Invoke(((T)_states[typeof(T)]).ContextData);
            _operations.Enqueue((OperationEnum.Set, typeof(T)));
        }

        void IGameStateModel.Back() => _operations.Enqueue((OperationEnum.Back, null));

        void IGameStateModel.Reset<T>() => _operations.Enqueue((OperationEnum.Reset, typeof(T)));

        private async UniTask SwitchAsync(OperationEnum operation, Type stateType) {
            _isSwitching = true;
            try {
                var from = _stack.Count > 0 ? _stack[^1] : null;
                switch (operation) {
                    case OperationEnum.Set: {
                        _stack.Remove(_states[stateType]);
                        _stack.Add(_states[stateType]);
                        break;
                    }
                    case OperationEnum.Back: {
                        if (_stack.Count < 2) {
                            Debug.LogWarning("Can't go back. Last state!");
                            return;
                        }

                        _stack.RemoveAt(_stack.Count - 1);
                        break;
                    }
                    case OperationEnum.Reset: {
                        _stack.Clear();
                        _stack.Add(_states[stateType]);
                        break;
                    }
                }

                var to = _stack[^1];
                if (to == from) {
                    return;
                }

                var activation = to.CollectActivationChanges();
                var deactivation = from?.CollectDeactivationChanges() ?? new List<(Type Type, bool SetActive)>();
                // The next state controls everything it lists itself
                deactivation.RemoveAll(change => activation.Exists(x => x.Type == change.Type));

                from?.OnBeforeDeactivate();
                await ApplyAsync(deactivation);
                from?.OnAfterDeactivate();

                to.OnBeforeActivate();
                await ApplyAsync(activation);
                to.OnAfterActivate();
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
            finally {
                _isSwitching = false;
            }
        }

        private UniTask ApplyAsync(List<(Type Type, bool SetActive)> changes) => UniTask.WhenAll(changes.Select(change => ApplyAsync(change.Type, change.SetActive)));

        private UniTask ApplyAsync(Type type, bool setActive) {
            if (type.IsSubclassOf(typeof(Node)) == false) {
                return _ui.ActivateUIControllerAsync(type, setActive);
            }

            var node = _root.GetNodeInChildren(type);
            if (node == null) {
                Debug.LogError($"Node of type {type.Name} not found");
                return UniTask.CompletedTask;
            }

            return node.SetActiveAsync(setActive);
        }

        private static IEnumerable<Type> FindStateTypes() {
            var assemblyName = typeof(BaseGameState).Assembly.GetName().Name;
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => assembly.GetReferencedAssemblies().Any(reference => reference.Name == assemblyName))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsAbstract == false && type.IsSubclassOf(typeof(BaseGameState)));
        }

        private enum OperationEnum {
            Set,
            Back,
            Reset
        }
    }
}