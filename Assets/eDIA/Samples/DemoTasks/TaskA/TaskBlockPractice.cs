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
		public MessagePanelInVR messagePanelInVR;
		private Coroutine moveRoutine = null;

		public ControllerInputRemapper controllerListener = null; // controller button remapper to proceed

		private void Awake() {
			AddToTrialSequence(TaskStep1);
			AddToTrialSequence(TaskStep2);
			AddToTrialSequence(TaskStep3);
			AddToTrialSequence(TaskStep4);
		}

		private void Start() {
			XRrigUtilities.EnableCustomHandPoses(false);
		}

// -------------------------------------------------------------------------------------------------------------------------------
#region TASK STEPS

		/// <summary>Present Cube</summary>
		public void TaskStep1 () {

			XRrigUtilities.SetHandPose("point");

			ExperimentManager.Instance.EnableExperimentPause (true);
			XRrigUtilities.EnableXRInteraction (false);

			theCube.gameObject.SetActive (true);
			theCube.transform.position = new Vector3 (0, XRrigUtilities.GetXRcam().position.y, Session.instance.CurrentBlock.settings.GetFloat ("distanceCube"));

			TaskManager.Instance.NextStep (Session.instance.CurrentBlock.settings.GetFloat ("timerShowCube"));
		}

		/// <summary>Move cube, wait on user input</summary>
		public void TaskStep2 () {

			if (moveRoutine == null) {
				moveRoutine = StartCoroutine ("MoveCube");
			}

			XRrigUtilities.EnableXRInteraction (true);

			messagePanelInVR.ShowMessage("Click button to continue");

			ExperimentManager.Instance.EnableExperimentProceed (true); // enable proceed button for experiment
			controllerListener.EnableRemapping("TriggerPressed", true); // enable controller button remapper to proceed
		}

		/// <summary>Stop moving, change color</summary>
		public void TaskStep3 () {

			controllerListener.EnableRemapping("TriggerPressed", false);

			if (moveRoutine != null) {
				StopCoroutine (moveRoutine);
				moveRoutine = null;
			}

			Color newCol;
			if (ColorUtility.TryParseHtmlString (Session.instance.CurrentBlock.settings.GetStringList ("cubeColors") [Session.instance.CurrentTrial.settings.GetInt ("color")], out newCol))
				theCube.GetComponent<MeshRenderer> ().material.color = newCol;
			else newCol = Color.magenta;

			XRrigUtilities.SetHandPose("idle");

			TaskManager.Instance.NextStep();
		}

		/// <summary>Wait</summary>
		public void TaskStep4 () {
			TaskManager.Instance.NextStep (Session.instance.CurrentBlock.settings.GetFloat ("timerWait"));
			Debug.Log("TaskStep4 started");
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK HELPERS

		public void TriggerPressed (InputAction.CallbackContext context) {
			Debug.Log("TriggerPressed");
			EventManager.TriggerEvent(eDIA.Events.Core.EvProceed, null);
		}


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