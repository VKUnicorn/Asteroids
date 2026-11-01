using System;
using Core.Bindings;
using R3;
using UnityEngine;

namespace Core.UI {
    /// <summary>
    /// Gives bindings below it access to the model properties
    /// </summary>
    public class UICanvas : MonoBehaviour, IBindingTarget {
        public UIBaseModel Model { get; private set; }

        private IDisposable _subscription;

        internal void SetModel(UIBaseModel model) {
            Model = model;
            _subscription = model.Opened.Subscribe(gameObject.SetActive);
        }

        IProperty IBindingTarget.GetProperty(string name) => ((IBindingTarget)Model)?.GetProperty(name);

        private void OnDestroy() => _subscription?.Dispose();
    }
}
