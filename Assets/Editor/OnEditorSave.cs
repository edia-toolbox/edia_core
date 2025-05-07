using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Edia.Utilities.Editor {
    [InitializeOnLoad]
    public static class OnEditorSave {
        static OnEditorSave() {
            EditorSceneManager.sceneSaved += OnSceneSaved;
        }

        private static void OnSceneSaved(UnityEngine.SceneManagement.Scene scene) {
            // CopyConfigsToSamples(scene);
        }

        private static void CopyConfigsToSamples(UnityEngine.SceneManagement.Scene scene) {
            string sourcePath = string.Concat(Application.dataPath, "/configs");
            string targetPath = string.Concat(Application.dataPath, "/Samples/StartersKit/configs");
        
            bool succes = FileManager.CopyDirectory(sourcePath, targetPath, ".meta", true);
        
            if (succes)
                AssetDatabase.Refresh();
            else {
                Debug.LogError("Failed to copy configs to samples");
            }
        }
    }
}