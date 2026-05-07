using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Edia {

    [EdiaHeader("EDIA CORE", "Scene Loader", "Loads a additional scene")]
    public class SceneLoader : Singleton<SceneLoader> {
        
        [Header("Scene Loader")]

#if UNITY_EDITOR
        [SerializeField] private SceneAsset sceneAsset;
#endif

        [SerializeField, HideInInspector] string _sceneName;

#if UNITY_EDITOR
        private void OnValidate() {
            // Update the scene name whenever the scene asset changes
            if (sceneAsset == null)
                return;
                
            string assetPath = AssetDatabase.GetAssetPath(sceneAsset);
            if (!string.IsNullOrWhiteSpace(assetPath))
                _sceneName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        }
#endif

        private void Awake() {
            LoadScene(_sceneName);
        }

        /// <summary>
        /// Loads a specified scene.
        /// </summary>
        /// <param name="sceneName"></param>Name of the scene to be loaded.
        public void LoadScene(string sceneName) {
            // Load the scene asynchronously
            if (string.IsNullOrWhiteSpace(sceneName)) {
                Debug.LogError("Scene name is empty, cannot load scene.");
                return;
            }
            
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid() && loadedScene.isLoaded) {
                Debug.LogWarning($"Scene {sceneName} is already loaded.");
                return;
            }
            
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
            if (operation == null) {
                Debug.LogError($"Failed to load scene '{sceneName}'. Make sure it is added to Build Settings.");
            }
        }

        /// <summary>
        /// Unloads a specified scene asynchronously.
        /// </summary>
        /// <param name="sceneName">The name of the scene to be unloaded asynchronously.</param>
        public void UnloadScene(string sceneName) {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded) {
                Debug.LogError($"Scene '{sceneName}' does not exist and cannot be unloaded.");
                return;
            }

            var operation = SceneManager.UnloadSceneAsync(sceneName);
            if (operation == null) {
                Debug.LogError($"Failed to unload scene '{sceneName}'.");
            }
        }
    }
}