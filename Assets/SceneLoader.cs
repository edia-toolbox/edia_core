using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {
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
}