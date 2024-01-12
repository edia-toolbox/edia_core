using eDIA;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StimulusD2 : MonoBehaviour
{
	bool isValid { get; set; } = false;
	public bool IsValid { get { return isValid; } }

	bool isTicked = false;
	public bool IsTicked { get { return isTicked; } }

	public Color IdleColor = Color.white;
	public Color TickedColor = Color.white;
    public TextMeshProUGUI ValueFieldObj;

    List<string> validTexts = new List<string>() { "II\nd\n ", "\nd\nII" };
	List<string> invalidTexts = new List<string>() { "II\np\n ", "I\np\n ", "I\nd\n ", "I\np\nI", "\np\nI", "\np\nII", "I\nd\nI" };
	

	public void SetValid (bool _isValid) {
		isValid = _isValid;
		ValueFieldObj.text = _isValid ? validTexts[Random.Range(0,2)] : invalidTexts[Random.Range(0, invalidTexts.Count)];
	}

	public void OnButtonClick () {
		isTicked = !isTicked;
		GetComponent<Image>().color = isTicked ? TickedColor : IdleColor;
	}

	public void Reset() {
		isValid = false;
		isTicked = false;
		GetComponent<Image>().color = IdleColor;
	}

	/// <summary>
	/// Converts state of stimuli into bool array
	/// </summary>
	/// <returns>[0] isValid, [1] isTicked</returns>
	public bool[] GetResult () {
		return new bool[] { isValid, isTicked };
	}
}
