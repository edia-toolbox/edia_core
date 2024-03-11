using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;

namespace TASK {
	
	public class MessagesToUser : MonoBehaviour {

		public void OnSessionStart() {
			MessagePanelInVR.Instance.ShowMessage(new List<string>() { "Welcome to the experiment", "Second page for testing" });
		}

		public void OnSessionPaused() {
			MessagePanelInVR.Instance.ShowMessage("Take a short break");
		}

		public void OnSessionEnd () {
			MessagePanelInVR.Instance.ShowMessage("Session ended, logfiles saved");
		}
	}
}