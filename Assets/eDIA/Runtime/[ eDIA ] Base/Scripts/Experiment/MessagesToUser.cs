using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UXF;

namespace TASK {
	
	public class MessagesToUser : MonoBehaviour {

		public void OnSessionStart() {
			//Experiment.Instance.ShowMessageToUser(new List<string>() { string.Format("Welcome to the {0} experiment", Session.instance.experimentName), "Second page for testing" });
			//MessagePanelInVR.Instance.ShowMessage(new List<string>() { "Welcome to the experiment", "Second page for testing" });
			Experiment.Instance.ShowMessageToUser("Hello");
		}

		public void OnSessionPaused() {
			Experiment.Instance.ShowMessageToUser("Take a short break");
		}

		public void OnSessionEnd () {
			Experiment.Instance.ShowMessageToUser("Session ended, logfiles saved");
		}
	}
}