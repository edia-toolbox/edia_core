using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UXF;
using TMPro;

namespace TASK {
	
	[System.Serializable]
	public class TaskBlockAssessment : MonoBehaviour {

		[Header (("Task related refs"))]
		public MessagePanelInVR messagePanelInVR;
		public GameObject qpanel;
		public TextMeshProUGUI qpanelTextField;

		private void Start() {
		}

// -------------------------------------------------------------------------------------------------------------------------------
#region TASK STEPS

		/// <summary>Present Cube</summary>
		public void TaskStep1 () {

			XRrigUtilities.EnableXRInteraction (false);

			messagePanelInVR.ShowMessage("Now you have to fill in some questions");
			ExperimentManager.Instance.EnableExperimentProceed (true); // enable proceed button
		}

		/// <summary>Move cube, wait on user input</summary>
		public void TaskStep2 () {

			XRrigUtilities.EnableXRInteraction (true);

			qpanel.SetActive(true);
			qpanelTextField.text = "Qtype: " + Session.instance.CurrentBlock.settings.GetStringList("qtypes")[Session.instance.CurrentTrial.settings.GetInt("qtype")];

			ExperimentManager.Instance.EnableExperimentProceed (true); // enable proceed button
		}

		/// <summary>Stop moving, change color</summary>
		public void TaskStep3 () {

			qpanel.SetActive(false);

			messagePanelInVR.ShowMessage("Thank you for your answer");
			TaskManager.Instance.NextStep(4f);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EVENT HOOKS
			
		/// <summary>Called when block starts</summary>
		public void OnBlockStart () {
			// TODO
			Debug.Log(name + " > OnBlockStart");
		}

		/// <summary>Called when block ends</summary>
		public void OnBlockEnd () {
			// TODO
			Debug.Log(name + " > OnBlockEnd");
		}

		public void OnStartNewTrial () {
			Debug.Log("OnStartNewTrial");
			messagePanelInVR.HidePanel();
		}

		public void OnBetweenSteps () {
			Debug.Log("OnBetweenSteps");
			messagePanelInVR.HidePanel();
		}

		/// <summary>Called from Experiment manager</summary>
		public void OnSessionStart() {
			messagePanelInVR.ShowMessage("Welcome to the experiment, please click button to continue", true);
		}

		// If there is a BREAK in the experiment, these methods get called
		public void OnSessionBreak() {
			messagePanelInVR.ShowMessage("Take a short break, \nClick button to continue", true);
		}

		public void OnSessionEnd () {
			messagePanelInVR.ShowMessage("Session ended, logfiles saved");
		}

		/// <summary>Called when the block introduction starts</summary>
		public void OnSessionResume () {
			messagePanelInVR.ShowMessage("Resuming experiment", 2f);
		}

		/// <summary>Called when the block introduction starts</summary>
		public void OnBlockIntroduction() {
			messagePanelInVR.ShowMessage(ExperimentManager.Instance.experimentConfig.GetBlockIntroduction(), true);
		}

		/// <summary>Called when block resumes</summary>
		public void OnBlockResumeAfterIntroduction () {

		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}

}