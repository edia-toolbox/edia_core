using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UXF;

namespace TASK {
	
	public class MessagesToUser : MonoBehaviour {

		public void OnSessionStart() {
			Experiment.Instance.ShowMessageToUser("Welcome to the experiment!");
		}

		public void OnSessionPaused() {
			Experiment.Instance.ShowMessageToUser("Take a short break");
		}

		public void OnSessionEnd () {
			Experiment.Instance.ShowMessageToUser("Session ended, logfiles saved");
		}
	}
}