using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA {

    public static class LogUtilities {
        public static void AddToLog (string message, string indicator, Color color) {
            Debug.Log( string.Format("[<b><color=#" +  ColorUtility.ToHtmlStringRGBA(color) + ">{0}</color></b>] {1}", indicator, message) );
        }
    }

}
