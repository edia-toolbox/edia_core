using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace eDIA {

	/// <summary> 
	/// The XRrigManager is a Singleton and therefor reachable from whatever script.<br/>
	/// The script travels along the scenes that are loaded as it is DontDestroyOnLoad. <br/>
	/// Responsible for loading/unloading, user related actions, top level application.<br/>
	/// Has references to the XR rig camera and hands for the rest of the application.<br/>
	/// </summary>
	public class XRManager : Singleton<XRManager> {

		[Header("Debug")]
		public bool showLog = false;
		public Color taskColor = Color.cyan;
		[Space(10f)]
		[Header ("References")]
		public Transform XRCam;
		public Transform XRLeft;
		public Transform XRRight;
		public Transform mainMenuHolder;
		public Transform messagePanelInVR;


		/// <summary>Main system manager, provides refs to XR rig components</summary>
		public static XRManager instance = null;


		void Awake () {

			// // Make a singleton for this so it's reachable 
			// if (instance == null) {
			// 	instance = this;
			// 	DontDestroyOnLoad (transform.gameObject);
			
			// 	// Check if references are filled in the inspector
			// 	CheckReferences ();
				
			// 	// Init the settings, either defaults or stored on disk
			// 	SystemSettings.InitSystemSettings();
			// }
			// else if (instance != this) Destroy (this.gameObject);

			CheckReferences();

		}

		void CheckReferences () {
			if (XRCam == null) 	Debug.LogError("XR Camera reference not set");
			if (XRLeft == null) 	Debug.LogError("XR LeftController reference not set");
			if (XRRight == null) 	Debug.LogError("XR RightController reference not set");
		}
		
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
				LogUtilities.AddToLog(_msg, "eDIA", taskColor);
		}
		
#endregion	// -------------------------------------------------------------------------------------------------------------------------------

	}
}