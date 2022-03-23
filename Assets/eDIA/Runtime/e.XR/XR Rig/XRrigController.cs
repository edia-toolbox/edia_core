using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace eDIA {
	
	public class XRrigController : MonoBehaviour	{

		[Header("Settings")]
		public eDIA.Constants.Interactor interactorType = eDIA.Constants.Interactor.LEFT;

		public bool isVisible = false;
		public bool isInteractive = false;
		public bool isAllowedToInteract = false;

		SkinnedMeshRenderer handSMR = null;
		XRInteractorLineVisual lineVisual = null;

#region SETTING UP

		void Awake() {
			handSMR = GetComponentInChildren<SkinnedMeshRenderer>(true);
			lineVisual = GetComponent<XRInteractorLineVisual>();

			MakeVisible(isVisible);
			AllowInteractive(isInteractive);

			EventManager.StartListening(eDIA.Events.Interaction.EvUpdateVisableInteractor, OnEvUpdateVisableInteractor);
			EventManager.StartListening(eDIA.Events.Interaction.EvUpdateInteractiveInteractor, OnEvUpdateInteractiveInteractor);
			EventManager.StartListening(eDIA.Events.Interaction.EvEnableXRInteraction, OnEvEnableXRInteraction);
		}


		void OnDestroy() {
			EventManager.StopListening(eDIA.Events.Interaction.EvUpdateVisableInteractor, OnEvUpdateVisableInteractor);
			EventManager.StopListening(eDIA.Events.Interaction.EvUpdateInteractiveInteractor, OnEvUpdateInteractiveInteractor);
			EventManager.StopListening(eDIA.Events.Interaction.EvEnableXRInteraction, OnEvEnableXRInteraction);
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EVENT LISTENERS

		/// <summary>Change the controller / interactor that is visible</summary>
		/// <param name="obj">Interactor enum index</param>
		private void OnEvUpdateVisableInteractor(eParam obj)
		{
			eDIA.Constants.Interactor receivedInteractor = (eDIA.Constants.Interactor)obj.GetInt();
			
			if ((receivedInteractor == eDIA.Constants.Interactor.BOTH) || (receivedInteractor == interactorType)) {

				MakeVisible(true);
			} else MakeVisible(false);
		}

		/// <summary>Change the controller / interactor that is the main interactor</summary>
		/// <param name="obj">Interactor enum index</param>
		private void OnEvUpdateInteractiveInteractor(eParam obj)
		{
			eDIA.Constants.Interactor receivedInteractor = (eDIA.Constants.Interactor)obj.GetInt();

			if ((receivedInteractor == eDIA.Constants.Interactor.BOTH) || (receivedInteractor == interactorType)) {
				AllowInteractive(true);
			} else AllowInteractive(false);
		}

		/// <summary>Change the controller / interactor that is visible</summary>
		/// <param name="obj">Interactor enum index</param>
		private void OnEvEnableXRInteraction(eParam obj)
		{
			EnableInteraction(obj.GetBool());
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region MAIN METHODS

		public void MakeVisible (bool _onOff) {
			handSMR.enabled = _onOff;		
			isVisible = _onOff;
		}

		public void AllowInteractive (bool _onOff) {
			isAllowedToInteract = _onOff;
			isInteractive = _onOff;
			lineVisual.enabled = _onOff;
		}

		public void EnableInteraction (bool _onOff) {
			isInteractive = _onOff;
			lineVisual.enabled = _onOff;
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
    	}
}