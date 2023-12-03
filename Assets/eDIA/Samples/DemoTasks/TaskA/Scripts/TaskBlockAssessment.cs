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
			Debug.Log("TaskStep1 ");
			MessagePanelInVR.Instance.ShowMessage("You are about to see a Questionnaire");

			Experiment.Instance.ProceedWithDelay (2f);
		}

		/// <summary>Move cube, wait on user input</summary>
		public void TaskStep2 () {
			Debug.Log("TaskStep2 ");
			XRManager.Instance.EnableXRInteraction (true);

			qpanel.SetActive(true);
			qpanelTextField.text = "Qtype: " + Session.instance.CurrentBlock.settings.GetStringList("qtypes")[Session.instance.CurrentTrial.settings.GetInt("qtype")];

			//XRControllerListener.EnableRemapping("TriggerPressed", true);

			Experiment.Instance.WaitOnProceed (); // enable proceed button

		}

		/// <summary>Stop moving, change color</summary>
		public void TaskStep3 () {
			Debug.Log("TaskStep3 ");
			qpanel.SetActive(false);

			MessagePanelInVR.Instance.ShowMessage("Thank you for your answer");

			Experiment.Instance.ProceedWithDelay(3f);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK HELPERS

		public void TriggerPressed (InputAction.CallbackContext context) {
			XRControllerListener.EnableRemapping("TriggerPressed", false);
			Experiment.Instance.Proceed();
		}

		public void PanelButtonPressed (int index)
		{
			Debug.Log(string.Join("Button pressed: ", index == 0 ? "Left" : "Right"));
			Experiment.Instance.Proceed();
		}



#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region OPTIONAL METHODS FOR YOUR TASK
 /*  

	GUIDELINES	
	* Dont use any call to EvProceed in these, the statemachine for that is handled 

 */
			
		public override void OnBlockStart () {
		}

		public override void OnBlockIntro () {
		}

		public override void OnStartTrial () {
			XRManager.Instance.EnableXRInteraction (false);
		}

		public override void OnEndTrial () {
		}

		public override void OnBetweenSteps () {
		}

		public override void OnBlockOutro () {
		}

		public override void OnBlockEnd () {
		}



#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}

};