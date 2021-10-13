using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TTF
{
	public static class StringUtilities
	{
		// Renaming for neuroscientists
		public static string CamelCaseToUnderscores(string _source)
		{
			return "";
		}

		// ------------------------------------------------------------------------------------------------------------------
		#region JSON helpers

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

			Debug.Log("Return JSON: " + j);
			return j;
		}

		public static T[] getJsonArray<T>(string json)
		{
			string newJson = "{ \"array\": " + json + "}";
			Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
			return wrapper.items;
		}

		public static T[] FromJson<T>(string jsonArray)
		{
			jsonArray = WrapArray(jsonArray);
			return FromJsonWrapped<T>(jsonArray);
		}

		public static T[] FromJsonWrapped<T>(string jsonObject)
		{
			Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(jsonObject);
			return wrapper.items;
		}

		private static string WrapArray(string jsonArray)
		{
			return "{ \"items\": " + jsonArray + "}";
		}

		public static string ToJson<T>(T[] array)
		{
			Wrapper<T> wrapper = new Wrapper<T>();
			wrapper.items = array;
			return JsonUtility.ToJson(wrapper);
		}

		public static string ToJson<T>(T[] array, bool prettyPrint)
		{
			Wrapper<T> wrapper = new Wrapper<T>();
			wrapper.items = array;
			return JsonUtility.ToJson(wrapper, prettyPrint);
		}

		[Serializable]
		private class Wrapper<T>
		{
			public T[] items;
		}

		#endregion

		// ------------------------------------------------------------------------------------------------------------------
	}
}