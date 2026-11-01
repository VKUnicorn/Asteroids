using Core.UI;
using Nodes.UI.Model;
using R3;
using UnityEngine;

namespace Nodes.UI.Controller {
    public class UIMainMenuController : UIBaseController<UIMainMenuModel> {
        protected override void OnModelSet() {
            Model.Play.Subscribe(_ => {
                Debug.Log("Play!");
            });
        }
    }
}
