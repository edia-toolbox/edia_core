using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UXF;

public class StimuliStroop : MonoBehaviour {

	[Header("references")]
	public Image ColorImage = null;
	public TextMeshProUGUI WordValueField = null;
	
	string value = "";
	[HideInInspector]
	public bool IsValid = false;

	public void Init(string col, string word, string target, string answer) {
		Color newColor = Color.white;
		ColorUtility.TryParseHtmlString(col, out newColor);
		IsValid = false;
		
		ColorImage.color = newColor;

		switch (target) {
			case "color":
				value = col;

				if (col == answer)
					IsValid = true;

				break;
			case "word":
				value = word;

				if (word == answer) 
					IsValid = true;

				break;
		}
	}

	public string GetValue() {
		return value;
	}
}
