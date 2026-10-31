using System;
using Cysharp.Threading.Tasks;

namespace Core.UI {
    public interface IUIManager {
        UniTask ActivateUIControllerAsync(Type controllerType, bool setActive);
    }
}
