using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace eDIA {
	
	public class XRrigController : MonoBehaviour	{

		[Header("Settings")]
		public eDIA.Constants.Interactor interactorType = eDIA.Constants.Interactor.LEFT;

		public bool isAllowedToBeVisable = false;
		public bool isAllowedToInteract = false;
		public bool isVisible = false;
		public bool isInteractive = false;

		SkinnedMeshRenderer handSMR = null;
		XRInteractorLineVisual lineVisual = null;

#region SETTING UP

		void Awake() {
			handSMR = GetComponentInChildren<SkinnedMeshRenderer>(true);
			lineVisual = GetComponent<XRInteractorLineVisual>();

			AllowVisible(isVisible);
			AllowInteractive(isAllowedToInteract);
			
			EventManager.StartListening(eDIA.Events.XR.EvUpdateVisableInteractor, OnEvUpdateVisableInteractor);
			EventManager.StartListening(eDIA.Events.XR.EvUpdateInteractiveInteractor, OnEvUpdateInteractiveInteractor);
			EventManager.StartListening(eDIA.Events.XR.EvEnableXRInteraction, OnEvEnableXRInteraction);
			EventManager.StartListening(eDIA.Events.XR.EvShowXRController, OnEvShowXRController);
		}

		void OnDestroy() {
			EventManager.StopListening(eDIA.Events.XR.EvUpdateVisableInteractor, OnEvUpdateVisableInteractor);
			EventManager.StopListening(eDIA.Events.XR.EvUpdateInteractiveInteractor, OnEvUpdateInteractiveInteractor);
			EventManager.StopListening(eDIA.Events.XR.EvEnableXRInteraction, OnEvEnableXRInteraction);
			EventManager.StopListening(eDIA.Events.XR.EvShowXRController, OnEvShowXRController);
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EVENT LISTENERS

		/// <summary>Change the controller / interactor that is visible</summary>
		/// <param name="obj">Interactor enum index</param>
		private void OnEvUpdateVisableInteractor(eParam obj)
		{
			eDIA.Constants.Interactor receivedInteractor = (eDIA.Constants.Interactor)obj.GetInt();
			
			if ((receivedInteractor == eDIA.Constants.Interactor.BOTH) || (receivedInteractor == interactorType)) {
				isAllowedToBeVisable = true;
				Show(true);
			} else {
				isAllowedToBeVisable = false;
				isVisible = false;
				handSMR.enabled = false;	
			}
		}

		/// <summary>Change the controller / interactor that is the main interactor</summary>
		/// <param name="obj">Interactor enum index</param>
		private void OnEvUpdateInteractiveInteractor(eParam obj)
		{
			eDIA.Constants.Interactor receivedInteractor = (eDIA.Constants.Interactor)obj.GetInt();

			if ((receivedInteractor == eDIA.Constants.Interactor.BOTH) || (receivedInteractor == interactorType)) {
				isAllowedToInteract = true;
			} else { 
				isAllowedToInteract = false;
				isInteractive = false;
				lineVisual.enabled = false;
			}
		}

		/// <summary>Change the controller / interactor that is visible</summary>
		/// <param name="obj">Interactor enum index</param>
		private void OnEvEnableXRInteraction(eParam obj)
		{
			EnableInteraction(obj.GetBool());
		}

		/// <summary>Change the controller / interactor that is visible</summary>
		/// <param name="obj">Interactor enum index</param>
		private void OnEvShowXRController(eParam obj)
		{
			Show(obj.GetBool());
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region MAIN METHODS

		/// <summary>Show the actual controller of hand visually</summary>
		/// <param name="_onOff">True/false</param>
		void AllowVisible (bool _onOff) {
			isAllowedToBeVisable = _onOff;
		}

		/// <summary>Allow this controller to be interacting with the environment</summary>
		/// <param name="_onOff">True/false</param>
		void AllowInteractive (bool _onOff) {
			isAllowedToInteract = _onOff;
		}

		/// <summary>Enable/Disable interaction</summary>
		/// <param name="_onOff">True/false</param>
		void EnableInteraction (bool _onOff) {

			if (!isAllowedToInteract)
				return;

			isInteractive = _onOff;
			lineVisual.enabled = _onOff;
		}

		/// <summary>Show/Hide controller</summary>
		/// <param name="_onOff">True/false</param>
		public void Show (bool _onOff) {

			if (!isAllowedToBeVisable)
				return;
 
			isVisible = _onOff;
			handSMR.enabled = _onOff;		
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
    	}
}