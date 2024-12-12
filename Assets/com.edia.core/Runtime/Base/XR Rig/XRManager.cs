using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Utils;

namespace Edia {

	public class XRManager : Singleton<XRManager> {

		[Header("Debug")]
		public bool showLog = false;
		Color logColor = Color.cyan;
		[Space(10f)]
		[Header("References")]
		public Transform XRCam;
		public Transform XRLeft;
		public Transform XRRight;
		public Transform CamOverlay;

		void Awake() {
			CheckReferences();
		}

		private void Start() {
			EnableXRRayInteraction(false); // Start the system with interaction rays disabled
		}

		void CheckReferences() {
			if (XRCam == null) Debug.LogError("XR Camera reference not set");
			if (XRLeft == null) Debug.LogError("XR LeftController reference not set");
			if (XRRight == null) Debug.LogError("XR RightController reference not set");
			if (CamOverlay == null) Debug.LogError("camOverlay reference not set");
		}

		private void OnDrawGizmos() {
			Gizmos.color = Color.cyan;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.5f, 0.0f, 0.5f));
			Gizmos.DrawLine(Vector3.zero, Vector3.forward);
		}

		#region Inspector debug calls

		[ContextMenu("TurnOnRayInteractor")]
		public void TurnOnRayInteractor() {
			EnableXRRayInteraction(true);
		}

		[ContextMenu("ShowHands")]
		public void ShowHands() {
			ShowHands(true);
		}

		[ContextMenu("ShowControllers")]
		public void ShowControllers() {
			ShowControllers(true);
		}


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region XR Helper methods

		/// <summary>The pivot of the player will be set on the location of this Injector</summary>
		public void MovePlayarea(Transform newTransform) {
			transform.position = newTransform.position;
			transform.rotation = newTransform.rotation;
		}


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region INTERACTION

		/// <summary>Turn XR hand / controller interaction possibility on or off.</summary>
		/// <param name="onOff">Boolean</param>
		public void EnableXRRayInteraction(bool onOff) {
			this.Add2Console("EnableXRInteraction " + onOff);
			XRLeft.GetComponent<XRController>().EnableRayInteraction(onOff);
			XRRight.GetComponent<XRController>().EnableRayInteraction(onOff);
		}

		public void EnableXROverlayRayInteraction(bool onOff) {
			XRLeft.GetComponent<XRController>().EnableXROverlayRayInteraction(onOff);
			XRRight.GetComponent<XRController>().EnableXROverlayRayInteraction(onOff);
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region SIGHT

		/// <summary>Fades VR user view to black</summary>
		public void HideVR () {
			Fade(true, -1f);
		}

		/// <summary>Fades VR user view to black</summary>
		/// <param name="fadeSpeed">Speed to fade, default: 1</param>
		public void HideVR(float fadeSpeed) {
			Fade(true, fadeSpeed);
		}

		void Fade(bool _onOff, float _fadeSpeed) {
			if (_onOff) XRCam.GetComponent<ScreenFader>().StartFadeBlackIn(_fadeSpeed);
			else XRCam.GetComponent<ScreenFader>().StartFadeBlackOut(_fadeSpeed);
		}

		/// <summary>Fades VR user view from black</summary>
		public void ShowVR () {
			Fade(false, -1f);
		}

		/// <summary>Fades VR user view from black</summary>
		/// <param name="fadeSpeed">Speed to fade, default: 1</param>
		public void ShowVR(float fadeSpeed) {
			Fade(false, fadeSpeed);
		}

		/// <summary>Instantly shows VR user view</summary>
		public void ShowVRInstantly () {
			XRCam.GetComponent<ScreenFader>().HideBlockingImage();
		}


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region HANDS

		/// <summary>Set the hand pose for the current interactive hand(s). Pose as string 'point','fist','idle'</summary>
		/// <param name="pose"></param>
		public void SetHandPose(string pose) {
			EventManager.TriggerEvent(Edia.Events.XR.EvHandPose, new eParam(pose));
		}

		/// <summary>Enable custom fixed handposes, expects boolean</summary>
		public void EnableCustomHandPoses(bool onOff) {
			EventManager.TriggerEvent(Edia.Events.XR.EvEnableCustomHandPoses, new eParam(onOff));
		}

		/// <summary>Shows the hands that are set to be allowed visible on/off</summary>
		public void ShowHands(bool onOff) {
			XRLeft.GetComponent<XRController>().ShowHandModel(onOff);
			XRRight.GetComponent<XRController>().ShowHandModel(onOff);
		}

		public void ShowControllers(bool onOff) {
			XRLeft.GetComponent<XRController>().ShowControllerModel(onOff);
			XRRight.GetComponent<XRController>().ShowControllerModel(onOff);
		}


		#endregion // -------------------------------------------------------------------------------------------------------------------------------

	}
}