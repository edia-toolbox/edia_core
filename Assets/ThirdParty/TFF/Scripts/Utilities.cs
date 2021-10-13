// #####################################################################################################
/*
 *  Helper methods, reachable throughout the whole realm
 */
// #####################################################################################################

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TFF
{
	public static class Utilities
	{

		/// <summary>
		/// Convert a List<string> to JSON formatted string. Format: key[0], value[1]
		/// </summary>
		public static string ConvertListToJSON(List<string> _msg)
		{
			string j = "{ ";
			while (_msg.Count > 0)
			{
				j += "\"" + _msg[0] + "\"" + ":\"" + _msg[1] + "\"";
				j += _msg.Count == 2 ? "" : ","; // no comma on last entry
				_msg.RemoveAt(0);
				_msg.RemoveAt(0);
			}
			j += " }";

			// Debug.Log("Return JSON: " + j);
			return j;
		}

	// ============================================================================================================================================================================================================
	#region Database querie helpers

			/// <summary>
			/// Shuffles the element order of the specified list.
			/// </summary>
			public static void Shuffle<T>(this IList<T> ts)
			{
					var count = ts.Count;
					var last = count - 1;
					for (var i = 0; i < last; ++i)
					{
						var r = UnityEngine.Random.Range(i, count);
						var tmp = ts[i];
						ts[i] = ts[r];
						ts[r] = tmp;
					}
			}

	#endregion

	// ============================================================================================================================================================================================================
	#region Recursive setters

		// Set all children of given gameobject to given layer
		public static void SetLayerRecursively(GameObject _go, int _layerNumber)
		{
			if (_go == null) return;
			foreach (Transform trans in _go.GetComponentsInChildren<Transform>(true))
			{
					trans.gameObject.layer = _layerNumber;
			}
		}

		// Set given gameobjects childrens meshrenderers material color to given color if they have given tagname
		public static void SetMeshColorRecursively(GameObject _go, Color _newColor, string tagName)
		{
			if (_go == null) return;
			foreach (MeshRenderer meshR in _go.GetComponentsInChildren<MeshRenderer>(true))
			{
					if (tagName == meshR.gameObject.tag)
						meshR.GetComponent<MeshRenderer>().material.color = _newColor;
			}
		}

		// Set childrens meshrenderers as shadowcasters of given gameobject
		public static void SetMeshAsCasterRecursively(GameObject _go)
		{
			if (_go == null) return;
			foreach (MeshRenderer meshR in _go.GetComponentsInChildren<MeshRenderer>(true))
			{
					meshR.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
					meshR.GetComponent<MeshRenderer>().receiveShadows = false;
			}
		}

	#endregion

	// ============================================================================================================================================================================================================
	#region Dictionary extensions

		public static void AddRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("Collection is null");
			}

			foreach (var item in collection)
			{
				if(!source.ContainsKey(item.Key)){ 
				source.Add(item.Key, item.Value);
				}
				else
				{
				// handle duplicate key issue here
				Debug.LogWarning("Found duplicate keys in Dictionary Merge:" + item.Key.ToString());
				}  
			} 
		}

	#endregion
	}
}