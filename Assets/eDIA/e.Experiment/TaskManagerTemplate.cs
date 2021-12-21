using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using eDIA;

namespace SFEM {

	// USER TASK CONTROL
	public class TaskManagerTemplate : TaskManagerBase {

#region MONO METHODS 
		public override void OnEnable() {
			base.OnEnable();

			// ---------------------------------------------------------------------------------------------
			// Custom task related actions go down here ▼

			// ..
		}

		void OnDisable() {
		}

		void OnDestroy() {
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region OVERRIDE INHERITED METHODS
		
		public override void OnEnable() {
			base.OnEnable(); //! Do not remove

				// ---------------------------------------------------------------------------------------------
				// Custom task related actions go down here ▼

				// ..
		}

		public override void Awake() {
			base.Awake(); // Tries to get the references to the MainCamera, Right/LeftController transforms
		
			// ---------------------------------------------------------------------------------------------
			// Custom task related actions go down here ▼

			// ..
		}

		public override void Start() {
			base.Start();

			TaskSequenceSteps();

			// ---------------------------------------------------------------------------------------------
			// Custom task related actions go down here ▼

			// ..
		}

		public override void ResetTrial() {
			base.ResetTrial();

			// Add resetting of values for the start of a new trial here
			// someValue = 0;
		}

		public override void OnSessionEndUXF () {
			base.OnSessionEndUXF(); //! Do not remove

			// Add actions to do when session has ended, like showing message to the user
			EventManager.TriggerEvent("EvShowMessage", new eParam("Session ended, logfiles saved"));

		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EDIA XR RIG 

		/// <summary>Setup rig with things we need for this task. </summary>
		public override void OnEvFoundXRrigReferences(eParam e) {
			base.OnEvFoundXRrigReferences(e); // Do system related hidden actions first

			// ---------------------------------------------------------------------------------------------
			// Custom task related actions go down here ▼

		}

		public void XRrigCleanUP () {
			// Remove shizzle from XR rig that is only for this task
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK STRUCTURE METHODS 

		/// <summary>The steps the trial goes through</summary>
		void TaskSequenceSteps() {
			trialSequence.Add(new TrialStep("First step", TaskStep1));
			trialSequence.Add(new TrialStep("secundo step", TaskStep2));
			trialSequence.Add(new TrialStep("last step", TaskStepX));
			//trialSequence.Add( new TrialStep("last step", TaskStepX) );
			// etc
		}

		void TaskStep1() {
			AddToLog("Step:" + (currentStep + 1) +" > " + trialSequence[currentStep].title);
		}

		void TaskStep2() {
			AddToLog("Step:" + (currentStep + 1) +" > " + trialSequence[currentStep].title);
		}

		// Example as last step to present the result
		void TaskStepX() {
			AddToLog("Step:" + (currentStep + 1) +" > " + trialSequence[currentStep].title);

			GenerateTaskTrialResults();
		}

		/// <summary>Call this from you code to proceed to the next step</summary>
		public void NextStepFromUserOrSceneOrButtonOrTimerOrWhatever() {
			NextStep();
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HELPERS

		/// <summary>Calculate scores and give values to log to UXF</summary>
		void GenerateTaskTrialResults() {

			// Easy adding of a tasks parameter in the trial results like below. 
			AddToTrialResults("some_key", "somevalue");

			AddToLog("GenerateTaskTrialResults");
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region SESSION START / END
		
		// Every Experiment must has a START and an END event!!
		// Use these to show an introduction at the start and a thank-you or score overview in the end. 

		/// <summary>Called from Experiment manager</summary>
		public override void OnSessionStart() {
			EventManager.TriggerEvent("EvShowMessage", new eParam("Welcome to the experiment, please click button to continue"));

			// Show Intro
			// System awaits 'EvProceed' event automaticcaly to proceed to first trial. 
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BREAK

		// If there is a BREAK in the experiment, these methods get called

		public override void OnSessionBreak() {
			EventManager.TriggerEvent("EvShowMessage", new eParam("Take a short break, \nClick button to continue"));

		}

		public override void OnSessionResume () {
			EventManager.TriggerEvent("EvShowMessage", new eParam("Resuming experiment"));

		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BLOCK INTRODUCTION
		
		// If there is a INTRODUCTION at the start at some block, these methods get called
		// Use ExperimentManager.Instance.experimentConfig.GetBlockIntroduction(Session.instance.currentBlockNum) to get the text.
		
		/// <summary>Hook up to Experiment OnSessionBreak event</summary>
		public override void OnBlockIntroduction() {
			EventManager.TriggerEvent("EvShowMessage", new eParam(  ExperimentManager.Instance.experimentConfig.GetBlockIntroduction(Session.instance.currentBlockNum) ));

		}

		/// <summary>Hook up to Experiment OnSessionResume event</summary>
		public override void OnBlockResume () {

		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}
}