using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using eDIA;

public class CtrlSlider : MonoBehaviour {

	[Header ("References")]
	public TextMeshProUGUI currentValueField;

	public int currentValue {
		get {
			return (int)GetComponent<Slider>().value;
		}

		set {
			GetComponent<Slider>().value = value;
			currentValueField.text = value.ToString();
		}
	}

	public void UpdateCurrentvalueField () {
		currentValueField.text = GetComponent<Slider>().value.ToString();
	}

}