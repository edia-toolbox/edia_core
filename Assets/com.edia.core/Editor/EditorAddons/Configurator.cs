using UnityEngine;
using UnityEditor;
using System.IO;

namespace Edia.Editor.Utils {
    [InitializeOnLoad]
    public class Configurator : EditorWindow {

        // Public
        public static Texture2D EDIAIcon;

        // Internal
        [SerializeField] private ThemeDefinition SelectedTheme;
        private                  Vector2         _scrollPos;
        private static           ThemeDefinition _selectedTheme;
        private static           string          _version;

        [System.Serializable]
        private class PackageJson {
            public string version;
        }

        // EditorPrefs keys
        private const string ThemeGuidKey       = "EDIA_SelectedThemeGuid";

        void OnEnable() {
            EDIAIcon ??= Resources.Load<Texture2D>("IconEdia");
            LoadSettings();
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
        
        private static void CreateInitialTheme() {
            string sourcePath = AssetDatabase.GetAssetPath(Resources.Load<ThemeDefinition>("EdiaDefaultTheme"));
            string targetPath = "Assets/ProjectColorTheme.asset";
            AssetDatabase.CopyAsset(sourcePath, targetPath);
            AssetDatabase.Refresh();

            _selectedTheme        = AssetDatabase.LoadAssetAtPath<ThemeDefinition>(targetPath);
            Constants.ActiveTheme = _selectedTheme; // Fires the event to force UI items to update

            Debug.Log($"Created & applied initial UI color theme at {targetPath}");
            SaveSettings();
        }

        private static void SaveSettings() {
            if (_selectedTheme is not null) {
                string themePath = AssetDatabase.GetAssetPath(_selectedTheme);
                string themeGuid = AssetDatabase.AssetPathToGUID(themePath);
                EditorPrefs.SetString(ThemeGuidKey, themeGuid);
            }
        }

        private static void LoadSettings() {
            string themeGuid = EditorPrefs.GetString(ThemeGuidKey, "");
            if (!string.IsNullOrEmpty(themeGuid)) {
                string themePath = AssetDatabase.GUIDToAssetPath(themeGuid);
                if (!string.IsNullOrEmpty(themePath)) {
                    _selectedTheme        = AssetDatabase.LoadAssetAtPath<ThemeDefinition>(themePath);
                    Constants.ActiveTheme = _selectedTheme; // Set current theme which updates all UI elements having a themehandler
                }
                else {
                    Debug.Log("Color theme asset not found.");
                }
            }
            else {
                Debug.Log("Empty ThemeGuid");
            }
        }

        [MenuItem("EDIA/Configurator", false, 0)]
        static void ShowConfigurator() {
            var window = (Configurator)EditorWindow.GetWindow(typeof(Configurator), false, "Configurator");
            window.minSize      = new Vector2(300, 400);
            window.titleContent = new GUIContent("Configurator");

            // Check if there is a theme assigned
            if (Constants.ActiveTheme is null) {
                CreateInitialTheme();
            }
            else LoadSettings();

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

            GUIStyle labelHeader = new GUIStyle(EditorStyles.label);
            labelHeader.wordWrap = true;
            labelHeader.font     = Resources.Load<Font>("bahnschrift");
            labelHeader.fontSize = 22;

            GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
            centeredStyle.alignment = TextAnchor.MiddleCenter;
            centeredStyle.font      = Resources.Load<Font>("bahnschrift");

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.wordWrap  = true;
            labelHeader.font   = Resources.Load<Font>("bahnschrift");
            boxStyle.alignment = TextAnchor.MiddleCenter;
            boxStyle.border    = new RectOffset(2, 2, 2, 2);

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
            GUILayout.Label("EDIA components depend on mandatory layers in order to function properly.", boxStyle);
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
            GUILayout.Label("UI color theme.", labelContent);
            GUILayout.Label("EDIA provides a customizable UI color theme. The theme is applied to all EDIA UI elements in the project.", boxStyle);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            _selectedTheme = EditorGUILayout.ObjectField("Color Theme", _selectedTheme, typeof(ThemeDefinition), false) as ThemeDefinition;
            if (EditorGUI.EndChangeCheck()) {
                SaveSettings();
            }

            if (GUILayout.Button("APPLY")) {
                Constants.ActiveTheme = _selectedTheme; // Fires the event to force UI items to update
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(40);
            if (GUILayout.Button("Debug")) {
                EditorPrefs.DeleteKey("Initalized");
                Debug.Log(EditorPrefs.HasKey("Initalized"));
            }

            EditorGUILayout.EndScrollView();
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