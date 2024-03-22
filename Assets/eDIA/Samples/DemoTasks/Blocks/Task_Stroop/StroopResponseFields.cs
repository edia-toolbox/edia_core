using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UXF;

public class StroopResponseFields : MonoBehaviour {

	[Header("references")]
	public Image ColorImage = null;
	
	string value = "";
	[HideInInspector]
	public bool IsValid = false;

	public void Init(string col, string word, string target, string correctAnswer) {
		Color newColor = Color.white;
		ColorUtility.TryParseHtmlString(col, out newColor);
		IsValid = false;
		
		ColorImage.color = newColor;

		switch (target) {
			case "color":
				value = col;

				if (col == correctAnswer)
					IsValid = true;

				break;
			case "word":
				value = word;

				if (word == correctAnswer) 
					IsValid = true;

				break;
		}
	}

	public string GetValue() {
		return value;
	}
}
