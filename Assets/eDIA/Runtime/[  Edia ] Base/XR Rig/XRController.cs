using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Edia {
	
	public class XRController : MonoBehaviour	{

		[Header("Settings")]
		public Edia.Constants.Interactor interactorType = Edia.Constants.Interactor.LEFT;

		public bool isAllowedToBeVisable = false;
		public bool isAllowedToInteract = false;
		public bool isVisible = false;
		public bool isInteractive = false;

		[Header("Models")]
		public GameObject HandModel = null;
		public GameObject ControllerModel = null;

		[Header("Interactives")]
		[Tooltip("Ray interactor to interact with UI & Default ")]
		public Transform rayInteractor = null;
		[Tooltip("Ray interactor to interact messagepanel OVERLAY UI")]
		public Transform XROverlayRayInteractor = null;

		#region SETTING UP

		void Awake() {

			AllowVisible(isVisible);
			AllowInteractive(isAllowedToInteract);
			
			EventManager.StartListening(Edia.Events.XR.EvUpdateVisableInteractor, OnEvUpdateVisableInteractor);
			EventManager.StartListening(Edia.Events.XR.EvUpdateInteractiveInteractor, OnEvUpdateInteractiveInteractor);
			EventManager.StartListening(Edia.Events.XR.EvEnableXRInteraction, OnEvEnableXRRayInteraction);
			EventManager.StartListening(Edia.Events.XR.EvShowXRController, OnEvShowXRController);
			EventManager.StartListening(Edia.Events.XR.EvEnableXROverlay, OnEvEnableXROverlay);

		}

		void OnDestroy() {
			EventManager.StopListening(Edia.Events.XR.EvUpdateVisableInteractor, OnEvUpdateVisableInteractor);
			EventManager.StopListening(Edia.Events.XR.EvUpdateInteractiveInteractor, OnEvUpdateInteractiveInteractor);
			EventManager.StopListening(Edia.Events.XR.EvEnableXRInteraction, OnEvEnableXRRayInteraction);
			EventManager.StopListening(Edia.Events.XR.EvShowXRController, OnEvShowXRController);
			EventManager.StopListening(Edia.Events.XR.EvEnableXROverlay, OnEvEnableXROverlay);
		}

		private void OnDrawGizmos() {
			Gizmos.color = Color.cyan;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.1f,0.02f,0.15f));
			Gizmos.DrawWireCube(Vector3.zero - (interactorType == Edia.Constants.Interactor.LEFT ? new Vector3(-0.06f,0.01f,0.05f) : new Vector3(0.06f,0.01f,0.05f)), new Vector3(0.03f,0.02f,0.05f));
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region EVENT LISTENERS

		/// <summary>Enable interaction with UI presented on layer 'camoverlay'</summary>
		private void OnEvEnableXROverlay(eParam obj)
		{
			//Debug.Log($"{this.gameObject.name} OnEvEnableXROverlay {obj.GetBool()}");
			HandModel.layer = LayerMask.NameToLayer(obj.GetBool() ? "CamOverlay" : "Default");
		}

		/// <summary>Change the controller / interactor that is visible</summary>
		/// <param name="obj">Interactor enum index</param>
		private void OnEvUpdateVisableInteractor(eParam obj)
		{
			Edia.Constants.Interactor receivedInteractor = (Edia.Constants.Interactor)obj.GetInt();
			
			if ((receivedInteractor == Edia.Constants.Interactor.BOTH) || (receivedInteractor == interactorType)) {
				isAllowedToBeVisable = true;
				ShowHandModel(true);
			} else {
				isAllowedToBeVisable = false;
				isVisible = false;
			}
		}

		/// <summary>Change the controller / interactor that is the main interactor</summary>
		/// <param name="obj">Interactor enum index</param>
		private void OnEvUpdateInteractiveInteractor(eParam obj)
		{
			Edia.Constants.Interactor receivedInteractor = (Edia.Constants.Interactor)obj.GetInt();

			if ((receivedInteractor == Edia.Constants.Interactor.BOTH) || (receivedInteractor == interactorType)) {
				isAllowedToInteract = true;
			} else { 
				isAllowedToInteract = false;
				isInteractive = false;
				rayInteractor.gameObject.SetActive(false);
			}
		}

		/// <summary>Change the controller / interactor that is visible</summary>
		/// <param name="obj">Interactor enum index</param>
		private void OnEvEnableXRRayInteraction(eParam obj)
		{
			EnableRayInteraction(obj.GetBool());
		}

		/// <summary>Change the controller / interactor that is visible</summary>
		/// <param name="obj">Interactor enum index</param>
		private void OnEvShowXRController(eParam obj)
		{
			ShowHandModel(obj.GetBool());
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
		public void EnableRayInteraction (bool onOff) {

			if (!isAllowedToInteract)
				return;

			rayInteractor.gameObject.SetActive(onOff);
			isInteractive = onOff;
		}


		/// <summary>Enable/Disable interaction</summary>
		/// <param name="onOff">True/false</param>
		public void EnableXROverlayRayInteraction(bool onOff) {

			if (!isAllowedToInteract)
				return;

			XROverlayRayInteractor.gameObject.SetActive(onOff);
		}

		/// <summary>Show/Hide hand</summary>
		/// <param name="onOff">True/false</param>
		public void ShowHandModel (bool onOff) {

			if (!isAllowedToBeVisable)
				return;
 
			isVisible = onOff;
			HandModel.SetActive(onOff);
		}

		/// <summary>Show/Hide controller</summary>
		/// <param name="onOff">True/false</param>
		public void ShowControllerModel(bool onOff) {

			if (!isAllowedToBeVisable)
				return;

			isVisible = onOff;
			ControllerModel.SetActive(onOff);
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}
}