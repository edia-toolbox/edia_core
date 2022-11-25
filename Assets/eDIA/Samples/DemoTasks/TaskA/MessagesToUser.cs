using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;

namespace TASK {
	
	public class MessagesToUser : MonoBehaviour {

		public MessagePanelInVR messagePanelInVR = null;  

		private void Awake() {
			if (messagePanelInVR == null)
				Debug.LogError("MessagePanel reference empty");
		}

		public void OnSessionStart() {
			messagePanelInVR.ShowMessage("Welcome to the experiment", true);
		}

		public void OnSessionBreak() {
			messagePanelInVR.ShowMessage("Take a short break, \nClick button to continue", true);
		}

		public void OnSessionEnd () {
			messagePanelInVR.ShowMessage("Session ended, logfiles saved");
		}


	}

}