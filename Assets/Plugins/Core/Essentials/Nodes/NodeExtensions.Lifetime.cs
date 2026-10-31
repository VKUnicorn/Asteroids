using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace Core {
    public static class NodeExtensions_Lifetime {
        public static async UniTask SetActiveAsync(this Node node, bool setActiveSelf, CancellationToken cancellationToken = default) {
            var prevActive = node.DesiredActiveSelf && node.DesiredActiveInHierarchy;
            node.SetDesiredActiveSelf(setActiveSelf);
            var sameDesiredState = prevActive == (node.DesiredActiveSelf && node.DesiredActiveInHierarchy);
            var sameActualState = node.IsActiveInHierarchy == setActiveSelf;
            if (sameDesiredState && sameActualState) {
                return;
            }

            if (setActiveSelf) {
                var activated = new List<Node>();
                await node.ActivateBranchInternal(activated, cancellationToken);
                foreach (var activatedNode in activated) {
                    activatedNode.OnAfterActivate();
                }
            }
            else {
                await node.DeactivateBranchInternal();
            }
        }
        
        internal static void SetDesiredActiveSelf(this Node node, bool active) {
            if (node.DesiredActiveSelf == active) {
                return; // no changes
            }

            node.DesiredActiveSelf = active;
            active = node.DesiredActiveSelf && node.DesiredActiveInHierarchy;
            foreach (var child in node.Children) {
                child.SetDesiredActiveHierarchy(active);    
            }
        }

        internal static void SetDesiredActiveHierarchy(this Node node, bool active) {
            if (node.DesiredActiveInHierarchy == active) {
                return; // no changes
            }

            node.DesiredActiveInHierarchy = active;

            active = node.DesiredActiveSelf && node.DesiredActiveInHierarchy;
            foreach (var child in node.Children) {
                child.SetDesiredActiveHierarchy(active);    
            }
        }
        
        public static async UniTask DestroyChildrenAsync<T>(this Node node) {
            for (var index = node.Children.Count - 1; index >= 0; index--) {
                if (node.Children[index] is T) {
                    await node.Children[index].SetActiveAsync(false);
                }
            }

            for (var index = node.Children.Count - 1; index >= 0; index--) {
                if (node.Children[index] is T) {
                    node.Children[index].Dispose();
                }
            }
        }
        
        public static async UniTask DestroyChildrenAsync(this Node node) {
            for (var index = node.Children.Count - 1; index >= 0; index--) {
                await node.Children[index].SetActiveAsync(false);
            }
            for (var index = node.Children.Count - 1; index >= 0; index--) {
                node.Children[index].Dispose();
            }
        }
        
        public static async UniTask DestroyAsync(this Node node) {
            await node.SetActiveAsync(false);
            node.Dispose();
        }
        
        public static void DestroyImmediate(this Node node) {
            node.Dispose();
        }

        public static bool IsDisposed(this Node node) => node.Lifetime.IsDisposed;
        
        public static void Dispose(this Node node) {
            if (node.IsDisposed()) {
                return;
            }

            if (node.IsActiveInHierarchy) {
                // Dangerous, async call in sync method
                node.SetActiveAsync(false).Forget();
            }
            
            for (var index = node.Children.Count - 1; index >= 0; index--) {
                var subNode = node.Children[index];
                try {
                    subNode.Dispose();
                    EditorValidateNodeIsDisposed(subNode);
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }

            node.DeactivateLifetime.Dispose();
            node.Lifetime.Dispose();
            node.Children.Clear();
            node.ParentRegistration?.Dispose();
        }
        
        public static async UniTask DisposeAsync(this Node node) {
            if (node.IsDisposed()) {
                return;
            }

            if (node.IsActiveInHierarchy) {
                await node.SetActiveAsync(false);
            }
            
            for (var index = node.Children.Count - 1; index >= 0; index--) {
                var subNode = node.Children[index];
                try {
                    subNode.Dispose();
                    EditorValidateNodeIsDisposed(subNode);
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }

            node.DeactivateLifetime.Dispose();
            node.Lifetime.Dispose();
            node.Children.Clear();
            node.ParentRegistration?.Dispose();
        }

        [Conditional("UNITY_EDITOR")]
        private static void EditorValidateNodeIsDisposed(Node node) {
            if (node.IsDisposed() == false) {
                Debug.LogError($"Node '{node.GetType().Name}' is not disposed! Check Dispose() method overrides.");
            }
        }
    }
}