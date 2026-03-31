using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Edia.Utilities {

	public static class Log {
        
		private static Dictionary<string, Color> logColors = new Dictionary<string, Color>();
        
		public static void AddToConsoleLog (string message, string indicator) {
			Debug.Log( string.Format("[<b><color=#" +  ColorUtility.ToHtmlStringRGBA(logColors.ContainsKey(indicator)? logColors.GetValueOrDefault(indicator) : GenerateNewColor(indicator)) + ">{0}</color></b>] {1}", indicator, message) );
		}

		private static Color GenerateNewColor(string indicator) {
			// Color newColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
			Color newColor = Constants.RandomEdiaColor();
			logColors.Add(indicator, newColor);
			return newColor;
		}

		public static void AddToConsoleLog (string message, string indicator, Color color) {
			Debug.Log( string.Format("[<b><color=#" +  ColorUtility.ToHtmlStringRGBA(color) + ">{0}</color></b>] {1}", indicator, message) );
		}
	}
	
	/// <summary> Array tools </summary>
	public static class ArrayTools
	{
		/// <summary>Converts given int array into string array</summary>
		/// <param name="floats">source</param>
		/// <returns>Int array</returns>
		public static string[] ConvertIntsToStrings (int[] ints) {
			List<string> result = new List<string>();
			foreach (int i in ints.ToList<int>())
				result.Add(i.ToString());
			return result.ToArray<string>();
		}

		/// <summary>Converts given string array into int array</summary>
		/// <param name="strings">source</param>
		/// <returns>Int array</returns>
		public static int[] ConvertStringsIntoInts (string[] strings) {
			List<int> result = new List<int>();
			foreach (string s in strings.ToList<string>()) 
				result.Add(int.Parse(s));
			return result.ToArray<int>();
		}

		/// <summary>Converts given float array into string array</summary>
		/// <param name="floats">source</param>
		/// <returns>String array</returns>
		public static string[] ConvertFloatsToStrings (float[] floats) {
			List<string> result = new List<string>();
			foreach (int i in floats.ToList<float>())
				result.Add(i.ToString());
			return result.ToArray<string>();
		}

		/// <summary>Converts given string array into float array</summary>
		/// <param name="strings">source</param>
		/// <returns>Float array</returns>
		public static float[] ConvertStringsIntoFloats (string[] strings) {
			List<float> result = new List<float>();
			foreach (string s in strings.ToList<string>()) 
				result.Add(int.Parse(s));
			return result.ToArray<float>();
		}
	}
	
	/// <summary> String tools </summary>
	public static class StringTools {
		
		/// <summary> Replaces all underscores with spaces</summary>
		/// <param name="text">string</param>
		/// <returns>String without underscores</returns>
		public static string RemoveUnderscores (string text) {
			return text.Replace('_', ' ');
		}

		/// <summary> Combines array of strings into one string</summary>
		/// <param name="text">string[]</param>
		/// <returns>One string with all array values seperated by space</returns>
		public static string CombineToOneString(string[] texts) {
			string result = string.Empty;
			foreach (string s in texts)
				result += s + " ";

			return result.Substring(0, result.Length-1);
		}	
	}



}