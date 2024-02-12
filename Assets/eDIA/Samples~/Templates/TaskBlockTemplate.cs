using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UXF;
 using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace TASK {
	
	[System.Serializable]
	public class TaskBlockTemplate : TaskBlock {

		/*

			Task related parameters

		*/


		private void Awake() {

			/*
				Each trial exists out of a sequence of steps. 
				In order to use them, we need to add the methods of this task to the trial sequence.
			*/
			AddToTrialSequence(TaskStep1);
			AddToTrialSequence(TaskStep2);
			//! etc
		}

		// Script gets enabled when it is it's turn.
		void OnEnable() {
		}

		// Script gets disabled when it it's turn is over.
		void OnDisable() {
		}

// -------------------------------------------------------------------------------------------------------------------------------
#region TASK STEPS

		/*
			
			Methods that define the steps taken in this trial.

			Main scripts to call:
			* XRManager.Instance.xxxxx => All things XR related
			* Experiment.instance.xxxxx => All things related to the progress of the experiment, logging data, etc

		*/

		/// <summary>Present Cube</summary>
		void TaskStep1 () {

			// Enable the pause button on the control panel
			Experiment.Instance.EnablePauseButton (true);

			// Disable XR interaction from the user
			XRManager.Instance.EnableXRInteraction (false);

			/* 
				Continue with the next step, either:
				* Directly: NextStep()
				* Delayed: NextStepWithDelay (seconds as float)
				
			*/

			Experiment.Instance.ProceedWithDelay (Session.instance.CurrentBlock.settings.GetFloat ("timer_showcube"));
		}

		/// <summary>Move cube, wait on user input</summary>
		void TaskStep2 () {

			// Enable interaction from the user. The system will automaticly enable the Ray Interaction for the active hands set in the settings.
			XRManager.Instance.EnableXRInteraction (true);

			// Show message to user and allow proceeding to NextStep by pressing the button.
			MessagePanelInVR.Instance.ShowMessage("Click button below to continue", true);

			// Tell the system to wait on button press. Which will also enable the button on the controlpanel to overrule the user
			Experiment.Instance.WaitOnProceed (); 
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK HELPERS


		/*
			Methods related to the task
		*/




#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region STATEMACHINE OVERRIDES

		/*  
			Methods from the statemachine that can be used in the task.
		*/
			
		public override void OnBlockStart () {
		}

		public override void OnStartTrial () {
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

}