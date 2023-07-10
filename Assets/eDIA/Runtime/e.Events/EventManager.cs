// #####################################################################################################
/*
 *  Project name  : VReha
 *  Author		  : Jeroen
 *  Description	  : Event manager handling global events
 *  Version		  : 0
 */
// #####################################################################################################

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==============================================================================================================================================
namespace eDIA {

	/// <summary>
	/// Parameter package definition to send along with an event. 
	/// </summary>
	public class eParam {
		// Empty constructor
		/// <summary>Default empty </summary>
		public eParam () { }

	#region Basic parameters

		//# Floats
		public float floatP;
		public float[] floatPs;

		/// <summary>Pass on a float</summary>
		public eParam (float _float) {
			floatP = _float;
		}

		public float GetFloat () {
			return floatP;
		}

		/// <summary>Pass on value array 'new float[] { value01, value02, .. }' </summary>
		public eParam (float[] _floats) {
			floatPs = _floats;
		}

		public float[] GetFloats () {
			return floatPs;
		}

		//# Strings
		public string stringP;
		public string[] stringPs;

		public eParam (string _string) {
			stringP = _string;
		}

		public string GetString () {
			return stringP;
		}

		/// <summary>Pass on value array "new string[] { value01, value02, .. }" </summary>
		public eParam (string[] _strings) {
			stringPs = _strings;
		}
		public string[] GetStrings () {
			return stringPs;
		}

		//# Ints
		public int intP;
		public int[] intPs;

		public eParam (int _int) {
			intP = _int;
		}

		public int GetInt () {
			return intP;
		}

		/// <summary>Pass on value array "new int[] { value01, value02, .. }" </summary>
		public eParam (int[] _ints) {
			intPs = _ints;
		}

		public int[] GetInts () {
			return intPs;
		}

		public int GetIntAt (int _index) {
			return intPs[_index];
		}

		//# Bools
		public bool boolP;
		public bool[] boolPs;

		public eParam (bool _bool) {
			boolP = _bool;
		}

		public bool GetBool () {
			return boolP;
		}

		/// <summary>Pass on value array "new bool[] { value01, value02, .. }" </summary>
		public eParam (bool[] _boolPs) {
			boolPs = _boolPs;
		}

		public bool[] GetBools () {
			return boolPs;
		}
		
		//# Vector3
		public Vector3 vector3P;
		public Vector3[] vector3Ps;

		public eParam (Vector3 _vector3) {
			vector3P = _vector3;
		}

		public Vector3 GetVector3 () {
			return vector3P;
		}

		//# Object container
		public object objectP;

		public eParam (object _objectP) {
			objectP = _objectP;
		}

		public object GetObject () {
			return objectP;
		}

		//# Transform container
		public Transform transformP;

		public eParam (Transform _transformP) {
			transformP = _transformP;
		}

		public Transform GetTransform () {
			return transformP;
		}

		//# StringBool
		public class StringBool {
			public string stringP;
			public bool boolP;
		}

		public StringBool stringBool = new StringBool();
		public eParam (string _string, bool _bool) {
			stringBool.stringP 	= _string;
			stringBool.boolP 		= _bool;
		}

		public string GetStringBool_String () {
			return stringBool.stringP;
		}

		public bool GetStringBool_Bool () {
			return stringBool.boolP;
		}


	}

	#endregion

	// ==============================================================================================================================================

	/// <summary>
	/// EventManager handles eventlisteners and fire events when triggered.<br/>
	/// It uses <c>eparam</c> class to be able to send custom packages along with the event.
	/// </summary>
	[System.Serializable]
	public class EventManager {
		private static Dictionary<string, Action<eParam>> eventDictionary = new Dictionary<string, Action<eParam>> ();

		public static bool showLog { get; set; } = false;

		/// <summary>
		/// Starts a listener to the given <c>eventName</c> string
		/// </summary>
		/// <param name="eventName">String definition of the event</param>
		/// <param name="listener">Method to trigger when event is fired</param>
		public static void StartListening (string eventName, Action<eParam> listener) {
			Action<eParam> thisEvent;

			if (eventDictionary.TryGetValue (eventName, out thisEvent)) {
				if (showLog)
					UnityEngine.Debug.Log("<color=#00ff00>[ + ]</color> " + eventName);

				//Add more event to the existing one
				thisEvent += listener;

				//Update the Dictionary
				eventDictionary[eventName] = thisEvent;
			} else {
				//Add event to the Dictionary for the first time
				thisEvent += listener;
				eventDictionary.Add (eventName, thisEvent);
			}

		}

		/// <summary>
		/// Stops the listener, if any, to the given <c>eventName</c> string
		/// </summary>
		/// <param name="eventName">String definition of the event</param>
		/// <param name="listener">Method to trigger when event is fired</param>
		public static void StopListening (string eventName, Action<eParam> listener) {
			//if (eventManager == null) return;
			Action<eParam> thisEvent;

			if (eventDictionary.TryGetValue (eventName, out thisEvent)) {
				if (showLog)
					UnityEngine.Debug.Log("<color=#FF0000>[ - ]</color> " + eventName);
				//Remove event from the existing one
				thisEvent -= listener;

				//Update the Dictionary
				eventDictionary.Remove(eventName);

			}
		}

		/// <summary>
		/// Triggers the event with the given eventname
		/// </summary>
		/// <param name="eventName">String definition of the event</param>
		/// <param name="eventParam">Parameter package to pass along</param>
		public static void TriggerEvent (string eventName, eParam eventParam) {
		
			Action<eParam> thisEvent = null;

			if (eventDictionary.TryGetValue (eventName, out thisEvent)) {
				if (showLog)
					Debug.Log ("<color=#00ff00>[]> </color>" + eventName);
				thisEvent.Invoke (eventParam);
			} else {
				Debug.Log("No listener for:" + eventName);
			}
		}

		public static void TriggerEvent (string eventName) {
			TriggerEvent (eventName, null);
		}

		internal static void StartListening(object evPointMode)
		{
			throw new NotImplementedException();
		}

		internal static void TriggerEvent(string evProceed, object onEvProceed)
		{
			throw new NotImplementedException();
		}
	}
}
