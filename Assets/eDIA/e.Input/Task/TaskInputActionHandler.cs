using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using System;
using eDIA;

[RequireComponent (typeof(InputActionManager))]
public class TaskInputActionHandler : MonoBehaviour
{

#region Variables

	string taskActionMapName            = "TASK"; // Task specific mapping
	InputActionMap taskActionMap        = null;

	InputAction emo1 = null;
	InputAction emo2 = null;
	InputAction emo3 = null;
	InputAction emo4 = null;

	void Awake() {
		SetupTaskActionEvents ();
	}

	void OnDestroy() {
		if (emo1 != null) emo1.performed -= emo1performed;
		if (emo2 != null) emo2.performed -= emo1performed;
		if (emo3 != null) emo3.performed -= emo1performed;
		if (emo4 != null) emo4.performed -= emo1performed;
		//! Add new InputActions listeners here
	}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region Setup ActionAsset, ActionMap and Action Listeners

	bool MapTaskActions () {
		try {
			emo1 = taskActionMap.FindAction("SelectEmotion1", true);
			emo2 = taskActionMap.FindAction("SelectEmotion2", true);
			emo3 = taskActionMap.FindAction("SelectEmotion3", true);
			emo4 = taskActionMap.FindAction("SelectEmotion4", true);
			//! Add new InputActions here
		}
		catch (Exception e) {
			print(e.Message);
			return false;
		}

		emo1.performed += emo1performed;
		emo2.performed += emo2performed;
		emo3.performed += emo3performed;
		emo4.performed += emo4performed;
		//! Add new InputActions listeners here

		return true;
	}

	bool SetupTaskActionEvents () {
		try {
			taskActionMap = GetComponent<InputActionManager>().actionAssets[0].FindActionMap(taskActionMapName,true);
		}
		catch (Exception e) {
			print("Missing/Incorrect task action asset link in the InputActionmanager!! " + e.Message);
			return false;
		}  

		MapTaskActions();

		return true;
	}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region Input action listeners conversion to EventManager triggers

	public void emo1performed (InputAction.CallbackContext context) {
		EventManager.TriggerEvent("EvEmotionSelected", new eParam(1)); // Convert it into our eventmanager system
	}

	public void emo2performed (InputAction.CallbackContext context) {
		EventManager.TriggerEvent("EvEmotionSelected", new eParam(2)); // Convert it into our eventmanager system
	}

	public void emo3performed (InputAction.CallbackContext context) {
		EventManager.TriggerEvent("EvEmotionSelected", new eParam(3)); // Convert it into our eventmanager system
	}

	public void emo4performed (InputAction.CallbackContext context) {
		EventManager.TriggerEvent("EvEmotionSelected", new eParam(4)); // Convert it into our eventmanager system
	}

#endregion // -------------------------------------------------------------------------------------------------------------------------------

}
