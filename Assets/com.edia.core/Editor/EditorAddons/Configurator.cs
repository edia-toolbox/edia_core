using UnityEngine;
using UnityEditor;
using System.IO;

namespace Edia.Editor.Utils {
    [InitializeOnLoad]
    public class Configurator : EditorWindow {

        // Public
        public Texture2D      EDIAIcon;
        public Texture2D      projectIcon;
        // ApiCompatibilityLevel targetApiLevel = ApiCompatibilityLevel.NET_4_6;

        // Internal
        [SerializeField] private ThemeDefinition SelectedTheme;
        private                  Vector2         scrollPos;
        private                  string          projectName;
        private                  ThemeDefinition selectedTheme;
        private static           string          version;

        [System.Serializable]
        private class PackageJson {
            public string version;
        }

        // EditorPrefs keys
        private const string ThemeGuidKey       = "EDIA_SelectedThemeGuid";
        private const string ProjectIconPathKey = "EDIA_ProjectIconPath";
        private const string ProjectNameKey     = "EDIA_ProjectName";

        void OnEnable() {
            LoadSettings();
        }

        static Configurator() {
#if UNITY_2018_1_OR_NEWER
            EditorApplication.projectChanged += OnProjectChanged;
#else
            EditorApplication.projectWindowChanged += OnProjectChanged;
#endif
        }

        /// <summary> Automatically show configurator if it has not been shown already </summary>
        private static void OnProjectChanged() {
            if (!EditorPrefs.HasKey("Initalized")) {
                EditorPrefs.SetBool("Initalized", true);
                Init();
            }
        }

        void LoadSettings() {
            string themeGuid = EditorPrefs.GetString(ThemeGuidKey, "");
            if (!string.IsNullOrEmpty(themeGuid)) {
                string themePath = AssetDatabase.GUIDToAssetPath(themeGuid);
                selectedTheme         = AssetDatabase.LoadAssetAtPath<ThemeDefinition>(themePath);
                Constants.ActiveTheme = selectedTheme; // Fires the event to force UI items to update
            }

            string iconPath = EditorPrefs.GetString(ProjectIconPathKey, "");
            if (!string.IsNullOrEmpty(iconPath)) {
                projectIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            }

            string projectName = EditorPrefs.GetString(ProjectNameKey, "");
            if (!string.IsNullOrEmpty(projectName)) {
                this.projectName = projectName;
            }
        }

        void SaveSettings() {
            if (selectedTheme is not null) {
                string themePath = AssetDatabase.GetAssetPath(selectedTheme);
                string themeGuid = AssetDatabase.AssetPathToGUID(themePath);
                EditorPrefs.SetString(ThemeGuidKey, themeGuid);
            }

            if (projectIcon is not null) {
                string iconPath = AssetDatabase.GetAssetPath(projectIcon);
                EditorPrefs.SetString(ProjectIconPathKey, iconPath);
            }

            if (!string.IsNullOrEmpty(projectName)) {
                EditorPrefs.SetString(ProjectNameKey, projectName);
            }
        }

        [MenuItem("EDIA/Configurator")]
        static void Init() {
            var window = (Configurator)EditorWindow.GetWindow(typeof(Configurator), false, "Configurator");
            window.minSize      = new Vector2(300, 400);
            window.titleContent = new GUIContent("Configurator");
            window.Show();
            
            string scriptPath  = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(window));
            string packagePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(scriptPath), "../../../com.edia.core/package.json"));
            if (File.Exists(packagePath)) {
                string jsonContent = File.ReadAllText(packagePath);
                var    packageData = JsonUtility.FromJson<PackageJson>(jsonContent);
                version = packageData.version;
            }
            else {
                version = "?.?.?";
            }
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

            // Logo and header
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var rect = GUILayoutUtility.GetRect(96, 96, GUI.skin.box);
            if (EDIAIcon)
                GUI.DrawTexture(rect, EDIAIcon, ScaleMode.ScaleToFit);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"EDIA version {version} \nUnity Toolbox for XR Research", centeredStyle);
            GUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

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

            GUILayout.Space(20);
            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();
            projectName = EditorGUILayout.TextField(new GUIContent("Project Name", "Enter the name of your project"), projectName);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Project Icon", GUILayout.Width(100));
            projectIcon = (Texture2D)EditorGUILayout.ObjectField(projectIcon, typeof(Texture2D), false);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck()) {
                SaveSettings();
            }

            EditorGUILayout.Separator();
            GUILayout.Label("EDIA requires a few mandatory layers to function properly!", labelContent);

            if (GUILayout.Button("Create layers")) {
                LayerTools.SetupLayers();
            }

            EditorGUILayout.Separator();
            GUILayout.Label("EDIA provides an optional folder structure guide.", labelContent);
            if (GUILayout.Button("Create folder structure")) {
                CreateFolderStructure(projectName);
            }

            GUILayout.Space(20);
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("UI Theme Settings", labelHeader);
            EditorGUILayout.Separator();
            GUILayout.Label("EDIA provides a customizable color theme.", labelContent);

            EditorGUI.BeginChangeCheck();
            selectedTheme = EditorGUILayout.ObjectField("Color Theme", selectedTheme, typeof(ThemeDefinition), false) as ThemeDefinition;
            if (EditorGUI.EndChangeCheck()) {
                SaveSettings();
            }


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create New Theme")) {
                string path = EditorUtility.SaveFilePanelInProject(
                    "Create UI Color Theme",
                    "ColorTheme",
                    "asset",
                    "Create a new UI Color Theme asset");

                if (!string.IsNullOrEmpty(path)) {
                    ThemeDefinition newTheme = ScriptableObject.CreateInstance<ThemeDefinition>();
                    AssetDatabase.CreateAsset(newTheme, path);
                    AssetDatabase.SaveAssets();

                    selectedTheme = newTheme;
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = newTheme;
                }
            }

            if (GUILayout.Button("APPLY")) {
                Constants.ActiveTheme = selectedTheme; // Fires the event to force UI items to update
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Debug")) {
                EditorPrefs.DeleteKey("Initalized");
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