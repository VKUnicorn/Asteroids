using System;
using R3;
using UnityEngine;

namespace Core.UI {
    public class UICanvas : MonoBehaviour {
        public UIBaseModel Model { get; private set; }

        private IDisposable _subscription;

        internal void SetModel(UIBaseModel model) {
            Model = model;
            _subscription = model.Opened.Subscribe(gameObject.SetActive);
        }

        private void OnDestroy() => _subscription?.Dispose();
    }
}
