using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Ms C# Coding Conventions
//
// * PascalCasing
// class, record, or struct, enums
// public members of types, such as fields, properties, events, methods, and local functions
//
// * camelCasing
// private or internal fields, and prefix them with _
//
// Use implicit typing for local variables when the type of the variable is obvious from 
// the right side of the assignment, or when the precise type is not important.
// var var1 = "This is clearly a string.";
// var var2 = 27;
//

namespace eDIA.Utilities {

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
	
}