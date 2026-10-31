using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Core.Tools;
using Core.Tools.Collections;
using Cysharp.Threading.Tasks;
using R3;

namespace Core {
    public abstract class Node : IInjectTarget, IUpdatable, IFixedUpdatable, IDisposable {
        internal static readonly LinkedListWithInsert<Node> FlatNodeList = new();
        
        public Node Parent { get; internal set; }
        public Container Container { get; internal set; }
        public ReadOnlyReactiveProperty<NodeStateEnum> State => InternalState;
        public List<Node> Children { get; } = new();
        public bool IsActiveInHierarchy =>  State.CurrentValue == NodeStateEnum.Active && DesiredActiveSelf && DesiredActiveInHierarchy;
        public bool IsActiveSelf => State.CurrentValue == NodeStateEnum.Active && DesiredActiveSelf;
        
        protected internal CancellationTokenSource GetDeactivationCancellationTokenSource() => (DeactivationCancellationTokenSource ??= new CancellationTokenSource());        
        protected internal CancellationToken GetDeactivationCancellationToken() => GetDeactivationCancellationTokenSource().Token;        
        protected internal CancellationToken GetCancellationToken() => (CancellationTokenSource ??= new CancellationTokenSource()).Token;
        protected internal Lifetime Lifetime { get; } = new();
        protected internal Lifetime DeactivateLifetime { get; } = new();
        
        internal IDisposable ParentRegistration;
        internal LinkedListWithInsert<Node>.ListNode ThisNodeInFlatList;
        internal LinkedListWithInsert<Node>.ListNode LastNodeInFlatList;
        internal readonly ReactiveProperty<NodeStateEnum> InternalState = new(NodeStateEnum.Created);
        internal bool DesiredActiveSelf = true;
        internal bool DesiredActiveInHierarchy = true;
        internal readonly List<Action> FacadesCallbacks = new();
        internal bool ActivationInProgress;
        internal bool DeactivationInProgress;
        internal CancellationTokenSource CancellationTokenSource;
        internal CancellationTokenSource DeactivationCancellationTokenSource;
        
        protected Node() { }

        #region Virtual

        protected internal abstract void OnConstruct();
        protected internal virtual void OnAfterInject() {}
        protected internal virtual UniTask OnActivate() => UniTask.CompletedTask;
        protected internal virtual void OnAfterActivate() {}
        protected internal virtual UniTask OnDeactivate() => UniTask.CompletedTask;
        protected internal virtual void OnAfterDeactivate() {}
        public virtual void Update(float deltaTime, float unscaledDeltaTime) { }
        public virtual void FixedUpdate(float fixedDeltaTime, float unscaledFixedDeltaTime) { }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node AddNode(Node child, bool setActive = true) => NodeExtensions_Tree.AddNode(this, child, setActive);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node AddNode(FacadeNode child, bool setActive = true) =>  NodeExtensions_Tree.AddNode(this, child, setActive);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Container.Registration RegisterInChildren<T>(T obj) where T : class => Container.Register<T>(obj);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Container.Registration RegisterInChildren(Type type, object service) => Container.Register(type, service);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Container.Registration[] RegisterInChildren(object obj, params Type[] interfaces) => interfaces.Select(x => Container.Register(x, obj)).ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Dispose() => NodeExtensions_Lifetime.Dispose(this);
    }
}