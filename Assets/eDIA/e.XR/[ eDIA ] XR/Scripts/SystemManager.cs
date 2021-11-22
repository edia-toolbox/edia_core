using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using eDIA.EditorUtils;

namespace eDIA {

	/// <summary> 
	/// The SystemManager is a Singleton and therefor reachable from whatever script.<br/>
	/// The script travels along the scenes that are loaded as it is DontDestroyOnLoad. <br/>
	/// Responsible for loading/unloading, user related actions, top level application stuff.<br/>
	/// Has references to the XR rig camera and hands for the rest of the application.<br/>
	/// </summary>
	public class SystemManager : MonoBehaviour {

		[Header("Debug")]
		public bool showLog = false;
		public Color taskColor = Color.cyan;
		[Space(10f)]
		[Tooltip("ignore flag to use static camera position instead of XR rig tracking")]
		public bool ignoreXR = false;

		[Header ("References")]
		public Transform XRrig_MainCamera;
		public Transform XRrig_LeftController;
		public Transform XRrig_RightController;
		public Transform mainMenuHolder;
		
		[Header("System")]
		public TargetHZ targetHZ = TargetHZ.H90;
		public enum TargetHZ { H60, H72, H90, H120 };

		/// <summary>Main system manager, provides refs to XR rig components</summary>
		public static SystemManager instance = null;

		void Awake () {

			// Make a singleton for this so it's reachable 
			if (instance == null) {
				instance = this;
				DontDestroyOnLoad (transform.gameObject);
			
				// Set time and location to avoid comma / period issues
				System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
				
				SetApplicationFramerate();

				CheckReferences ();
				
				if(ignoreXR)
					DisableXRrig();
			}
			else if (instance != this) Destroy (this.gameObject);
		}

		/// <summary>
		/// In order to get a fixed timestep for experiments, we set the application to a fixed rate </summary>
		private void SetApplicationFramerate() {
			QualitySettings.vSyncCount = 0; // Don't vsync
			int tframerate = int.Parse(targetHZ.ToString().Substring(1,targetHZ.ToString().Length-1));
			Application.targetFrameRate = tframerate;
			AddToLog("Target framerate set to " + tframerate);
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

		/// <summary>
		/// 
		/// </summary>
		void DisableXRrig () {
			XRrig_MainCamera.transform.parent.transform.localPosition = new Vector3(0, 1.675f, 0);
			XRrig_RightController.GetComponent<XRInteractorLineVisual>().enabled = false;
			XRrig_LeftController.GetComponent<XRInteractorLineVisual>().enabled = false;

			// Hide hands
			XRrig_RightController.GetComponentInChildren<SkinnedMeshRenderer>(true).enabled = false;
			XRrig_LeftController.GetComponentInChildren<SkinnedMeshRenderer>(true).enabled = false;
		}

	#region MISC	

			public void AddToLog(string _msg) {
				if (showLog)
					LogUtilities.AddToLog(_msg, "eDIA", taskColor);
			}
			

	#endregion	// -------------------------------------------------------------------------------------------------------------------------------
	}
}