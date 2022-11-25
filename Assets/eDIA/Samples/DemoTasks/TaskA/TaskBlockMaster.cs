using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UXF;

namespace TASK {
	
	[System.Serializable]
	public class TaskBlockMaster : MonoBehaviour {


#region EVENT HOOKS
			
		/// <summary>Called from Experiment manager</summary>
		public void OnSessionStart() {
			// messagePanelInVR.ShowMessage("Welcome to the experiment, please click button to continue", true);
		}

		// If there is a BREAK in the experiment, these methods get called
		public void OnSessionBreak() {
			// messagePanelInVR.ShowMessage("Take a short break, \nClick button to continue", true);
		}

		public void OnSessionEnd () {
			// messagePanelInVR.ShowMessage("Session ended, logfiles saved");
		}

		/// <summary>Called when the block introduction starts</summary>
		public void OnSessionResume () {
			// messagePanelInVR.ShowMessage("Resuming experiment", 2f);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}

}