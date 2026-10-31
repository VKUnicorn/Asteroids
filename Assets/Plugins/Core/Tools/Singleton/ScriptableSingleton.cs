using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;

#endif

namespace Core.Tools.Singletons {
    public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T> {
        private static T _cachedInstance;

        private static string FileName => typeof(T).Name;

#if UNITY_EDITOR

        private static string AssetPath => "Assets/Resources/" + FileName + ".asset";

#endif

        private static string ResourcePath => FileName;

        public static T Instance {
            get {
                if (_cachedInstance != null) {
                    return _cachedInstance;
                }

                //Prod.Log("Creating ScriptableSingleton: " + typeof(T).Name);
                _cachedInstance = Resources.Load(ResourcePath) as T;
                if (_cachedInstance != null) {
                    _cachedInstance.OnInitialize();
                    return _cachedInstance;
                }

#if UNITY_EDITOR
                if (_cachedInstance == null) {
                    _cachedInstance = CreateAndSave();
                }
#endif
                if (_cachedInstance == null) {
                    Debug.LogWarning($"No instance of {FileName} found, using default values. {typeof(ScriptableSingleton<T>).Name}");
                    _cachedInstance = CreateInstance<T>();
                    _cachedInstance.hideFlags = HideFlags.HideAndDontSave;
                    _cachedInstance.OnInitialize();
                }

                return _cachedInstance;
            }
        }

#if UNITY_EDITOR

        protected static T CreateAndSave() {
            var instance = CreateInstance<T>();
            instance.OnAssetCreate();
            //Saving during Awake() will crash Unity, delay saving until next editor frame
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                EditorApplication.delayCall += () => SaveAsset(instance);
            }
            else {
                SaveAsset(instance);
            }

            return instance;
        }

        private static void SaveAsset(T obj) {
            var dirName = Path.GetDirectoryName(AssetPath);
            if (!Directory.Exists(dirName)) {
                Directory.CreateDirectory(dirName);
            }

            AssetDatabase.CreateAsset(obj, AssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log(FileName + " saved!");
        }

        protected virtual void OnAssetCreate() {
            // Do setup particular to your class here
        }

#endif

        protected virtual void OnInitialize() { }
    }
}