using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using eDIA;

/*
	This is a DEMO script for a very simple task.
	All main functionalities to run the Experiment, UXF and Trial statemachine are in TaskManagerBase so no need to worry about that.

	To start your own TaskManager for your experiment task: 

	1. Copy/Paste the complete 'DemoTask' folder from /eDIA/Samples to the /Assets/ folder in your project
	2. Rename the script file and class to your Taskname
	3. Switch the inspector to 'debug' mode (3 dots right topcorner)
	4. Find [ SYSTEM ] > [ EXP ] gameobject and replace 'TaskManagerDemo' script entry in the inspector with your TaskManager script and switch inspector mode back again


*/

	// USER TASK CONTROL
	public class TaskManagerDemo : TaskManagerBase {

		[Header(("Task related refs"))]
		public GameObject theCube;
		
		private Coroutine moveRoutine = null;

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
			trialSequence.Add(new TrialStep("Present Cube", TaskStep1));
			trialSequence.Add(new TrialStep("Move cube, wait on user input", TaskStep2));
			trialSequence.Add(new TrialStep("Stop moving, change color", TaskStep3));
			trialSequence.Add(new TrialStep("Wait ", TaskStep4));
			//trialSequence.Add( new TrialStep("last step", TaskStepX) );
			// etc
		}

		/// <summary>Present Cube</summary>
		void TaskStep1() {
			AddToLog("Step:" + (currentStep + 1) +" > " + trialSequence[currentStep].title);

			theCube.gameObject.SetActive(true);
			theCube.transform.position = new Vector3(0, XRrig_MainCamera.position.y, theCube.transform.position.z);

			Invoke("NextStepFromUserOrSceneOrButtonOrTimerOrWhatever", 1f);
		}
		
		/// <summary>Move cube, wait on user input</summary>
		void TaskStep2() {
			AddToLog("Step:" + (currentStep + 1) +" > " + trialSequence[currentStep].title);

			if (moveRoutine == null) {
				moveRoutine = StartCoroutine("MoveCube");
			}

			EventManager.StartListening("EvProceed", OnEvProceed); //! Continues to the next step
		}

		/// <summary>Stop moving, change color</summary>
		void TaskStep3() {
			AddToLog("Step:" + (currentStep + 1) +" > " + trialSequence[currentStep].title);

			if (moveRoutine != null) {
				StopCoroutine(moveRoutine);
				moveRoutine = null;
			}
			
			Color newCol;
			if (ColorUtility.TryParseHtmlString(Session.instance.CurrentTrial.settings.GetString("color"), out newCol))
				theCube.GetComponent<MeshRenderer>().material.color = newCol;
			else newCol = Color.magenta;

			NextStepFromUserOrSceneOrButtonOrTimerOrWhatever();
		}

		/// <summary>Wait</summary>
		void TaskStep4() {
			AddToLog("Step:" + (currentStep + 1) +" > " + trialSequence[currentStep].title);

			Invoke("NextStepFromUserOrSceneOrButtonOrTimerOrWhatever", 2f); //! Alternative way to call a method after X seconds
		}

		/// <summary>Call this from your code to proceed to the next step</summary>
		public void NextStepFromUserOrSceneOrButtonOrTimerOrWhatever() {
			NextStep();
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HELPERS
		/// <summary>Moves the cube up or down depending on the setting `direction` in the trial settings.</summary>
		IEnumerator MoveCube () {
			float increment = Session.instance.CurrentTrial.settings.GetInt("direction") == 1 ? 0.001f : -0.001f;
			
			while (true) {
				theCube.transform.Translate(new Vector3(0, increment ,0), Space.World);
				yield return new WaitForEndOfFrame();
			}
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

			Debug.Log("<color=#ffffff> >>> Welcome to the experiment, please click button A to continue </color>");
		}

		/// <summary>Called from Experiment manager</summary>
		public override void OnSessionEnd () {
			base.OnSessionEnd();

			// Show Outro
			// System awaits 'EvProceed' event automaticcaly to proceed to finalizing session. 

			Debug.Log("<color=#ffffff> >>> Thank you for participating in the experiment, please click button A to end this session </color>");

		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BREAK

		// If there is a BREAK in the experiment, these methods get called
		public override void OnSessionBreak() {
			base.OnSessionBreak();

			Debug.Log("<color=#ffffff> >>> Take a short break </color>");
		}

		public override void OnSessionResume () {
			base.OnSessionResume();

			Debug.Log("<color=#ffffff> >>> Resuming experiment </color>");
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BLOCK INTRODUCTION
		
		// If there is a INTRODUCTION at the start at some block, these methods get called
		// Use ExperimentManager.Instance.experimentConfig.GetBlockIntroduction(Session.instance.currentBlockNum) to get the text.
		
		/// <summary>Hook up to Experiment OnSessionBreak event</summary>
		public override void OnBlockIntroduction() {
			base.OnBlockIntroduction();

			Debug.Log("<color=#ffffff> >>> "+ ExperimentManager.Instance.experimentConfig.GetBlockIntroduction(Session.instance.currentBlockNum) + "</color>");
		}

		/// <summary>Hook up to Experiment OnSessionResume event</summary>
		public override void OnBlockContinue (Trial trial) {
			base.OnBlockContinue(trial);

		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}