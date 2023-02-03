using System;
using System.IO.IsolatedStorage;
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

public class DebugConsoleDisplay : MonoBehaviour
{
	public List<string> debugs = new List<string>();

	public TextMeshProUGUI display;
	private bool isOn = true;
	private int maxLogSize = 20;

	public void ToggleConsole () {
		isOn = !isOn;

		ShowConsole();
	}

	private void OnEnable() {
		Application.logMessageReceived += HandleLog;

		ShowConsole();
	}

	private void OnDestroy() {
		Application.logMessageReceived -= HandleLog;
	}

	public void ShowConsole () {
		display.gameObject.SetActive(isOn);
		GetComponent<Image>().enabled = isOn;
	}

	void HandleLog(string logString, string stackTrace, LogType type)
	{
		if (type is not LogType.Log)
			logString = "<color=#FF0000>" + logString + "</color>";

		string[] splitString = logString.Split(char.Parse(":"));
		string debugKey = splitString[0];
		string debugValue = splitString.Length > 1 ? splitString[1] : "";

		debugs.Add(String.Concat (debugKey, debugValue));
		if (debugs.Count > maxLogSize) debugs.RemoveAt(0);

		string displayText = "";
		foreach(string s in debugs) 
		{
			displayText += s + "\n";
		}

		display.text = displayText;
	}
}



