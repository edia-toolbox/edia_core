using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Edia.Utilities {

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
	


	public static class ColorTools {

		public static UnityEngine.Color Red = new UnityEngine.Color(0.5f, 0f, 0f);
		public static UnityEngine.Color Green = new UnityEngine.Color(0f, 0.5f, 0.21f);
		public static UnityEngine.Color Blue = new UnityEngine.Color(0f, 0.26f, 0.5f);
		public static UnityEngine.Color Orange = new UnityEngine.Color(0.5f, 0.36f, 0f);
		public static UnityEngine.Color Yellow = new UnityEngine.Color(0.5f, 0.49f, 0f);
		public static UnityEngine.Color Purple = new UnityEngine.Color(0.35f, 0f, 0.5f);
		
		/// <summary> Adds HTML (predefined Edia) color code around given string</summary>
		/// <param name="text">String to colorize</param>
		/// <param name="color">Color to use</param>
		/// <returns>Altered string with HTML <color=#.... > </returns>
		public static string AddColor(this string text, UnityEngine.Color color) {
			return string.Concat($"<color=#{ColorUtility.ToHtmlStringRGB(color)}", $">{text}</color>");
		}
	}
}