using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UXF;
 
namespace TASK {
	
	[System.Serializable]
	public class TaskBlockPractice : MonoBehaviour {

		[Header (("Task related refs"))]
		public GameObject theCube;
		public MessagePanelInVR messagePanelInVR;
		private Coroutine moveRoutine = null;


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
			ExperimentManager.Instance.EnableExperimentProceed (true); // enable proceed button

			messagePanelInVR.ShowMessage("Click button to continue");
		}

		/// <summary>Stop moving, change color</summary>
		public void TaskStep3 () {

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
#region EVENT HOOKS
			
		/// <summary>Called when block resumes</summary>
		public void OnBlockStart () {
			
		}

		public void OnResetTrial () {
			messagePanelInVR.HidePanel();
		}

		/// <summary>Called from Experiment manager</summary>
		public void OnSessionStart() {
			messagePanelInVR.ShowMessage("Welcome to the experiment, please click button to continue", true);
		}

		// If there is a BREAK in the experiment, these methods get called
		public void OnSessionBreak() {
			messagePanelInVR.ShowMessage("Take a short break, \nClick button to continue", true);
		}

		public void OnSessionEnd () {
			messagePanelInVR.ShowMessage("Session ended, logfiles saved");
		}

		/// <summary>Called when the block introduction starts</summary>
		public void OnSessionResume () {
			messagePanelInVR.ShowMessage("Resuming experiment", 2f);
		}

		/// <summary>Called when the block introduction starts</summary>
		public void OnBlockIntroduction() {
			messagePanelInVR.ShowMessage(ExperimentManager.Instance.experimentConfig.GetBlockIntroduction(), true);
		}

		/// <summary>Called when block resumes</summary>
		public void OnBlockResume () {

		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}

}