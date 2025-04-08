using System.Collections.Generic;
using UnityEngine;

namespace Edia {

    public static class LogUtilities {
        
        private static Dictionary<string, Color> logColors = new Dictionary<string, Color>();
        
        public static void AddToConsoleLog (string message, string indicator) {
            Debug.Log( string.Format("[<b><color=#" +  ColorUtility.ToHtmlStringRGBA(logColors.ContainsKey(indicator)? logColors.GetValueOrDefault(indicator) : GenerateNewColor(indicator)) + ">{0}</color></b>] {1}", indicator, message) );
        }

        private static Color GenerateNewColor(string indicator) {
            Color newColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            logColors.Add(indicator, newColor);
            return newColor;
        }

        public static void AddToConsoleLog (string message, string indicator, Color color) {
            Debug.Log( string.Format("[<b><color=#" +  ColorUtility.ToHtmlStringRGBA(color) + ">{0}</color></b>] {1}", indicator, message) );
        }
    }

}
