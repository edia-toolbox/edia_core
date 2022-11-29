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
			AddToTrialSequence(TaskStep1);
			AddToTrialSequence(TaskStep2);
			AddToTrialSequence(TaskStep3);
		}


// -------------------------------------------------------------------------------------------------------------------------------
#region TASK STEPS

		/// <summary>Present Cube</summary>
		public void TaskStep1 () {

			XRrigUtilities.EnableXRInteraction (false);

			messagePanelInVR.ShowMessage("blaaa");
			Experiment.Instance.NextStep (5f);
		}

		/// <summary>Move cube, wait on user input</summary>
		public void TaskStep2 () {

			XRrigUtilities.EnableXRInteraction (true);

			qpanel.SetActive(true);
			qpanelTextField.text = "Qtype: " + Session.instance.CurrentBlock.settings.GetStringList("qtypes")[Session.instance.CurrentTrial.settings.GetInt("qtype")];

			Experiment.Instance.EnableExperimentProceed (true); // enable proceed button
			
			messagePanelInVR.ShowMessage("Now you have to fill in some questions", true);
			controllerListener.EnableRemapping("TriggerPressed", true);

		}

		/// <summary>Stop moving, change color</summary>
		public void TaskStep3 () {

			qpanel.SetActive(false);

			controllerListener.EnableRemapping("TriggerPressed", false);

			messagePanelInVR.ShowMessage("Thank you for your answer");
			Experiment.Instance.NextStep(4f);
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
			messagePanelInVR.ShowMessage(Session.instance.CurrentBlock.settings.GetString("introduction"), true);
		}


		public override void OnStartNewTrial () {
		}

		public override void OnBetweenSteps () {
			// messagePanelInVR.HidePanel();
		}

		/// <summary>Called when block ends</summary>
		public override void OnBlockEnd () {
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}

}