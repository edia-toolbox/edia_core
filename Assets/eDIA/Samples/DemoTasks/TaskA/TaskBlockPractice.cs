using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UXF;
 using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace TASK {
	
	[System.Serializable]
	public class TaskBlockPractice : TaskBlock {

		[Header (("Task related refs"))]
		public GameObject theCube;
		private Coroutine moveRoutine = null;

		[Header("Helpers")]
		[Tooltip("Makes it possible to map controller buttons to methods in this script.")]
		public XRControllerInputRemapper XRControllerListener = null; 

		private void Awake() {

			/*
				Each trial exists out of a sequence of steps. 
				In order to use them, we need to add the methods of this task to the trial sequence.
			*/
			AddToTrialSequence(TaskStep1);
			AddToTrialSequence(TaskStep2);
			AddToTrialSequence(TaskStep3);
			AddToTrialSequence(TaskStep4);
		}

		// Script gets enabled when it is it's turn.
		void OnEnable() {
			/*
				By default the hands are reacting on the trigger and press, but we overrule it here by this method.
			*/
			XRManager.Instance.EnableCustomHandPoses(true);
		}

		// Script gets disabled when it it's turn is over.
		void OnDisable() {
			XRManager.Instance.EnableCustomHandPoses(false);
		}

// -------------------------------------------------------------------------------------------------------------------------------
#region TASK STEPS

		/*

			This example script represents a task. 
			A rather useluss one, but the main purpose is to show the options the eDIA framework gives you to work with.

			XRManager.Instance.xxxxx => All things XR related
			Experiment.instance.xxxxx => All things related to the progress of the experiment, logging data, etc


		*/

		/// <summary>Present Cube</summary>
		public void TaskStep1 () {

			// Set a custom hand pose
			XRManager.Instance.SetHandPose("point");

			// Enable the pause button on the control panel
			Experiment.Instance.EnablePauseButton (true);

			// Disable XR interaction from the user
			XRManager.Instance.EnableXRInteraction (false);

			// Task stuff
			theCube.gameObject.SetActive (true);
			theCube.transform.position = new Vector3 (0, XRManager.Instance.XRCam.position.y, Session.instance.CurrentBlock.settings.GetFloat ("distance_cube"));

			/* 
				Continue with the next step, either:
				Directly: NextStep()
				Delayed: NextStepWithDelay (seconds as float)
				
			*/
			Experiment.Instance.NextStepWithDelay (Session.instance.CurrentBlock.settings.GetFloat ("timer_showcube"));
		}

		/// <summary>Move cube, wait on user input</summary>
		public void TaskStep2 () {

			// Task stuff
			if (moveRoutine == null) {
				moveRoutine = StartCoroutine ("MoveCube");
			}

			// Enable interaction from the user. The system will automaticly enable the Ray Interaction for the active hands set in the settings.
			XRManager.Instance.EnableXRInteraction (true);

			// Show message to user and allow proceeding to NextStep by pressing the button.
			MessagePanelInVR.Instance.ShowMessage("Click button below to continue", true);

			// Tell the system to wait on button press. Which will also enable the button on the controlpanel to overrule the user
			Experiment.Instance.WaitOnProceed (); 
		}

		/// <summary>Stop moving, change color</summary>
		public void TaskStep3 () {

			// Task stuff
			if (moveRoutine != null) {
				StopCoroutine (moveRoutine);
				moveRoutine = null;
			}

			Color newCol;
			if (ColorUtility.TryParseHtmlString (Session.instance.CurrentBlock.settings.GetStringList ("cube_colors") [Session.instance.CurrentTrial.settings.GetInt ("color")], out newCol))
				theCube.GetComponent<MeshRenderer> ().material.color = newCol;
			else newCol = Color.magenta;

			// Reset the handpose to idle state
			XRManager.Instance.SetHandPose("idle");

			// Disable the ray on the hand(s)
			XRManager.Instance.EnableXRInteraction (false);

			/* 
				The XRControllerListener is a separate scrtip that allows remapping a XRcontroller input action to a public method.
				Very usefull to enable input in one or more steps of the trial.

				In this case enabling "TriggerPressed" predefined mapping on the script.
			*/
			XRControllerListener.EnableRemapping("TriggerPressed", true);

			// Show message to user
			MessagePanelInVR.Instance.ShowMessage("To continue click the trigger button on the controller");
		}

		// Callback method for remapping XR controller input to this method -> see XRControllerInputRemapper script on this object
		public void TriggerPressed (InputAction.CallbackContext context) {
			// First switch off the listener
			XRControllerListener.EnableRemapping("TriggerPressed", false);

			// Continue the trial
			Experiment.Instance.NextStep();
		}

		/// <summary>Wait</summary>
		public void TaskStep4 () {

			MessagePanelInVR.Instance.ShowMessage("Thank you, end of trial");

			Experiment.Instance.NextStepWithDelay (Session.instance.CurrentBlock.settings.GetFloat ("timer_wait"));
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region TASK HELPERS

		/// <summary>Moves the cube up or down depending on the setting `direction` in the trial settings.</summary>
		IEnumerator MoveCube () {
			float increment = Session.instance.CurrentTrial.settings.GetInt ("direction") == 1 ? 0.001f : -0.001f;

			while (true) {
				theCube.transform.Translate (new Vector3 (0, increment, 0), Space.World);
				yield return new WaitForEndOfFrame ();
			}
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region OPTIONAL METHODS FOR YOUR TASK
 /*  

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
			XRManager.Instance.EnableXRInteraction (false);
		}

		public override void OnBetweenSteps () {
		}

		/// <summary>Called when block ends</summary>
		public override void OnBlockEnd () {
			// Clean up
			theCube.gameObject.SetActive (false);
			XRManager.Instance.EnableCustomHandPoses(false);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}

}