using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Edia {
	
	public class XRController : MonoBehaviour	{

		[Header("Settings")]
		public Edia.Constants.Manipulator InteractionSide = Edia.Constants.Manipulator.LEFT;

		public bool isAllowedToBeVisible = false;
		public bool isAllowedToInteract = false;
		public bool isVisible = false;
		public bool isInteractive = false;
		bool isOverlayInteractive = false;

		[Header("Models")]
		public GameObject HandModel = null;
		private Transform[] HandModelChildren;
		public GameObject ControllerModel = null;
		private Transform[] ControllerModelChildren;
		
		[Tooltip("Ray interactor to interact with UI & Default ")]
		public Transform rayInteractor = null;
		[Tooltip("Ray interactor to interact messagepanel OVERLAY UI")]
		public Transform XROverlayRayInteractor = null;
		
		#region SETTING UP

		void Awake() {

			AllowVisible(isVisible);
			AllowInteractive(isAllowedToInteract);

			HandModelChildren = HandModel.GetComponentsInChildren<Transform>();
			ControllerModelChildren = ControllerModel.GetComponentsInChildren<Transform>();
			
			EventManager.StartListening(Edia.Events.XR.EvUpdateVisableSide, OnEvUpdateVisableSide);
			EventManager.StartListening(Edia.Events.XR.EvUpdateInteractiveSide, OnEvUpdateInteractiveSide);
			EventManager.StartListening(Edia.Events.XR.EvShowXRController, OnEvShowXRController);
			EventManager.StartListening(Edia.Events.XR.EvEnableXROverlay, OnEvEnableXROverlay);

		}

		void OnDestroy() {
			EventManager.StopListening(Edia.Events.XR.EvUpdateVisableSide, OnEvUpdateVisableSide);
			EventManager.StopListening(Edia.Events.XR.EvUpdateInteractiveSide, OnEvUpdateInteractiveSide);
			EventManager.StopListening(Edia.Events.XR.EvShowXRController, OnEvShowXRController);
			EventManager.StopListening(Edia.Events.XR.EvEnableXROverlay, OnEvEnableXROverlay);
		}

		private void OnDrawGizmos() {
			Gizmos.color = Color.cyan;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.1f,0.02f,0.15f));
			Gizmos.DrawWireCube(Vector3.zero - (InteractionSide == Edia.Constants.Manipulator.LEFT ? new Vector3(-0.06f,0.01f,0.05f) : new Vector3(0.06f,0.01f,0.05f)), new Vector3(0.03f,0.02f,0.05f));
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region EVENT LISTENERS

		/// <summary>Enable interaction with UI presented on layer 'camoverlay'</summary>
		private void OnEvEnableXROverlay(eParam obj)
		{
			foreach (Transform t in HandModelChildren) {
				t.gameObject.layer = LayerMask.NameToLayer(obj.GetBool() ? "CamOverlay" : "Default");
			}
			
			foreach (Transform t in ControllerModelChildren) {
				t.gameObject.layer = LayerMask.NameToLayer(obj.GetBool() ? "CamOverlay" : "Default");
			}
		}

		/// <summary>Change the controller / interactor that is visible</summary>
		/// <param name="obj">Interactor enum index</param>
		private void OnEvUpdateVisableSide(eParam obj)
		{
			Edia.Constants.Manipulator recievedSide = (Edia.Constants.Manipulator)obj.GetInt();
			
			if ((recievedSide == Edia.Constants.Manipulator.BOTH) || (recievedSide == InteractionSide)) {
				isAllowedToBeVisible = true;
				ShowHandModel(true);
			} else {
				isAllowedToBeVisible = false;
				isVisible = false;
				ShowHandModel(false);
			}
		}

		/// <summary>Change the controller / interactor that is the main interactor</summary>
		/// <param name="obj">Interactor enum index</param>
		private void OnEvUpdateInteractiveSide(eParam obj)
		{
			Edia.Constants.Manipulator receivedSide = (Edia.Constants.Manipulator)obj.GetInt();

			if ((receivedSide == Edia.Constants.Manipulator.BOTH) || (receivedSide == InteractionSide)) {
				isAllowedToInteract = true;
			} else {

				// check if this hand was interactive, so calling setting it active for the other hand
				if (isInteractive) {
					Invoke("EnableDelayedXRInteraction", 0.1f);
				}

				// check if this hand was overlay interactive, so calling setting it active for the other hand
				if (isOverlayInteractive) {
					Invoke("EnableDelayedXROverlayInteraction", 0.1f);
				}

				isAllowedToInteract = false;
				isInteractive = false;
				rayInteractor.gameObject.SetActive(false);

				isOverlayInteractive = false;
				XROverlayRayInteractor.gameObject.SetActive(false);
			}
		}

		// Send out XR interaction
		private void EnableDelayedXRInteraction () {
			XRManager.Instance.EnableXRRayInteraction(true);
		}

		// Send out XR OVERLAY interaction
		private void EnableDelayedXROverlayInteraction() {
			XRManager.Instance.EnableXROverlayRayInteraction(true);
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
			isAllowedToBeVisible = onOff;
		}

		/// <summary>Allow this controller to be interacting with the environment</summary>
		/// <param name="onOff">True/false</param>
		void AllowInteractive (bool onOff) {
			isAllowedToInteract = onOff;
		}

		/// <summary>Enable/Disable interaction</summary>
		/// <param name="onOff">True/false</param>
		public void EnableRayInteraction(bool onOff ) {

			if (!isAllowedToInteract)
				return;

			if (!isVisible)
				return;
			
			rayInteractor.gameObject.SetActive(onOff);
			isInteractive = onOff;
		}


		/// <summary>Enable/Disable interaction</summary>
		/// <param name="onOff">True/false</param>
		public void EnableXROverlayRayInteraction(bool onOff) {

			if (!isAllowedToInteract)
				return;

			if (!isVisible)
				return;
			
			XROverlayRayInteractor.gameObject.SetActive(onOff);
			isOverlayInteractive = onOff;
		}

		/// <summary>Show/Hide hand</summary>
		/// <param name="onOff">True/false</param>
		public void ShowHandModel (bool onOff) {

			isVisible = onOff && isAllowedToBeVisible;
			HandModel.SetActive(onOff && isAllowedToBeVisible);
		}

		/// <summary>Show/Hide controller</summary>
		/// <param name="onOff">True/false</param>
		public void ShowControllerModel(bool onOff) {

			isVisible = onOff && isAllowedToBeVisible;
			ControllerModel.SetActive(onOff && isAllowedToBeVisible);
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}
}