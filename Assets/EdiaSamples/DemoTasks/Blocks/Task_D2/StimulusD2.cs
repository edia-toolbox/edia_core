using Edia;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StimulusD2 : MonoBehaviour
{
	public bool IsValid { get; private set; } = false;

	public bool IsTicked = false;
	//public bool IsTicked { get { return IsTicked; } }

	public bool IsSelected = false;

	public Color IdleColor = Color.white;
	public Color TickedColor = Color.white;
	public Color HighlightedColor = Color.white;
	public TextMeshProUGUI ValueFieldObj;

    List<string> _validTexts = new List<string>() { "II\nd\n ", "\nd\nII", "I\nd\nI" };
	List<string> _invalidTexts = new List<string>() { "II\np\n ", "I\np\n ", "I\np\nI", "\np\nI", "\np\nII", 
		"I\nd\n ", "\nd\nI "};

	//void OnEnable() {
	//	GetComponent<Button>().onClick.AddListener(() => this.OnButtonClick());
	//}

	//void OnDisable() {
	//	GetComponent<Button>().onClick.RemoveListener(() => this.OnButtonClick());
	//}

	public void SetValid (bool isValid) {
		this.IsValid = isValid;
		ValueFieldObj.text = isValid ? _validTexts[Random.Range(0, _validTexts.Count)] : _invalidTexts[Random.Range(0, _invalidTexts.Count)];
	}

	public void OnButtonClick () {
		if (!IsSelected)
			return;

		IsTicked = !IsTicked;
		GetComponent<Image>().color = IsTicked ? TickedColor : IdleColor;
	}

	public void OnPointerEnter () {
		GetComponent<Image>().color = HighlightedColor;
		IsSelected = true;
	}

	public void OnPointerExit () {
		GetComponent<Image>().color = IsTicked ? TickedColor : IdleColor;
		IsSelected = false;
	}

	public void Reset() {
		IsValid = false;
		IsTicked = false;
		GetComponent<Image>().color = IdleColor;
	}

	/// <summary>
	/// Converts state of stimuli into bool array
	/// </summary>
	/// <returns>[0] IsValid, [1] IsTicked</returns>
	public bool[] GetResult () {
		return new bool[] { IsValid, IsTicked };
	}
}
