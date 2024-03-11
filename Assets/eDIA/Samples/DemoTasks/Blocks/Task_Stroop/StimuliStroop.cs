using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UXF;

public class StimuliStroop : MonoBehaviour {
	public bool IsValid = false;
	public Image ColorImage = null;
	public TextMeshProUGUI WordValueField = null;
	string value = "";

	public void Init(string col, string word, string target, string answer) {
		Color newColor = Color.white;
		ColorUtility.TryParseHtmlString(col, out newColor);
		IsValid = false;

		switch (target) {
			case "color":
				ColorImage.color = newColor;
				ColorImage.gameObject.SetActive(true);
				WordValueField.gameObject.SetActive(false);
				value = col;

				if (col == answer)
					IsValid = true;

				break;
			case "word":
				WordValueField.text = word;
				WordValueField.color = newColor;
				WordValueField.gameObject.SetActive(true);
				ColorImage.color = Color.white;
				ColorImage.gameObject.SetActive(true);
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
