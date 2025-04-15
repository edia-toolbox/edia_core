using UnityEngine;
using UnityEditor;
using System.IO;

namespace Edia.Editor.Utils {
    [InitializeOnLoad]
    public class Configurator : EditorWindow {
        public Texture2D EDIAIcon;
        public static bool forceShow = false;
        ApiCompatibilityLevel targetApiLevel = ApiCompatibilityLevel.NET_4_6;
        string projectName = GetProjectName();
        [SerializeField] private ColorThemeDefinition selectedColorTheme;

        Vector2 scrollPos;
        
        public static event System.Action OnThemeChanged;
        
        
        [MenuItem("EDIA/Configurator")]
        static void Init() {
            var window = (Configurator)EditorWindow.GetWindow(typeof(Configurator), false, "Configurator");
            window.minSize = new Vector2(300, 400); 
            window.titleContent = new GUIContent("Configurator");
            window.Show();
        }

        public void OnGUI() {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.wordWrap = true;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var rect = GUILayoutUtility.GetRect(128, 128, GUI.skin.box);
            if (EDIAIcon)
                GUI.DrawTexture(rect, EDIAIcon, ScaleMode.ScaleToFit);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("eDIA v0.4.0 \nUnity Toolbox for XR Research Studies", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            GUILayout.Label("Editor Settings", EditorStyles.boldLabel);
            GUILayout.Label("For the framework to work, a basic set of layers is needed");

            if (GUILayout.Button("Create layers")) {
                LayerTools.SetupLayers();
            }

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Project name:");
            projectName = EditorGUILayout.TextField(projectName);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("Click the button below to create an exemplary Unity project folder structure within the root folder of your project.", MessageType.Info);
            if (GUILayout.Button("Create folder structure")) {
                CreateFolderStructure(projectName);
            }

            EditorGUILayout.Separator();
            
            GUILayout.Label("Help and info", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            GUILayout.Label("The framework comes with documentation", labelStyle);
            if (GUILayout.Button("Open documentation"))
                Application.OpenURL("https://gitlab.gwdg.de/3dia/edia_framework/-/wikis/home");

            EditorGUILayout.Separator();

            GUILayout.Label("Examples", EditorStyles.boldLabel);
            GUILayout.Label("Click 'import samples' from the package manager.", labelStyle);

            EditorGUILayout.Separator();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("UI Theme Settings", EditorStyles.boldLabel);

// Add a field for the UIColorThemeDefinition
            ColorThemeDefinition newSelectedColorTheme = (ColorThemeDefinition)EditorGUILayout.ObjectField(
                "UI Color Theme", 
                selectedColorTheme, 
                typeof(ColorThemeDefinition), 
                false);
            
            if (newSelectedColorTheme != selectedColorTheme) {
                selectedColorTheme = newSelectedColorTheme;
                
            }

            // Add an APPLY button to fire the OnThemeChanged event
            if (GUILayout.Button("APPLY"))
            {
                Constants.ActiveTheme = selectedColorTheme;
            }

            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create New Theme"))
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "Create UI Color Theme",
                    "ColorTheme",
                    "asset",
                    "Create a new UI Color Theme asset");
            
                if (!string.IsNullOrEmpty(path))
                {
                    // Create a new instance of UIColorThemeDefinition
                    ColorThemeDefinition newTheme = ScriptableObject.CreateInstance<ColorThemeDefinition>();
            
                    // Save it to the selected path
                    AssetDatabase.CreateAsset(newTheme, path);
                    AssetDatabase.SaveAssets();
            
                    // Set it as the current theme
                    selectedColorTheme = newTheme;
            
                    // Focus the Project window and select the new asset
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = newTheme;
                }
            }

            EditorGUILayout.EndHorizontal();

            // EditorGUILayout.Separator();
            
            // EditorGUILayout.Separator();
            EditorGUILayout.EndScrollView();
        }

        public static string GetProjectName() {
            // Get the full path of the Unity project directory
            string projectDirectory = Directory.GetCurrentDirectory();

            // Extract the name of the directory (which should be the project name)
            string projectName = new DirectoryInfo(projectDirectory).Name;

            return projectName;
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

        static void SetSettingsWindows() {
            Debug.Log("SetSettingsWindows placeholder");
        }

        static void SetSettingsOculus() {
            Debug.Log("SetSettingsOculus placeholder");
        }

        static void SetSettingsAndroid() {
            Debug.Log("SetSettingsAndroid placeholder");
        }
    }
}