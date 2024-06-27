using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Edia.EditorUtils {

	[InitializeOnLoad]
	public class Docu : EditorWindow {

		public static bool forceShow = false;
		ApiCompatibilityLevel targetApiLevel = ApiCompatibilityLevel.NET_4_6;

		Vector2 scrollPos;

		[MenuItem("EDIA/Documentation")]
		static void Init() {
			var window = (Docu)EditorWindow.GetWindow(typeof(Docu), false, "Docu");
			window.minSize = new Vector2(300, 400);
			window.titleContent = new GUIContent("Documentation");
			window.Show();
		}

		public void OnGUI() {
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

			GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
			labelStyle.wordWrap = true;

			EditorGUILayout.Separator();

			GUILayout.Label("Editor Settings", EditorStyles.boldLabel);
			GUILayout.Label("For the framework to work, a basic set of layers is needed");

			EditorGUILayout.Separator();

			if (GUILayout.Button("Open documentation")) {
				OpenDocumentation();
			}
	
			EditorGUILayout.EndScrollView();
		}

		public static void OpenDocumentation ()
		{
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.edia.core");

			Debug.Log($"From helppanel: {packageInfo.assetPath}");
            Debug.Log($"From helppanel: {packageInfo.documentationUrl}");
            Debug.Log($"From helppanel: {packageInfo.description}");

            Application.OpenURL(packageInfo.assetPath + "/APIreference~/index.html");

            //if (!string.IsNullOrEmpty(packageInfo.documentationUrl))
            //    Application.OpenURL(packageInfo.documentationUrl);
        }
	}
}