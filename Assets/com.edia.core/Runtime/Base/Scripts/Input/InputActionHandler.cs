using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace Edia {

	/// <summary>
	/// Mapping new Input system <c>InputAction</c> to EDIA <c>EventManager</c>
	/// </summary>
	[RequireComponent (typeof(InputActionManager))]
	public class InputActionHandler : MonoBehaviour {

	#region Variables

		string ActionMapName = "eDIA"; // Default eDIA XR mapping config
		InputActionMap actionmap = null;

		InputAction actionMenu 		= null;
		InputAction actionProceed 	= null;

		void Awake() {
			SetupActionEvents ();
		}

		void OnDestroy() {
			if (actionMenu != null) 	actionMenu.performed -= menuPerformed;
			if (actionProceed != null) 	actionProceed.performed -= proceedPerformed;
		}

	#endregion // -------------------------------------------------------------------------------------------------------------------------------
	#region Setup ActionAsset, ActionMap and Action Listeners

		bool MapActions () {
			try {
				actionMenu 			= actionmap.FindAction("Menu", true);
				actionProceed 		= actionmap.FindAction("Proceed", true);
			}
			catch (Exception e) {
				print(e.Message);
				return false;
			}  

			actionMenu.performed 	+= menuPerformed;
			actionProceed.performed += proceedPerformed;

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

	#endregion // -------------------------------------------------------------------------------------------------------------------------------
	#region Input action listeners conversion to EventManager triggers

		public void menuPerformed (InputAction.CallbackContext context) {
			Debug.Log("menuPerformed");
			EventManager.TriggerEvent(Edia.Events.System.EvCallMainMenu, null); // Convert it into our eventmanager system
		}

		public void proceedPerformed (InputAction.CallbackContext context) {
			Debug.Log("proceedPerformed");
			EventManager.TriggerEvent(Edia.Events.StateMachine.EvProceed); // Convert it into our eventmanager system
		}


	#endregion // -------------------------------------------------------------------------------------------------------------------------------

	}

}