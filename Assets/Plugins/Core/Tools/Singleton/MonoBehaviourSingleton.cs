using System;
using UnityEngine;

namespace Core.Tools.Singletons {
    public interface IKeepAliveMonoBehaviourSingleton { };

    public interface IAlwaysAccessibleOnQuit { };

    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour {
        public static bool IsQuitting = false;
        public static bool IsInitialized = false;

        public static T Instance {
            get {
                if (IsQuitting) {
                    if (_instance is IAlwaysAccessibleOnQuit) {
                        return _instance;
                    }
                    else {
                        return null;
                    }
                }

                return _instance ?? CreateInstance();
            }
        }

        private static T _instance;

        protected virtual void Awake() {
            if (_instance == null || _instance == this) {
                _instance = this as T;

                try {
                    if (!IsInitialized) {
                        Initialization();
                    }
                }
                catch (Exception E) {
                    Debug.LogException(E);
                }
                finally {
                    IsInitialized = true;
                }
            }
            else {
                Destroy(gameObject);
            }
        }

        private void OnDestroy() {
            try {
                if (this == _instance) {
                    try {
                        Finalization();
                    }
                    catch (Exception E) {
                        Debug.LogException(E);
                    }
                    finally {
                        IsInitialized = false;
                    }
                }
            }
            catch (Exception E) {
                Debug.LogException(E);
            }

            if (this == _instance) {
                _instance = null;
            }
        }

        protected virtual void OnApplicationQuit() {
            IsQuitting = true;
        }

        protected virtual void Initialization() { }

        protected virtual void Finalization() { }

        private static T CreateInstance() {
            if (_instance == null) {
                try {
                    _instance = FindObjectOfType<T>() as T;
                }
                catch (Exception E) {
                    Debug.Log(E);
                }
                finally {
                    if (Application.isPlaying) {
                        if (_instance != null) {
                            // кейс, когда мы запрашиваем GameObject, который реально уже существует на сцене, но для которого еще не был вызван Awake:
                            // нам придется его удалить, и создать новый с насильным запуском Awake, чтобы все процедуры отработали штатно прямо сейчас
                            _instance.gameObject.SetActive(false);
                            Destroy(_instance.gameObject);
                            _instance = null;

                            Debug.Log("Force creating singleton" + typeof(T).Name);
                        }

                        var go = typeof(IKeepAliveMonoBehaviourSingleton).IsAssignableFrom(typeof(T))
                                     ? UtilsGameObject.GetPermanent(typeof(T).Name)
                                     : UtilsGameObject.GetTemp(typeof(T).Name);

                        // хукаемся так, чтобы m_instance заполнился ДО вызова Awake,
                        // тем самым делая неважным порядок вызова Awake в наследовании
                        go.SetActive(false);
                        _instance = go.AddComponent<T>();
                        go.SetActive(true);
                    }
                }
            }

            return _instance;
        }

        public static bool HasInstance => (!IsQuitting || (_instance is IAlwaysAccessibleOnQuit)) && _instance != null;
    }
}