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

	Coroutine timer = null;
	Slider mySlider = null;

	private void Awake() {
		mySlider = GetComponent<Slider>();	
	}

	public int maxValue {
		get {
			return (int)mySlider.maxValue;
		}

		set {
			mySlider.maxValue = value;
			maxValueField.text = value.ToString();
		}
	}

	public int currentValue {
		get {
			return (int)mySlider.value;
		}

		set {
			mySlider.value = value;
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


	public void StartAnimation (float duration) {
		gameObject.SetActive(true);
		timer = StartCoroutine("AnimateSliderOverTime", duration);
	}

	public void StopAnimation () {
		StopCoroutine( "AnimateSliderOverTime");
		mySlider.value = 0f;
	}

	IEnumerator AnimateSliderOverTime(float duration)
	{
		float animationTime = 0f;

		while (animationTime < duration)
		{
			animationTime += Time.deltaTime;
			float lerpValue = animationTime / duration;
			mySlider.value = Mathf.Lerp(1f, 0f, lerpValue);
			yield return null;
		}
	}

}
