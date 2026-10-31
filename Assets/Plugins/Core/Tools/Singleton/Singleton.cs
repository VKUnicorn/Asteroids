using UnityEngine;

namespace Core.Tools.Singletons {
    public abstract class Singleton<T> where T : new() {
        public static bool HasInstance => _instance != null;

        public static T Instance {
            get {
                if (_instance == null) {
                    _instance = new T();
                }

                return _instance;
            }
        }

        private static T _instance;

        public Singleton() {
            if (_instance != null) {
#if UNITY_EDITOR
                Debug.LogError($"Singleton {typeof(T).Name} already have instance!");
#endif
            }
        }
    }
}