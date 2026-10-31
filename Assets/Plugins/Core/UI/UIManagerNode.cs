using System;
using System.Collections.Generic;
using Core.Tools;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.UI {
    /// <summary>
    /// Creates controllers on first activation. Canvas prefab is loaded from Addressables
    /// by the model name without "Model" suffix: HudModel -> "Hud"
    /// </summary>
    public class UIManagerNode : FacadeNode<IUIManager>, IUIManager {
        private readonly Transform _root;
        private readonly Dictionary<Type, UIBaseController> _controllers = new();

        public UIManagerNode(Transform root) {
            _root = root;
        }

        protected override void OnConstruct() {
            RegisterInParent(this);
        }

        async UniTask IUIManager.ActivateUIControllerAsync(Type controllerType, bool setActive) {
            if (_controllers.TryGetValue(controllerType, out var controller) == false) {
                if (setActive == false) {
                    return;
                }

                controller = (UIBaseController)Activator.CreateInstance(controllerType);
                _controllers.Add(controllerType, controller);
                Container.Inject(controller, this);
                controller.OnModelSet();
                await SpawnCanvasAsync(controller.BaseModel);
            }

            controller.SetActive(setActive);
        }

        private async UniTask SpawnCanvasAsync(UIBaseModel model) {
            var address = model.GetType().Name.Replace("Model", "");
            var instance = await Addressables.InstantiateAsync(address, _root);
            Disposable.Create(() => {
                if (instance != null) {
                    Addressables.ReleaseInstance(instance);
                }
            }).AddTo(Lifetime);

            instance.GetComponent<UICanvas>().SetModel(model);
        }
    }
}
