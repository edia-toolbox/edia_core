using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Edia.Utilities {

	public static class ArrayTools
	{

		public static string[] ConvertIntsToStrings (int[] ints) {
			List<string> result = new List<string>();
			foreach (int i in ints.ToList<int>())
				result.Add(i.ToString());
			return result.ToArray<string>();
		}


		public static int[] ConvertStringsIntoInts (string[] strings) {
			List<int> result = new List<int>();
			foreach (string s in strings.ToList<string>()) 
				result.Add(int.Parse(s));
			return result.ToArray<int>();
		}


		public static string[] ConvertFloatsToStrings (float[] floats) {
			List<string> result = new List<string>();
			foreach (int i in floats.ToList<float>())
				result.Add(i.ToString());
			return result.ToArray<string>();
		}


		public static float[] ConvertStringsIntoFloats (string[] strings) {
			List<float> result = new List<float>();
			foreach (string s in strings.ToList<string>()) 
				result.Add(int.Parse(s));
			return result.ToArray<float>();
		}

	}

	public static class StringTools {

		public static string RemoveUnderscores (string text) {
			return text.Replace('_', ' ');
		}

		public static string CombineToOneString(string[] texts) {
			string result = string.Empty;
			foreach (string s in texts)
				result += s + " ";

			return result.Substring(0, result.Length-1);
		}	

	}
	
}