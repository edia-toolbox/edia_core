using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ManualVersionUpdater : EditorWindow {
    private string versionNumber = "";

    [MenuItem("EDIA/Tools/VersionUpdater")]
    public static void ShowWindow() {
        // Get existing open window or create a new one
        EditorWindow.GetWindow(typeof(ManualVersionUpdater), false, "Version Updater");
    }

    void OnEnable() {
        // Load the current version when the window is opened
        versionNumber = PlayerSettings.bundleVersion;
    }

    void OnGUI() {
        GUILayout.Label("Version Updater", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Current Version:", GUILayout.Width(100));
        EditorGUILayout.LabelField(PlayerSettings.bundleVersion);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("New Version:", GUILayout.Width(100));
        versionNumber = EditorGUILayout.TextField(versionNumber);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Update Version")) {
            // Validate and update version
            if (!string.IsNullOrEmpty(versionNumber)) {
                PlayerSettings.bundleVersion = versionNumber;
                UpdatePackageJson(versionNumber);
                    
                Debug.Log("Version updated to: " + versionNumber);
            }
            else {
                EditorUtility.DisplayDialog("Error", "Version number cannot be empty", "OK");
            }
        }
    }
    private void UpdatePackageJson(string newVersion) {
        string[] files = System.IO.Directory.GetFiles(Application.dataPath, "package.json", System.IO.SearchOption.AllDirectories);

        foreach (string file in files) {
            string json = System.IO.File.ReadAllText(file);
            if (json.Contains("\"name\": \"com.edia")) {
                try {
                    var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    if (jsonObject != null && jsonObject.ContainsKey("version")) {
                        jsonObject["version"] = newVersion;
                        string updatedJson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);
                        System.IO.File.WriteAllText(file, updatedJson);
                        Debug.Log($"Updated package.json at {file} to version: {newVersion}");
                    }
                } catch (System.Exception e) {
                    Debug.LogError($"Failed to update version in package.json at {file}. Error: {e.Message}");
                }
            }
        }
    }
}