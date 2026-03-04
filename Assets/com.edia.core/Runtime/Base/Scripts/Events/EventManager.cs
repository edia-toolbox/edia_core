// #####################################################################################################
/*
 *  Project name  : EDIA
 *  Author		  : Jeroen
 *  Description	  : Event manager handling global events
 *  Version		  : 1
 */
// #####################################################################################################

using System;
using System.Collections.Generic;
using UnityEngine;

// ==============================================================================================================================================
namespace Edia {

    /// <summary>
    /// Parameter package definition to send along with an event. 
    /// </summary>
    public class eParam {
        // Empty constructor
        /// <summary>Default empty </summary>
        public eParam() {
        }

#region Basic parameters

        //# Floats
        public float   floatP;
        public float[] floatPs;

        /// <summary>Pass on a float</summary>
        public eParam(float _float) {
            floatP = _float;
        }

        public float GetFloat() {
            return floatP;
        }

        /// <summary>Pass on value array 'new float[] { value01, value02, .. }' </summary>
        public eParam(float[] _floats) {
            floatPs = _floats;
        }

        public float[] GetFloats() {
            return floatPs;
        }

        //# Strings
        public string   stringP;
        public string[] stringPs;

        public eParam(string _string) {
            stringP = _string;
        }

        public string GetString() {
            return stringP;
        }

        /// <summary>Pass on value array "new string[] { value01, value02, .. }" </summary>
        public eParam(string[] _strings) {
            stringPs = _strings;
        }

        public string[] GetStrings() {
            return stringPs;
        }

        //# Ints
        public int   intP;
        public int[] intPs;

        public eParam(int _int) {
            intP = _int;
        }

        public int GetInt() {
            return intP;
        }

        /// <summary>Pass on value array "new int[] { value01, value02, .. }" </summary>
        public eParam(int[] _ints) {
            intPs = _ints;
        }

        public int[] GetInts() {
            return intPs;
        }

        public int GetIntAt(int _index) {
            if (intPs == null || (uint)_index >= (uint)intPs.Length)
                return 0;
            return intPs[_index];
        }

        //# Bools
        public bool   boolP;
        public bool[] boolPs;

        public eParam(bool _bool) {
            boolP = _bool;
        }

        public bool GetBool() {
            return boolP;
        }

        /// <summary>Pass on value array "new bool[] { value01, value02, .. }" </summary>
        public eParam(bool[] _boolPs) {
            boolPs = _boolPs;
        }

        public bool[] GetBools() {
            return boolPs;
        }

        //# Vector3
        public Vector3   vector3P;
        public Vector3[] vector3Ps;

        public eParam(Vector3 _vector3) {
            vector3P = _vector3;
        }

        public Vector3 GetVector3() {
            return vector3P;
        }

        //# Object container
        public object objectP;

        public eParam(object _objectP) {
            objectP = _objectP;
        }

        public object GetObject() {
            return objectP;
        }

        //# Transform container
        public Transform transformP;

        public eParam(Transform _transformP) {
            transformP = _transformP;
        }

        public Transform GetTransform() {
            return transformP;
        }

        //# StringBool
        public class StringBool {
            public string stringP;
            public bool   boolP;
        }

        public StringBool stringBool;

        public eParam(string _string, bool _bool) {
            stringBool = new StringBool { stringP = _string, boolP = _bool };
        }

        public string GetStringBool_String() {
            return stringBool?.stringP;
        }

        public bool GetStringBool_Bool() {
            return stringBool?.boolP ?? false;
        }

    }

#endregion

    // ==============================================================================================================================================

    /// <summary>
    /// Provides functionality for managing events using event listeners and event triggers.
    /// All methods must be called from the Unity main thread only.
    /// </summary>
    /// <remarks>
    /// The EventManager class allows registering listeners to specific events, stopping listeners,
    /// and triggering events with or without parameterized data.
    /// </remarks>
    [System.Serializable]
    public class EventManager {
        private static Dictionary<string, Action<eParam>> eventDictionary = new Dictionary<string, Action<eParam>>();

        public static bool showLog { get; set; } = false;

        /// <summary>
        /// Starts a listener to the given <c>eventName</c> string
        /// </summary>
        /// <param name="eventName">String definition of the event</param>
        /// <param name="listener">Method to trigger when event is fired</param>
        public static void StartListening(string eventName, Action<eParam> listener) {
            Action<eParam> thisEvent;

            if (eventDictionary.TryGetValue(eventName, out thisEvent)) {
                if (showLog)
                    UnityEngine.Debug.Log("<color=#00ff00>[ + ]</color> " + eventName);

                thisEvent += listener;
                eventDictionary[eventName] = thisEvent;
            }
            else {
                thisEvent += listener;
                eventDictionary.Add(eventName, thisEvent);
            }
        }

        /// <summary>
        /// Stops the listener, if any, to the given <c>eventName</c> string
        /// </summary>
        /// <param name="eventName">String definition of the event</param>
        /// <param name="listener">Method to trigger when event is fired</param>
        public static void StopListening(string eventName, Action<eParam> listener) {
            //if (eventManager == null) return;
            Action<eParam> thisEvent;

            if (eventDictionary.TryGetValue(eventName, out thisEvent)) {
                if (showLog)
                    UnityEngine.Debug.Log("<color=#FF0000>[ - ]</color> " + eventName);

                thisEvent -= listener;
                if (thisEvent == null)
                    eventDictionary.Remove(eventName);
                else
                    eventDictionary[eventName] = thisEvent;
            }
        }

        /// <summary>
        /// Triggers the event with the given eventname
        /// </summary>
        /// <param name="eventName">String definition of the event</param>
        /// <param name="eventParam">Parameter package to pass along</param>
        public static void TriggerEvent(string eventName, eParam eventParam) {
            if (!eventDictionary.TryGetValue(eventName, out var thisEvent)) {
                if (showLog)
                    Debug.Log("No listener for:" + eventName);
                return;
            }

            if (showLog)
                Debug.Log("<color=#00ff00>[]> </color>" + eventName);

            foreach (var handler in thisEvent.GetInvocationList()) {
                try {
                    ((Action<eParam>)handler)(eventParam);
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        }

        public static void TriggerEvent(string eventName) {
            TriggerEvent(eventName, null);
        }

        /// <summary>Returns subscriber count for an event (debugging).</summary>
        public static int GetListenerCount(string eventName) {
            return eventDictionary.TryGetValue(eventName, out var e) ? e.GetInvocationList().Length : 0;
        }

        /// <summary>Removes all listeners. Use on scene teardown or test cleanup.</summary>
        public static void RemoveAllListeners() {
            eventDictionary.Clear();
        }
    }
}