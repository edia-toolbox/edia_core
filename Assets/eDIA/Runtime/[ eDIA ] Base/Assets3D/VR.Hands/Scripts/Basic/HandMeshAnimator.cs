using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using eDIA;
using System;

public class HandMeshAnimator : MonoBehaviour {

#region VARIABLES

	[Header("Refs")]
	public InputActionReference gripReference = null;
	public InputActionReference pointReference = null;

	[Header("Settings")]
	public float smoothingSpeed = 5f;

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
		EventManager.StartListening(eDIA.Events.XR.EvHandPose, OnEvHandPose);
		EventManager.StartListening(eDIA.Events.XR.EvEnableCustomHandPoses, EvEnableCustomHandPoses);
	}

	private void OnDestroy() {
		EventManager.StopListening(eDIA.Events.XR.EvHandPose, OnEvHandPose);
		EventManager.StopListening(eDIA.Events.XR.EvEnableCustomHandPoses, EvEnableCustomHandPoses);
	}



	#endregion // -------------------------------------------------------------------------------------------------------------------------------
	#region EVENTHANDLERS

	private void EvEnableCustomHandPoses(eParam obj) {

		if (obj.GetBool()) {
	    		gripReference.action.performed += GripPerformed;
			pointReference.action.performed += PointPerformed;
		} else {
			gripReference.action.performed -= GripPerformed;
			pointReference.action.performed -= PointPerformed;
		}
	}


	private void OnEvHandPose (eParam obj)
	{
		string pose = obj.GetString();

		switch (pose) {
			case "idle":
				StartCoroutine(GotoIntoIdleMode());		
			break;
			case "point":
				StartCoroutine(GoIntoPose(gripFingers));		
			break;
			case "fist":
				StartCoroutine(GoIntoPose(gripFingers));		
				StartCoroutine(GoIntoPose(pointFingers));
			break;
		}
	}

	IEnumerator GoIntoPose (List<Finger> fingers) {
		float val = 0f;

		while (val < 1f) {
			SetFingerTargets (fingers, val);
			val += 0.1f;
			yield return new WaitForEndOfFrame();
		}

		SetFingerTargets (gripFingers, 1f);
	}

	IEnumerator GotoIntoIdleMode () {
		float val = 1f;

		while (val > 0f) {
			SetFingerTargets (gripFingers, val);
			val -= 0.1f;
			yield return new WaitForEndOfFrame();
		}

		SetFingerTargets (gripFingers, 0f);
	}

	void GripPerformed (InputAction.CallbackContext context) {
		SetFingerTargets (gripFingers, context.ReadValue<float>());
	}

	void PointPerformed (InputAction.CallbackContext context) {
		SetFingerTargets (pointFingers, context.ReadValue<float>());
	}

	private void Update () {
		// Smooth input values
		SmoothFinger (gripFingers);
		SmoothFinger(pointFingers);

		// Apply smooth values
		AnimateFinger(pointFingers);
		AnimateFinger (gripFingers);
	}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HELPERS

	private void SetFingerTargets (List<Finger> fingers, float value) {
		foreach (Finger finger in fingers)
			finger.target = value;
	}

	private void SmoothFinger (List<Finger> fingers) {
		foreach (Finger finger in fingers) {
			float time = smoothingSpeed * Time.unscaledDeltaTime;
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