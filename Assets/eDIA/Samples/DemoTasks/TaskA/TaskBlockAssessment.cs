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
		public MessagePanelInVR messagePanelInVR;
		public GameObject qpanel;
		public TextMeshProUGUI qpanelTextField;
		public InputActionReference inputActionSubmit;

		public ControllerInputRemapper controllerListener = null;

		private void Awake() {
			// Set up sequence
			trialSteps.Add(TaskStep1);
			trialSteps.Add(TaskStep2);
			trialSteps.Add(TaskStep3);

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
			
			controllerListener.EnableRemapping("TriggerPressed", true);

		}

		/// <summary>Stop moving, change color</summary>
		public void TaskStep3 () {

			qpanel.SetActive(false);

			controllerListener.EnableRemapping("TriggerPressed", false);

			messagePanelInVR.ShowMessage("Thank you for your answer");
			TaskManager.Instance.NextStep(4f);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK HELPERS

		public void TriggerPressed (InputAction.CallbackContext context) {
			Debug.Log("TriggerPressed");
			EventManager.TriggerEvent(eDIA.Events.Core.EvProceed, null);
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
			messagePanelInVR.ShowMessage(ExperimentManager.Instance.experimentConfig.GetBlockIntroduction(), true);
		}

		/// <summary>Called when block resumes</summary>
		public override void OnBlockResumeAfterIntro () {

		}

		public override void OnStartNewTrial () {
			messagePanelInVR.HidePanel();
		}

		public override void OnBetweenSteps () {
			messagePanelInVR.HidePanel();
		}

		/// <summary>Called when block ends</summary>
		public override void OnBlockEnd () {
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}

}