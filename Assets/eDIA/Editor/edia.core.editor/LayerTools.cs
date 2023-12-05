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
			}

			Debug.Log("<color=#00FFFF>[eDIA]</color> Done");
		}

		static void AddLayerAt(SerializedProperty layers, int index, string layerName, bool tryOtherIndex = true)
		{
			// Skip if a layer with the name already exists.
			for (int i = 0; i < layers.arraySize; ++i)
			{
				if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
				{
					Debug.Log("Skipping layer '" + layerName + "' because it already exists.");
					return;
				}
			}

			// Extend layers if necessary
			if (index >= layers.arraySize)
				layers.arraySize = index + 1;

			// set layer name at index
			var element = layers.GetArrayElementAtIndex(index);
			if (string.IsNullOrEmpty(element.stringValue))
			{
				element.stringValue = layerName;
				Debug.Log("Added layer '" + layerName + "' at index " + index + ".");
			}
			else
			{
				Debug.LogWarning("Could not add layer at index " + index + " because there already is another layer '" + element.stringValue + "'.");

				if (tryOtherIndex)
				{
					// Go up in layer indices and try to find an empty spot.
					for (int i = index + 1; i < 32; ++i)
					{
						// Extend layers if necessary
						if (i >= layers.arraySize)
							layers.arraySize = i + 1;

						element = layers.GetArrayElementAtIndex(i);
						if (string.IsNullOrEmpty(element.stringValue))
						{
							element.stringValue = layerName;
							Debug.Log("Added layer '" + layerName + "' at index " + i + " instead of " + index + ".");
							return;
						}
					}

					Debug.LogError("Could not add layer " + layerName + " because there is no space left in the layers array.");
				}
			}
		}

	}
}