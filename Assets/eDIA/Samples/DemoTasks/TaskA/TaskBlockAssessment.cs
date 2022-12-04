using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UXF;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace TASK {
	
	[System.Serializable]
	public class TaskBlockAssessment : TaskBlock {

		[Header (("Task related refs"))]
		public GameObject qpanel;
		public TextMeshProUGUI qpanelTextField;

		public XRControllerInputRemapper XRControllerListener = null;

		private void Awake() {
			// Set up sequence
			AddToTrialSequence(TaskStep1);
			AddToTrialSequence(TaskStep2);
			AddToTrialSequence(TaskStep3);
		}

		// Script gets enabled at the moment it is it's turn in the experiment
		void OnEnable() {
		}

		// Script gets disabled at the moment it's turn is over
		void OnDisable() {
		}

// -------------------------------------------------------------------------------------------------------------------------------
#region TASK STEPS

		/// <summary>Present Cube</summary>
		public void TaskStep1 () {

			MessagePanelInVR.Instance.ShowMessage("You are about to see a Questionnaire");

			Experiment.Instance.NextStepWithDelay (2f);
		}

		/// <summary>Move cube, wait on user input</summary>
		public void TaskStep2 () {

			XRManager.instance.EnableXRInteraction (true);

			qpanel.SetActive(true);
			qpanelTextField.text = "Qtype: " + Session.instance.CurrentBlock.settings.GetStringList("qtypes")[Session.instance.CurrentTrial.settings.GetInt("qtype")];
			
			MessagePanelInVR.Instance.ShowMessage("Now you have to fill in some questions");

			XRControllerListener.EnableRemapping("TriggerPressed", true);

			Experiment.Instance.WaitOnProceed (); // enable proceed button

		}

		/// <summary>Stop moving, change color</summary>
		public void TaskStep3 () {

			qpanel.SetActive(false);

			

			MessagePanelInVR.Instance.ShowMessage("Thank you for your answer");

			Experiment.Instance.NextStepWithDelay(3f);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK HELPERS

		public void TriggerPressed (InputAction.CallbackContext context) {
			XRControllerListener.EnableRemapping("TriggerPressed", false);
			Experiment.Instance.NextStep();
		}



#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region OPTIONAL METHODS FOR YOUR TASK
 /*  

	GUIDELINES	
	* Dont use any call to EvProceed in these, the statemachine for that is handled 

 */
			
		/// <summary>Called when block starts</summary>
		public override void OnBlockStart () {
		}

		/// <summary>Called when this block has a introduction text in the json</summary>
		public override void OnBlockIntroduction() {
			MessagePanelInVR.Instance.ShowMessage(Session.instance.CurrentBlock.settings.GetString("introduction"));
		}

		public override void OnStartNewTrial () {
			XRManager.instance.EnableXRInteraction (false);
		}

		public override void OnBetweenSteps () {
			
		}

		/// <summary>Called when block ends</summary>
		public override void OnBlockEnd () {
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}

}