using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandMeshAnimator : MonoBehaviour {

#region VARIABLES

	[Header("Refs")]
	public InputActionReference gripReference = null;

	[Header("Settings")]
	public float speed = 5f;

	private Animator animator = null;

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region SETUP HAND AND FINGERS

	private readonly List<Finger> gripFingers = new List<Finger> () {
		new Finger (FingerType.Middle),
			new Finger (FingerType.Ring),
			new Finger (FingerType.Pinky)
	};

	private readonly List<Finger> pointFingers = new List<Finger> () {
		new Finger (FingerType.Index),
			new Finger (FingerType.Thumb),
	};

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region STARTERS

	private void Awake () {
		animator = GetComponent<Animator> ();
	}

	private void Start()
	{
	    	gripReference.action.performed += GripPerformed;
	}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EVENTHANDLERS

	void GripPerformed (InputAction.CallbackContext context) {
		SetFingerTargets (gripFingers, context.ReadValue<float>());
	}

	private void Update () {
		// Smooth input values
		// SmoothFinger (gripFingers);
		// SmoothFinger(pointFingers);

		// Apply smooth values
		// AnimateFinger(pointFingers);
		// AnimateFinger (gripFingers);
	}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HELPERS

	private void SetFingerTargets (List<Finger> fingers, float value) {
		foreach (Finger finger in fingers)
			finger.target = value;
	}

	private void SmoothFinger (List<Finger> fingers) {
		foreach (Finger finger in fingers) {
			float time = speed * Time.unscaledDeltaTime;
			finger.current = Mathf.MoveTowards (finger.current, finger.target, time);
		}
	}

	private void AnimateFinger (List<Finger> fingers) {
		foreach (Finger finger in fingers)
			AnimateFinger (finger.type.ToString (), finger.current);
	}

	private void AnimateFinger (string finger, float blend) {
		animator.SetFloat (finger, blend);

	}

	//! Old and probably not going to use? Or maybe when we need pointing again.
	// private void CheckPointer()
	// {
	//     if (controller.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float pointerValue))
	//             SetFingerTargets(pointFingers, pointerValue);
	// }

	// public void SetHandPosePointing () {
	//     StartCoroutine(SetHandPose(1f));
	// }

	// IEnumerator SetHandPose (float _value) {
	// 	float currentValue = 0f;

	// 	while (currentValue < _value) {
	// 		currentValue += 0.01f;

	// 		SetFingerTargets (gripFingers, currentValue);
	// 		SmoothFinger (gripFingers);
	// 		AnimateFinger (gripFingers);
	// 		yield return new WaitForEndOfFrame ();
	// 	}

	// 	Debug.Log ("Ended animation");
	// }
#endregion // -------------------------------------------------------------------------------------------------------------------------------

}