using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderExperimenterStatus : MonoBehaviour {

	[Header ("References")]
	public TextMeshProUGUI currentValueField;
	public TextMeshProUGUI maxValueField;
	public TextMeshProUGUI descriptionField;
	public bool useCapitals = true;

	public int maxValue {
		get {
			return (int)GetComponent<Slider>().maxValue;
		}

		set {
			GetComponent<Slider>().maxValue = value;
			maxValueField.text = value.ToString();
		}
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

	public string description {
		get {
			return descriptionField.text;
		}

		set {
			descriptionField.text = useCapitals ? value.ToString().ToUpper() : value.ToString();
		}
	}

	void OnEnable() {
		// currentValue = 0;
	}

}