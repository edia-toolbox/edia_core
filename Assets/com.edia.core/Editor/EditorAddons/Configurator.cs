using UnityEngine;
using UnityEditor;
using System.IO;

namespace Edia.Editor.Utils {
    [InitializeOnLoad]
    public class Configurator : EditorWindow {

        // Public
        public static Texture2D EDIAIcon;

        // Internal
        private                  Vector2         _scrollPos;
        private static           ThemeDefinition _selectedTheme;
        private static           string          _version;

        [System.Serializable]
        private class PackageJson {
            public string version;
        }

        void OnEnable() {
            EDIAIcon ??= Resources.Load<Texture2D>("IconEdia");
        }

        static Configurator() {
            EditorApplication.projectChanged += OnProjectChanged;
        }

        /// <summary> Automatically show configurator if it has not been shown already </summary>
        private static void OnProjectChanged() {
            if (!EditorPrefs.HasKey("Initalized")) {
                EditorPrefs.SetBool("Initalized", true);
                ShowConfigurator();
            }
        }

        public static void UnsubscribeFromEvents() {
            EditorApplication.projectChanged -= OnProjectChanged;
        }

        [MenuItem("EDIA/Configurator", false, 0)]
        static void ShowConfigurator() {
            var window = (Configurator)EditorWindow.GetWindow(typeof(Configurator), false, "Configurator");
            window.minSize      = new Vector2(300, 400);
            window.titleContent = new GUIContent("Configurator");

            // Find version of package
            string scriptPath  = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(window));
            string packagePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(scriptPath), "../../../com.edia.core/package.json"));
            if (File.Exists(packagePath)) {
                string jsonContent = File.ReadAllText(packagePath);
                var    packageData = JsonUtility.FromJson<PackageJson>(jsonContent);
                _version = packageData.version;
            }
            else {
                _version = "?.?.?";
            }

            window.Show();
        }


        private void OnGUI() {
            // Styles
            GUIStyle labelContent = new GUIStyle(EditorStyles.label);
            labelContent.fontSize = 13;
            labelContent.wordWrap = true;
            
            GUIStyle labelHeader = new GUIStyle(EditorStyles.label);
            labelHeader.wordWrap = true;
            labelHeader.font     = Resources.Load<Font>("bahnschrift");
            labelHeader.fontSize = 22;

            GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
            centeredStyle.alignment = TextAnchor.MiddleCenter;
            centeredStyle.font      = Resources.Load<Font>("bahnschrift");

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.wordWrap = true;
            boxStyle.font = Resources.Load<Font>("bahnschrift");
            boxStyle.alignment = TextAnchor.MiddleCenter;
            boxStyle.border = new RectOffset(2, 2, 2, 2);
            boxStyle.stretchWidth = true;
            boxStyle.stretchHeight = true;
            boxStyle.fixedWidth = 0;
            boxStyle.fixedHeight = 0;

            // Logo and header
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var rect = GUILayoutUtility.GetRect(96, 96, GUI.skin.box);
            if (EDIAIcon)
                GUI.DrawTexture(rect, EDIAIcon, ScaleMode.ScaleToFit);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Label($"EDIA version {_version} \nUnity Toolbox for XR Research", centeredStyle);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false);

            // Edia details
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.BeginVertical();
            
            GUILayout.Space(20);
            EditorGUILayout.Separator();
            GUILayout.Label("Documentation", labelHeader);
            EditorGUILayout.Separator();
            if (GUILayout.Button("Getting started"))
                Application.OpenURL("https://edia-toolbox.github.io/gettingstarted/");

            if (GUILayout.Button("API Reference"))
                Application.OpenURL("https://edia-toolbox.github.io/apiref/");

            GUILayout.Space(20);
            EditorGUILayout.Separator();
            GUILayout.Label("Examples", labelHeader);
            EditorGUILayout.Separator();
            GUILayout.Label("Each EDIA module comes with samples. \nConsult the `samples` area the package manager.", labelContent);

            if (GUILayout.Button("Open Package Manager"))
                EditorApplication.ExecuteMenuItem("Window/Package Manager");

            // Project settings ------------------------------------------------------
            GUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Your Project", labelHeader);

            GUILayout.Space(10);
            GUILayout.Label("Layer setup.", labelContent);
            GUILayout.Label("EDIA components depend on mandatory layers in order to function properly.\nEDIA will auto-create&overwrite necessary layers on runtime.", boxStyle);
            if (GUILayout.Button("Create layers")) {
                Edia.Utilities.LayerTools.SetupLayers();
            }

            GUILayout.Space(10);
            EditorGUILayout.Separator();
            GUILayout.Label("Folder structure guide.", labelContent);
            GUILayout.Label("To keep your project organised from the start we provide a suggestion for a folder structure", boxStyle);
            if (GUILayout.Button("Create folder structure")) {
                CreateFolderStructure("ProjectName");
            }

            GUILayout.Space(10);
            GUILayout.Label("UI color theme.");
            GUILayout.Label("EDIA provides a customizable UI color theme. The theme is applied to all EDIA UI elements in the project.",boxStyle);

            EditorGUILayout.BeginHorizontal(); 
            GUILayout.Label($"Last applied theme: {(Constants.ActiveTheme is not null ? Constants.ActiveTheme.name : "None")}");
            if (GUILayout.Button("Apply", GUILayout.Width(80))) {
                string themePath = AssetDatabase.GetAssetPath(Constants.ActiveTheme);
                Constants.ApplyTheme(themePath); 
            }
            if (GUILayout.Button("Create", GUILayout.Width(80))) {
                CreateNewTheme();
            }
            
            GUILayout.Space(30);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
            GUILayout.Space(30);
        }

        static void CreateNewTheme() {
            var theme = ScriptableObject.CreateInstance<ThemeDefinition>();
            AssetDatabase.CreateAsset(theme, "Assets/Settings/ProjectColorTheme.asset");
            AssetDatabase.SaveAssets();
            Selection.activeObject = theme;
        }
        
        static void CreateFolderStructure(string projectName) {
            FileManager.CreateFolder(projectName);
            FileManager.CreateFolder(projectName + "/Assets2D");
            FileManager.CreateFolder(projectName + "/Assets2D/Textures");
            FileManager.CreateFolder(projectName + "/Assets2D/Sprites");
            FileManager.CreateFolder(projectName + "/Prefabs");
            FileManager.CreateFolder(projectName + "/Assets3D");
            FileManager.CreateFolder(projectName + "/Assets3D/Models");
            FileManager.CreateFolder(projectName + "/Assets3D/Src");
            FileManager.CreateFolder(projectName + "/Materials");
            FileManager.CreateFolder(projectName + "/Scripts");
            FileManager.CreateFolder(projectName + "/Scenes");
            FileManager.CreateFolder("Editor");
            FileManager.CreateFolder("Settings");
            FileManager.CreateFolder("ThirdParty");
            AssetDatabase.Refresh();
        }

    }
}