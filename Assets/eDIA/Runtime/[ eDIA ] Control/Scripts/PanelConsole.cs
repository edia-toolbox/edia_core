using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Net;
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

namespace eDIA.Manager
{

	public class PanelConsole : MonoBehaviour
	{

		[HideInInspector]
		public GameObject holder = null;
		public TextMeshProUGUI textfield;
		public int maxLines = 20;

		private List<string> consoleLog = new List<string>();

		private void Awake() {
			textfield.text = "";
		}

		public void Add2Console(string msg)
		{
			consoleLog.Insert(0,msg);
			textfield.text = GetConsoleLogDataTrimmed();
		}

		public List<string> GetConsoleLogData () {
			return consoleLog;
		}

		string GetConsoleLogDataTrimmed () {

			string tmp = "";
			
			for (int i=0;i<consoleLog.Count;i++)
				if (i < maxLines) {
					tmp = consoleLog[i] + "\n" + tmp;
				}

			return tmp;
		}

		public void ShowConsole(bool onOff)
		{
			holder.SetActive(onOff);
		}

		[ContextMenu("ShowConsole")]
		public void ShowConsole()
		{
			ShowConsole(true);
		}

		[ContextMenu("HideConsole")]
		public void HideConsole()
		{
			ShowConsole(false);
		}

		public void Toggle()
		{
			ShowConsole(!holder.gameObject.activeSelf);
		}

	}

}
