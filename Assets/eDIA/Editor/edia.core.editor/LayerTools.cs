using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace eDIA.EditorUtils
{
	public static class LayerTools
	{
		static int failed;

		public static void SetupLayers()
		{
			Debug.Log("<color=#00FFFF>[eDIA]</color> Creating layers ");

			Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
			failed = 0;

			if (asset != null && asset.Length > 0)
			{
				SerializedObject serializedObject = new SerializedObject(asset[0]);
				SerializedProperty layers = serializedObject.FindProperty("layers");

				// Add your layers here, these are just examples. Keep in mind: indices below 6 are the built in layers.
				AddLayerAt(layers, 3, "Hidden", false);
				AddLayerAt(layers, 6, "ControlUI", false);
				AddLayerAt(layers, 7, "CamOverlay", false);
				AddLayerAt(layers, 8, "GazeCollision", false);
				AddLayerAt(layers, 31, "Teleport", false);

				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
			} else {
				Debug.LogError("TagManager.asset not loaded");
				return;
			}

			Debug.Log(string.Format("<color=#00FFFF>[eDIA]</color>Layers created with {0} errors", failed));
		}

		static bool DoesLayerExists (SerializedProperty layers, int index, string layerName) {
			return layers.GetArrayElementAtIndex(index).stringValue == layerName;
		}

		static void AddLayerAt(SerializedProperty layers, int index, string layerName, bool tryOtherIndex = true)
		{
			if (!DoesLayerExists(layers, index,layerName)) {

				var element = layers.GetArrayElementAtIndex(index);

				if (string.IsNullOrEmpty(element.stringValue))
				{
					element.stringValue = layerName;
					Debug.Log("<i>" + layerName + "</i> added on index " + index);
				}
				else
				{
					failed++;
					Debug.LogError("Creating <i>" + layerName + "</i> on layer " + index + " failed. Layer contains: <i>" + element.stringValue + "</i>. Please reassign your objects to a layer between 11-30");
				}
			} else {
				Debug.Log("<i>" + layerName + " </i> already exists on layer " + index + ", skipping");
			}

		}

	}
}