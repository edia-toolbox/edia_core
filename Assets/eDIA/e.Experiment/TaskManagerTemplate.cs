using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using eDIA;

namespace TASK {

		/// <summary>
		/// This is a DEMO script for a very simple task.
		/// All main functionalities to run the Experiment, UXF and Trial statemachine are in TaskManagerBase so no need to worry about that.<br/><br/>
		/// <para>To start your own TaskManager for your experiment task: 
		/// 1. Copy/Paste the complete 'DemoTask' folder from /eDIA/Samples to the /Assets/ folder in your project<br/>
		/// 2. Rename the script file and class to your Taskname<br/>
		/// 3. Switch the inspector to 'debug' mode (3 dots right topcorner)<br/>
		/// 4. Find [ SYSTEM ] > [ EXP ] gameobject and replace 'TaskManagerDemo' script entry in the inspector with your TaskManager script and switch inspector mode back again</para>
		/// </summary>
	public class TaskManagerTemplate : TaskManagerBase {

#region MONO METHODS 

		void OnDisable() {
		}

		void OnDestroy() {
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region OVERRIDE INHERITED METHODS
		public override void OnEnable() {
			base.OnEnable();

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
			base.OnSessionEndUXF();

			// Add actions to do when session has ended, like showing message to the user
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
			base.OnSessionStart();

			// Show Intro
			// System awaits 'EvProceed' event automaticcaly to proceed to first trial. 
		}

		/// <summary>Called from Experiment manager</summary>
		public override void OnSessionEnding () {
			base.OnSessionEnding();

		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BREAK

		// If there is a BREAK in the experiment, these methods get called

		public override void OnSessionBreak() {

		}

		public override void OnSessionResume () {

		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BLOCK INTRODUCTION
		
		// If there is a INTRODUCTION at the start at some block, these methods get called
		// Use ExperimentManager.Instance.experimentConfig.GetBlockIntroduction(Session.instance.currentBlockNum) to get the text.
		
		/// <summary>Called when the block introduction starts</summary>
		public override void OnBlockIntroduction() {
			base.OnBlockIntroduction();

		}

		/// <summary>Called when block resumes</summary>
		public override void OnBlockResume () {
			base.OnBlockResume();

		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}
}