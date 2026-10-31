using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Core.Tools;
using Core.Tools.Extensions;
using Cysharp.Threading.Tasks;
using R3;
using Debug = UnityEngine.Debug;

namespace Core {
    public static class NodeExtensions_Tree {
        public static async UniTask InitNodes(this Node node, CancellationToken cancellationToken = default) {
            node.Container ??= new Container(node, null);
            
            node.ConstructInternal();
            node.InjectInternal();
            var activated = new List<Node>();
            await node.ActivateBranchInternal(activated, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var activatedNode in activated) {
                try {
                    activatedNode.OnAfterActivate();
                }
                catch (Exception e) {
                    Debug.LogError($"Failed to Activate {node.GetType().Name}");
                    Debug.LogException(e);
                }
            }
        }
        
        internal static void ConstructInternal(this Node node) {
            if (node.State.CurrentValue == NodeStateEnum.Created) {
                try {
                    node.OnConstruct();
                }
                catch (Exception e) {
                    Debug.LogError($"Failed to Construct {node.GetType().Name}");
                    Debug.LogException(e);
                }
                
                ConstructChildren();
                node.InternalState.Value = NodeStateEnum.Constructed;
            }
            else {
                ConstructChildren();
            }

            RegisterFacades();
            return;
            
            void ConstructChildren() {
                foreach (var child in node.Children) {
                    if (child.State.CurrentValue >= NodeStateEnum.Constructed)
                        continue;

                    child.ConstructInternal();
                }
            }

            void RegisterFacades() {
                foreach (var cb in node.FacadesCallbacks) {
                    cb.Invoke();
                }

                node.FacadesCallbacks.Clear();
            }
        }

        internal static void InjectInternal(this Node node) {
            if (node.State.CurrentValue == NodeStateEnum.Constructed) {
                node.Container.SetConstructed(true);
                try {
                    node.Container.Inject(node, node);
                    node.OnAfterInject();
                }
                catch (Exception e) {
                    Debug.LogError($"Failed to Inject {node.GetType().Name}");
                    Debug.LogException(e);
                }
                node.InternalState.Value = NodeStateEnum.Injected;
            }

            foreach (var child in node.Children) {
                child.InjectInternal();
            }
        }

        internal static async UniTask ActivateBranchInternal(this Node node, List<Node> activated, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            var shouldBeDeactivated = !node.DesiredActiveSelf || !node.DesiredActiveInHierarchy;
            if (shouldBeDeactivated)
                return;

            if (node.ActivationInProgress) {
                throw new Exception($"Activation called twice in loop! {node.GetType()} {node.InternalState.Value}");
            }
            
            node.ActivationInProgress = true;
            
            // Dynamically added in OnActivate state nodes
            if (node.InternalState.Value == NodeStateEnum.Created) {
                if (node.GetType().IsGenericType(typeof(FacadeNode<>))) {
                    Debug.LogError($"You are trying to use dynamic creation of '{node.GetType().Name}: FacadeNode<>' in " +
                                   $"{node.Parent.GetType().Name}.OnActivate() state. " +
                                   $"Facades creation via AddNode() only allowed in OnConstruct() !");
                }
                
                node.ConstructInternal();
                node.InjectInternal();
            }
            
            if (node.InternalState.Value != NodeStateEnum.Active) {
                // First time
                if (node.InternalState.Value < NodeStateEnum.Active) {
                    Disposable.Create(() => {
                        node.CancellationTokenSource?.Cancel();
                        node.CancellationTokenSource?.Dispose();
                    }).AddTo(node.Lifetime);
                }
                
                Disposable.Create(() => {
                    node.DeactivationCancellationTokenSource?.Cancel();
                    node.DeactivationCancellationTokenSource?.Dispose();
                    node.DeactivationCancellationTokenSource = null;
                }).AddTo(node.DeactivateLifetime);
                
                try {
#if UNITY_EDITOR && !DISABLE_EDITOR_NODE_DEBUGGER
                    var activationTask = node.OnActivate();
                    var launchTime = Stopwatch.StartNew();
                    while (activationTask.Status == UniTaskStatus.Pending) {
                        await UniTask.Yield();
                        if (launchTime.ElapsedMilliseconds > 20000) {
                            Debug.LogError($"Node '{node.GetType().Name}.OnActivate()' is taking too long... " +
                                           $"Should avoid circular dependencies. Check twice. No circular dependencies!");
                            break;
                        }
                    }
                    
                    await activationTask;
#else
                    await node.OnActivate();
#endif
                }
                catch (OperationCanceledException) {
                    throw;
                }
                catch (Exception e) {
                    Debug.LogError($"Failed to Activate {node.GetType().Name}");
                    Debug.LogException(e);
                }
                
                activated.Add(node);
            }
            
            // Deactivated in OnActivate
            if (node.DesiredActiveSelf == false || node.DesiredActiveInHierarchy == false) {
                node.ActivationInProgress = false;
                return;
            }
            
            for (var index = 0; index < node.Children.Count; index++) {
                var child = node.Children[index];
                await child.ActivateBranchInternal(activated, cancellationToken);
            }
            
            node.ActivationInProgress = false;
            node.InternalState.Value = NodeStateEnum.Active;
        }

        internal static async UniTask DeactivateBranchInternal(this Node node) {
            var shouldBeActivated = node.DesiredActiveSelf && node.DesiredActiveInHierarchy;
            if (shouldBeActivated) {
                Debug.LogWarning($"Strange behaviour. {node.GetType()}");
                return;
            }

            if (node.DeactivationInProgress) {
                throw new Exception($"Deactivation called twice in loop! {node.GetType()}");
            }

            node.DeactivationInProgress = true;

            for (var index = 0; index < node.Children.Count; index++) {
                var child = node.Children[index];
                await child.DeactivateBranchInternal();
            }

            if (node.InternalState.Value == NodeStateEnum.Active) {
                try {
                    await node.OnDeactivate();
                }
                catch (OperationCanceledException) {
                    throw;
                }
                catch (Exception e) {
                    Debug.LogError($"Failed to Deactivate {node.GetType().Name}");
                    Debug.LogException(e);
                }
            }

            node.DeactivateLifetime.Clear();
            node.InternalState.Value = NodeStateEnum.Inactive;
            node.DeactivationInProgress = false;
            try {
                node.OnAfterDeactivate();               
            }
            catch (Exception e) {
                Debug.LogError($"Failed to Deactivate {node.GetType().Name}");
                Debug.LogException(e);
            }
        }
        
        public static Node AddNode(this Node node, Node child, bool setActive = true) {
            child.SetDesiredActiveSelf(setActive);
            child.SetDesiredActiveHierarchy(node.DesiredActiveInHierarchy);
            child.ParentRegistration = node.AddChild(child);
            return node;
        }

        public static Node AddNode(this Node node, FacadeNode child, bool setActive = true) {
            child.SetDesiredActiveSelf(setActive);
            child.SetDesiredActiveHierarchy(node.DesiredActiveInHierarchy);
            child.ParentRegistration = node.AddChild(child);
            node.FacadesCallbacks.Add(() => {
                foreach (var facade in child.Facades) {
                    node.RegisterInChildren(facade.Item2, facade.Item1);
                }
            });
            return node;
        }
        
        internal static IDisposable AddChild(this Node node, Node child) {
            child.Parent = node;
            child.Container = new Container(child, node.Container);
            child.LastNodeInFlatList = node.ThisNodeInFlatList;

            var nodeInFlatList = Node.FlatNodeList.InsertAfter(child, node.LastNodeInFlatList);
            child.ThisNodeInFlatList = nodeInFlatList;
            node.LastNodeInFlatList = nodeInFlatList;
            node.Children.Add(child);
            return Disposable.Create((t: node, child, nodeInFlatList),tuple => {
                if (tuple.t.LastNodeInFlatList == tuple.nodeInFlatList) {
                    tuple.t.LastNodeInFlatList = nodeInFlatList.Previous;
                }
                tuple.t.Children.Remove(tuple.child);
                tuple.nodeInFlatList.Dispose();
            });
        }
    }
}