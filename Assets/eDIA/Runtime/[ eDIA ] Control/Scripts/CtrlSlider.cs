using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using eDIA;

public class CtrlSlider : MonoBehaviour {

	[Header ("References")]
	public TextMeshProUGUI currentValueField;
	private Slider mySlider;

	private void Awake() {
		mySlider = GetComponent<Slider>();
		currentValueField.text = mySlider.value.ToString();
	}

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