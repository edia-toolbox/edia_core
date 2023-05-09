using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace eDIA {

	/// <summary> 
	/// This singleton script travels along the scenes that are loaded as it is DontDestroyOnLoad. <br/>
	/// Responsible for loading/unloading, user related actions, top level application.<br/>
	/// Has references to the XR rig camera and hands for the rest of the application.<br/>
	/// </summary>
	public class XRManager : Singleton<XRManager> {

		[Header("Debug")]
		public bool showLog = false;
		public Color logColor = Color.cyan;
		[Space(10f)]
		[Header ("References")]
		public Transform XRCam;
		public Transform XRLeft;
		public Transform XRRight;
		public Transform mainMenuHolder;
		public Transform camOverlay;

		void Awake () {
			CheckReferences();
		}

		private void Start() {
			EnableXRInteraction(false); // Start the system with interaction rays disabled
		}

		void CheckReferences () {
			if (XRCam == null) 	Debug.LogError("XR Camera reference not set");
			if (XRLeft == null) 	Debug.LogError("XR LeftController reference not set");
			if (XRRight == null) 	Debug.LogError("XR RightController reference not set");
			if (camOverlay == null) Debug.LogError("camOverlay reference not set");
		}

		private void OnDrawGizmos() {
			Gizmos.color = Color.cyan;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.5f,0.0f,0.5f));
			Gizmos.DrawLine(Vector3.zero, Vector3.forward);
		}

#region Inspector debug calls

		[ContextMenu("TurnOnRayInteractor")]
		public void TurnOnRayInteractor () {
			EnableXRInteraction(true);
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region XR Helper methods

		/// <summary>The pivot of the player will be set on the location of this Injector</summary>
		public void MovePlayarea(Transform newTransform) {
			transform.position = newTransform.position;
			transform.rotation = newTransform.rotation;
		}

		/// <summary>Turn XR hand / controller interaction possibility on or off.</summary>
		/// <param name="onOff">Boolean</param>
		public void EnableXRInteraction (bool onOff) {
			XRLeft.GetComponent<XRController>().EnableInteraction(onOff);
			XRRight.GetComponent<XRController>().EnableInteraction(onOff);
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HANDS

		/// <summary>Set the hand pose for the current interactive hand(s). Pose as string 'point','fist','idle'</summary>
		/// <param name="pose"></param>
		public  void SetHandPose (string pose) {
			EventManager.TriggerEvent (eDIA.Events.XR.EvHandPose, new eParam ( pose ));
		}

		/// <summary>Enable custom fixed handposes, expects boolean</summary>
		public  void EnableCustomHandPoses (bool onOff) {
			EventManager.TriggerEvent (eDIA.Events.XR.EvEnableCustomHandPoses, new eParam ( onOff ));
		}

		/// <summary>Shows the hands that are set to be allowed visible on/off</summary>
		public  void ShowHands (bool onOff) {
			XRLeft.GetComponent<XRController>().Show(onOff);
			XRRight.GetComponent<XRController>().Show(onOff);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
	#region MISC	

		public void AddToLog(string _msg) {
			if (showLog)
				LogUtilities.AddToLog(_msg, "eDIA", logColor);
		}
		
#endregion	// -------------------------------------------------------------------------------------------------------------------------------

	}
}