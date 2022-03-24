using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace eDIA {

	/// <summary> 
	/// The SystemManager is a Singleton and therefor reachable from whatever script.<br/>
	/// The script travels along the scenes that are loaded as it is DontDestroyOnLoad. <br/>
	/// Responsible for loading/unloading, user related actions, top level application stuff.<br/>
	/// Has references to the XR rig camera and hands for the rest of the application.<br/>
	/// </summary>
	public class XRrigManager : MonoBehaviour {

		[Header("Debug")]
		public bool showLog = false;
		public Color taskColor = Color.cyan;
		[Space(10f)]
		[Header ("References")]
		public Transform XRrig_MainCamera;
		public Transform XRrig_LeftController;
		public Transform XRrig_RightController;
		public Transform mainMenuHolder;

		/// <summary>Main system manager, provides refs to XR rig components</summary>
		public static XRrigManager instance = null;

		void Awake () {

			// Make a singleton for this so it's reachable 
			if (instance == null) {
				instance = this;
				DontDestroyOnLoad (transform.gameObject);
			
				// Check if references are filled in the inspector
				CheckReferences ();
				
				// Init the settings, either defaults or stored on disk
				SystemSettings.InitSystemSettings();
			}
			else if (instance != this) Destroy (this.gameObject);
		}
		
		void CheckReferences () {
			if (XRrig_MainCamera == null) 	Debug.LogError("XRrig_MainCamera reference not set");
			if (XRrig_LeftController == null) 	Debug.LogError("XRrig_LeftController reference not set");
			if (XRrig_RightController == null) 	Debug.LogError("XRrig_RightController reference not set");
		}
		
		/// <summary>The pivot of the playare will be set on the location of this Injector</summary>
		public void MovePlayarea(Transform newTransform) {
			transform.position = newTransform.position;
			transform.rotation = newTransform.rotation;
		}


	#region MISC	

		public void AddToLog(string _msg) {
			if (showLog)
				LogUtilities.AddToLog(_msg, "eDIA", taskColor);
		}
		
	#endregion	// -------------------------------------------------------------------------------------------------------------------------------
	}
}