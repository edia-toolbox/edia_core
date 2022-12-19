using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

namespace eDIA.Manager {

	public class PanelConsole : MonoBehaviour
	{
		public TextMeshProUGUI textfield = null;

		private void Awake() {
			textfield = GetComponentInChildren<TextMeshProUGUI>();
		}

		public void Add2Console (string msg) {
			textfield.text = msg + "\n" + textfield.text;
		}

		public void ShowConsole (bool onOff) {
			gameObject.SetActive(onOff);
		}

		[ContextMenu("ShowConsole")]
		public void ShowConsole() {
			ShowConsole(true);
		}

		[ContextMenu("HideConsole")]
		public void HideConsole () {
			ShowConsole(false);
		}

	}
	
}
