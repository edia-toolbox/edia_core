using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace eDIA.EditorUtils
{
	public static class LayerTools
	{
		public static void SetupLayers()
		{
			Debug.Log("<color=#00FFFF>[eDIA]</color> Creating layers ");

			Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");

			if (asset != null && asset.Length > 0)
			{
				SerializedObject serializedObject = new SerializedObject(asset[0]);
				SerializedProperty layers = serializedObject.FindProperty("layers");

				// Add your layers here, these are just examples. Keep in mind: indices below 6 are the built in layers.
				AddLayerAt(layers, 3, "Hidden", false);
				AddLayerAt(layers, 6, "ControlUI", false);
				AddLayerAt(layers, 7, "CamOverlay", false);
				AddLayerAt(layers, 10, "GazeCollision", false);
				AddLayerAt(layers, 31, "Teleport", false);

				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
			} else {
				Debug.LogError("TagManager.asset not loaded");
				return;
			}

			Debug.Log("<color=#00FFFF>[eDIA]</color> Done");
		}

		static void AddLayerAt(SerializedProperty layers, int index, string layerName, bool tryOtherIndex = true)
		{
			// Skip if a layer with the name already exists.
			for (int i = 0; i < layers.arraySize; i++)
			{
				if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
				{
					Debug.Log("<i>" + layerName + " </i> already exists on index " + i + ", skipping");
					return;
				}
			}

			// set layer name at index
			var element = layers.GetArrayElementAtIndex(index);

			if (string.IsNullOrEmpty(element.stringValue))
			{
				element.stringValue = layerName;
				Debug.Log(layerName + " added on index " + index);
			}
			else
			{
				Debug.LogError("Creating <i>" + layerName + "</i> on " + index + " failed. Layer contains: <i>" + element.stringValue + "</i>. Please reassign your objects to a layer between 11-30");
			}
		}

	}
}