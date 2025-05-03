using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Edia {

    public class SceneLoader : Singleton<SceneLoader> {
        [Header("Scene Loader")]
        [InspectorHeader("EDIA CORE", "Scene Loader", "Loads a additional scene")]
        [SerializeField] private Object sceneAsset;

        private string _sceneName;

#if UNITY_EDITOR
        private void OnValidate() {
            // Update the scene name whenever the scene asset changes
            if (sceneAsset != null) {
                string assetPath = AssetDatabase.GetAssetPath(sceneAsset);
                if (!string.IsNullOrEmpty(assetPath))
                    _sceneName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            }
        }
#endif

        private void Awake() {
            LoadSceneAsync();
        }

        private void LoadSceneAsync() {
            // Load the scene asynchronously
            if (!string.IsNullOrEmpty(_sceneName))
                SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
            else
                Debug.LogError("Scene name is empty, cannot load scene.");
        }

#region Available methods

        /// <summary>
        /// Loads a specified scene asynchronously and additively.
        /// </summary>
        /// <param name="sceneName">The name of the scene to be loaded asynchronously.</param>
        public void LoadScene(string sceneName) {
            _sceneName = sceneName;
            LoadSceneAsync();
        }

        /// <summary>
        /// Unloads a specified scene asynchronously.
        /// </summary>
        /// <param name="sceneName">The name of the scene to be unloaded asynchronously.</param>
        public void UnloadScene(string sceneName) {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid()) {
                Debug.LogError($"Scene '{sceneName}' does not exist and cannot be unloaded.");
                return;
            }

            SceneManager.UnloadSceneAsync(sceneName);
        }

#endregion
    }
}