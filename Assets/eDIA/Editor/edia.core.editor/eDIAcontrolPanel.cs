using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace eDIA.EditorUtils {

    [InitializeOnLoad]
    public class eDIAcontrolPanel : EditorWindow {

        public Texture2D eDIAIcon;
        public static bool forceShow = false;
        ApiCompatibilityLevel targetApiLevel = ApiCompatibilityLevel.NET_4_6;

        static string version;

        Vector2 scrollPos;

        [MenuItem ("eDIA/Show control panel")]
        static void Init () {
            var window = (eDIAcontrolPanel) EditorWindow.GetWindow (typeof (eDIAcontrolPanel), false, "eDIA control panel");
            window.minSize = new Vector2 (300, 400);
            window.titleContent = new GUIContent ("eDIA control panel");
            window.Show ();

            // TODO Fix the filepath to show the version, or find another solution

            if (File.Exists("./../../VERSION.txt"))
            {
                Debug.Log("exists");
                version = File.ReadAllText("./../../VERSION.txt");
            }
            else
            {
                version = "unknown";
            }
        }

        public void OnGUI () {
            scrollPos = EditorGUILayout.BeginScrollView (scrollPos, false, false);
            GUIStyle labelStyle = new GUIStyle (EditorStyles.label);
            labelStyle.wordWrap = true;

            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            var rect = GUILayoutUtility.GetRect (128, 128, GUI.skin.box);
            if (eDIAIcon)
                GUI.DrawTexture (rect, eDIAIcon, ScaleMode.ScaleToFit);
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("eDIA VR Experiment Framework", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            GUILayout.Label ("Version " + version, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();

            EditorGUILayout.Separator ();
            
            GUILayout.Label ("Editor Settings", EditorStyles.boldLabel);
            EditorGUILayout.TextArea("For the framework to work, a basic set of layers is needed");
            
            if (GUILayout.Button ("Create layers")) {
                LayerTools.SetupLayers();
            }


            // EditorGUILayout.Separator ();

            // GUILayout.Label ("To update the changelog, hit this button", labelStyle);
            // if (GUILayout.Button ("Update Changelog from CSV"))
            //     eDIA.Utilities.ProjectFileUtilities.UpdateChangelog ();

            EditorGUILayout.Separator ();

            // GUILayout.Label("Platform selector", EditorStyles.boldLabel);
            // EditorGUILayout.HelpBox("Click the buttons below to switch to your desired output platform. You will also need to select the Data Handler(s) you wish to use in your UXF Session Component.", MessageType.Info);

            // if (GUILayout.Button("Select Windows / PC VR")) SetSettingsWindows();
            // if (GUILayout.Button("Select Web Browser")) SetSettingsWebGL();
            // if (GUILayout.Button("Select Android VR (e.g. Oculus Quest)")) SetSettingsOculus();
            // if (GUILayout.Button("Select Android")) SetSettingsAndroid();

            // EditorGUILayout.Separator();

            GUILayout.Label ("Help and info", EditorStyles.boldLabel);

            EditorGUILayout.Space ();
            GUILayout.Label ("The framework comes with documentation", labelStyle);
            if (GUILayout.Button ("Open documentation"))
                Application.OpenURL ("https://gitlab.gwdg.de/3dia/edia_framework/-/wikis/home");

            EditorGUILayout.Separator ();

            GUILayout.Label ("Examples", EditorStyles.boldLabel);
            GUILayout.Label ("Click 'import samples' from the package manager.", labelStyle);

            EditorGUILayout.Separator ();

            GUILayout.Label ("Create basic config files", EditorStyles.boldLabel);

            if (GUILayout.Button ("Create config folder + demo JSON files"))
                CreateConfigFiles ();

            EditorGUILayout.Separator ();

            // GUILayout.Label ("Compatibility", EditorStyles.boldLabel);

            // bool compatible = PlayerSettings.GetApiCompatibilityLevel (BuildTargetGroup.Standalone) == targetApiLevel;

            // if (compatible) {
            //     EditorGUILayout.HelpBox ("API Compatibility Level is set correctly", MessageType.Info);
            // } else {
            //     EditorGUILayout.HelpBox ("API Compatibility Level should be set to .NET 2.0 (Older versions of Unity) or .NET 4.x (Unity 2018.3+), expect errors on building", MessageType.Warning);
            //     if (GUILayout.Button ("Fix")) {
            //         PlayerSettings.SetApiCompatibilityLevel (BuildTargetGroup.Standalone, targetApiLevel);
            //     }
            // }
            
            

            EditorGUILayout.Separator ();
            // EditorGUILayout.HelpBox("To show this window again go to UXF -> Show setup wizard in the menubar.", MessageType.None);

            EditorGUILayout.EndScrollView ();
        }

        static void CreateConfigFiles () {
            FileManager.CreateFolder("Configs/Participants");
            FileManager.CreateFolder("Configs/Tasks");

            FileManager.CopyFileTo("../packages/eDIA/Editor/edia.core.editor/jsons","TASKA_PARTICIPANTID.json", "Configs/Participants");
            FileManager.CopyFileTo("../packages/eDIA/Editor/edia.core.editor/jsons","TASKA.json", "Configs/Tasks"); 

            FileManager.CopyFileTo("../packages/eDIA/Editor/edia.core.editor/jsons","TASKB_PARTICIPANTID.json", "Configs/Participants");
            FileManager.CopyFileTo("../packages/eDIA/Editor/edia.core.editor/jsons","TASKB.json", "Configs/Tasks");

            Debug.Log("Created Config folders with demo configuration files");
        }
       
    }

}