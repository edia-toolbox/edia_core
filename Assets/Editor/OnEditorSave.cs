using Edia.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Edia.EditorUtils {
    [InitializeOnLoad]
    public static class OnEditorSave {
        static OnEditorSave() {
            EditorSceneManager.sceneSaved += OnSceneSaved;
        }

        private static void OnSceneSaved(UnityEngine.SceneManagement.Scene scene) {
            CopyConfigsToSamples(scene);
        }

        // Copies
        private static void CopyConfigsToSamples(UnityEngine.SceneManagement.Scene scene) {
            string sourcePath = string.Concat(Application.dataPath, "/configs");
            string targetPath = string.Concat(Application.dataPath, "/Samples/configs");
            
            bool succes = FileManager.CopyDirectory(sourcePath, targetPath, ".meta"); 
            
            if (succes)
                AssetDatabase.Refresh();
            
            Debug.Log(string.Concat($"[{ColorTools.AddColor("Edia-Automator", Color.yellow)}]: >> Configs copied to samples ",
                succes ? "successfully".AddColor(ColorTools.Green) : "failed!".AddColor(ColorTools.Red)));
        }
    }
}