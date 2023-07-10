using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace eDIA {
	
	public class XRController : MonoBehaviour	{

		[Header("Settings")]
		public eDIA.Constants.Interactor interactorType = eDIA.Constants.Interactor.LEFT;

		public bool isAllowedToBeVisable = false;
		public bool isAllowedToInteract = false;
		public bool isVisible = false;
		public bool isInteractive = false;

		SkinnedMeshRenderer handSMR = null;
		public Transform rayInteractor = null;

#region SETTING UP

		void Awake() {
			handSMR = GetComponentInChildren<SkinnedMeshRenderer>(true);

			AllowVisible(isVisible);
			AllowInteractive(isAllowedToInteract);
			
			EventManager.StartListening(eDIA.Events.XR.EvUpdateVisableInteractor, OnEvUpdateVisableInteractor);
			EventManager.StartListening(eDIA.Events.XR.EvUpdateInteractiveInteractor, OnEvUpdateInteractiveInteractor);
			EventManager.StartListening(eDIA.Events.XR.EvEnableXRInteraction, OnEvEnableXRInteraction);
			EventManager.StartListening(eDIA.Events.XR.EvShowXRController, OnEvShowXRController);
			EventManager.StartListening(eDIA.Events.XR.EvEnableRayForCamOverlayer, OnEvEnableRayForCamOverlayer);

		}

		void OnDestroy() {
			EventManager.StopListening(eDIA.Events.XR.EvUpdateVisableInteractor, OnEvUpdateVisableInteractor);
			EventManager.StopListening(eDIA.Events.XR.EvUpdateInteractiveInteractor, OnEvUpdateInteractiveInteractor);
			EventManager.StopListening(eDIA.Events.XR.EvEnableXRInteraction, OnEvEnableXRInteraction);
			EventManager.StopListening(eDIA.Events.XR.EvShowXRController, OnEvShowXRController);
			EventManager.StopListening(eDIA.Events.XR.EvEnableRayForCamOverlayer, OnEvEnableRayForCamOverlayer);
		}

		private void OnDrawGizmos() {
			Gizmos.color = Color.cyan;
			// GizmoHelpers.DrawWireCubeOriented(transform.position,transform.localRotation,0.1f);
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.1f,0.02f,0.15f));
			Gizmos.DrawWireCube(Vector3.zero - (interactorType == eDIA.Constants.Interactor.LEFT ? new Vector3(-0.06f,0.01f,0.05f) : new Vector3(0.06f,0.01f,0.05f)), new Vector3(0.03f,0.02f,0.05f));
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region EVENT LISTENERS

		/// <summary>Enable interaction with UI presented on layer 'camoverlay'</summary>
		private void OnEvEnableRayForCamOverlayer(eParam obj)
		{
			//rayInteractor.GetComponent<XRRayInteractor>().raycastMask = 
		}

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
				rayInteractor.gameObject.SetActive(false);
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
		/// <param name="onOff">True/false</param>
		void AllowVisible (bool onOff) {
			isAllowedToBeVisable = onOff;
		}

		/// <summary>Allow this controller to be interacting with the environment</summary>
		/// <param name="onOff">True/false</param>
		void AllowInteractive (bool onOff) {
			isAllowedToInteract = onOff;
		}

		/// <summary>Enable/Disable interaction</summary>
		/// <param name="onOff">True/false</param>
		public void EnableInteraction (bool onOff) {

			if (!isAllowedToInteract)
				return;

			rayInteractor.gameObject.SetActive(onOff);
			isInteractive = onOff;
		}

		/// <summary>Show/Hide controller</summary>
		/// <param name="onOff">True/false</param>
		public void Show (bool onOff) {

			if (!isAllowedToBeVisable)
				return;
 
			isVisible = onOff;
			handSMR.enabled = onOff;		
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
    	}
}