using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UXF;

namespace TASK {
	
	public class MessagesToUser : MonoBehaviour {

		public void OnSessionStart() {
			//Xperiment.Instance.ShowMessageToUser(new List<string>() { string.Format("Welcome to the {0} experiment", Session.instance.experimentName), "Second page for testing" });
			//MessagePanelInVR.Instance.ShowMessage(new List<string>() { "Welcome to the experiment", "Second page for testing" });
			Xperiment.Instance.ShowMessageToUser("Hello");
		}

		public void OnSessionPaused() {
			Xperiment.Instance.ShowMessageToUser("Take a short break");
		}

		public void OnSessionEnd () {
			Xperiment.Instance.ShowMessageToUser("Session ended, logfiles saved");
		}
	}
}