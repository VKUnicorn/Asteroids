using System;
using System.Collections.Generic;
using Core.Tools.UnityHelpers;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Core {
    public abstract class RootUpdaterNode : Node, IUpdater {
        protected virtual IUnityLoopProvider UnityLoopProvider => UnityRuntimeHelper.Instance;
        private readonly List<IUpdatable> _updatables = new();
        private readonly List<IFixedUpdatable> _fixedUpdatables = new();

        protected internal override UniTask OnActivate() {
            UnityLoopProvider.OnUpdate += Update;
            UnityLoopProvider.OnFixedUpdate += FixedUpdate;
            return UniTask.CompletedTask;
        }
        
        protected internal override UniTask OnDeactivate() {
            if (UnityLoopProvider != null) {
                UnityLoopProvider.OnUpdate -= Update;
                UnityLoopProvider.OnFixedUpdate -= FixedUpdate;
            }

            return UniTask.CompletedTask;
        }

        private void Update() {
            var dt = Time.deltaTime;
            var dtu = Time.unscaledDeltaTime;
            foreach (var node in FlatNodeList) {
                if (node.State.CurrentValue != NodeStateEnum.Active)
                    continue;

                node.Update(dt, dtu);
            }
            
            foreach (var updatable in _updatables) {
                updatable.Update(dt, dtu);
            }

            Update(dt, dtu);
        }

        private void FixedUpdate() {
            var dt = Time.fixedDeltaTime;
            var dtu = Time.fixedUnscaledDeltaTime;
            foreach (var node in FlatNodeList) {
                if (node.State.CurrentValue != NodeStateEnum.Active)
                    continue;

                node.FixedUpdate(dt, dtu);
            }

            foreach (var updatable in _fixedUpdatables) {
                updatable.FixedUpdate(dt, dtu);
            }

            FixedUpdate(dt, dtu);
        }

        public IDisposable SubscribeUpdate(IUpdatable u) {
            if (_updatables.Contains(u))
                return Disposable.Empty;
            
            _updatables.Add(u);
            return Disposable.Create(() => _updatables.Remove(u));
        }

        public IDisposable SubscribeFixedUpdate(IFixedUpdatable u) {
            if (_fixedUpdatables.Contains(u))
                return Disposable.Empty;
            
            _fixedUpdatables.Add(u);
            return Disposable.Create(() => _fixedUpdatables.Remove(u));
        }

        public void RegisterUpdate(IUpdatable u) {
            if (_updatables.Contains(u))
                return;
            
            _updatables.Add(u);
        }

        public void RegisterFixedUpdate(IFixedUpdatable u) {
            if (_fixedUpdatables.Contains(u))
                return;
            
            _fixedUpdatables.Add(u);
        }

        public bool UnregisterUpdate(IUpdatable u) => _updatables.Remove(u);

        public bool UnregisterFixedUpdate(IFixedUpdatable u) => _fixedUpdatables.Remove(u);
    }
}