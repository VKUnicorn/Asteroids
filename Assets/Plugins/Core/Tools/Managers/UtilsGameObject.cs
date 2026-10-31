using UnityEngine;

namespace Core.Tools {
    public static class UtilsGameObject {
        private static Transform _rootObject;
        private static Transform _rootTempObject;

        static UtilsGameObject() {
            _rootObject = (new GameObject("Utils")).transform;
            GameObject.DontDestroyOnLoad(_rootObject.gameObject);
        }

        public static GameObject GetPermanent(string name) {
            var child = new GameObject(name);
            child.transform.SetParent(_rootObject);
            return child;
        }

        public static GameObject GetTemp(string name) {
            if (_rootTempObject == null) {
                _rootTempObject = (new GameObject("Utils[TEMP]")).transform;
            }

            var child = new GameObject(name);
            child.transform.SetParent(_rootTempObject);
            return child;
        }
    }
}