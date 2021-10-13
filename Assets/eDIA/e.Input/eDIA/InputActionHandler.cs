using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using eDIA;
using System;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

[RequireComponent (typeof(InputActionManager))]
public class InputActionHandler : MonoBehaviour {

#region Variables

	string ActionMapName = "eDIA"; // Default eDIA XR mapping config
	InputActionMap actionmap = null;

	InputAction actionMenu 		= null;
	InputAction actionProceed 	= null;

	// TODO: Do we gather experiment level actions in this main InputActionHandler?
	InputAction actionStartExperiment 	= null;
	//! Add new InputActions here

	void Awake() {
		SetupActionEvents ();
	}

	void OnDestroy() {
		if (actionMenu != null) 	actionMenu.performed -= menuPerformed;
		if (actionProceed != null) 	actionProceed.performed -= proceedPerformed;
		if (actionStartExperiment != null) 	actionStartExperiment.performed -= actionStartExperimentPerformed;
		//! Add new InputActions listeners here
	}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region Setup ActionAsset, ActionMap and Action Listeners

	bool MapActions () {
		try {
			actionMenu 			= actionmap.FindAction("Menu", true);
			actionProceed 		= actionmap.FindAction("Proceed", true);
			actionStartExperiment 	= actionmap.FindAction("StartExperiment", true);
			//! Add new InputActions here
		}
		catch (Exception e) {
			print(e.Message);
			return false;
		}  

		actionMenu.performed 	+= menuPerformed;
		actionProceed.performed += proceedPerformed;
		actionStartExperiment.performed += actionStartExperimentPerformed;
		//! Add new InputActions listeners here

		return true;
	}

	bool SetupActionEvents () {
		try {
			actionmap = GetComponent<InputActionManager>().actionAssets[0].FindActionMap(ActionMapName,true);
		}
		catch (Exception e) {
			print("Missing/Incorrect action asset link in the InputActionmanager!! " + e.Message);
			return false;
		}  

		MapActions();

		return true;
	}

	// To change the map on runtime
	public void SetActionMapName (string mapName) {
		if(mapName != null && mapName != string.Empty) ActionMapName = mapName;
	}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region Input action listeners conversion to EventManager triggers

	public void menuPerformed (InputAction.CallbackContext context) {
		EventManager.TriggerEvent("EvMenuPerformed", null); // Convert it into our eventmanager system
	}

	public void proceedPerformed (InputAction.CallbackContext context) {
		EventManager.TriggerEvent("EvProceed", null); // Convert it into our eventmanager system
	}

	public void actionStartExperimentPerformed (InputAction.CallbackContext context) {
		EventManager.TriggerEvent("EvStartExperiment", null); // Convert it into our eventmanager system
	}


#endregion // -------------------------------------------------------------------------------------------------------------------------------

}
