using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UXF;

	[System.Serializable]
	public class TaskTrialPractice : MonoBehaviour {

		[Header (("Task related refs"))]
		public GameObject theCube;
		private Coroutine moveRoutine = null;

		/// <summary>Present Cube</summary>
		void TaskStep1 () {

			EventManager.TriggerEvent (eDIA.Events.Interaction.EvHandPose, new eParam ("point"));

			ExperimentManager.Instance.EnableExperimentPause (true);
			XRrigUtilities.EnableXRInteraction (false);

			theCube.gameObject.SetActive (true);
			theCube.transform.position = new Vector3 (0, XRrigUtilities.GetXRcam().position.y, TaskManager.Instance.taskSettings.GetFloat ("distanceCube"));

			Invoke ("NextStepFromUserOrSceneOrButtonOrTimerOrWhatever", TaskManager.Instance.taskSettings.GetFloat ("timerShowCube"));
		}

		/// <summary>Move cube, wait on user input</summary>
		void TaskStep2 () {

			if (moveRoutine == null) {
				moveRoutine = StartCoroutine ("MoveCube");
			}

			XRrigUtilities.EnableXRInteraction (true);
			ExperimentManager.Instance.EnableExperimentProceed (true); // enable proceed button
			// EventManager.StartListening (eDIA.Events.Core.EvProceed, OnEvProceed); //! Continues to the next step
			EventManager.TriggerEvent ("EvShowMessage", new eParam ("Click button to continue"));
		}

		/// <summary>Stop moving, change color</summary>
		void TaskStep3 () {

			if (moveRoutine != null) {
				StopCoroutine (moveRoutine);
				moveRoutine = null;
			}

			Color newCol;
			if (ColorUtility.TryParseHtmlString (TaskManager.Instance.taskSettings.GetStringList ("cubeColors") [Session.instance.CurrentTrial.settings.GetInt ("color")], out newCol))
				theCube.GetComponent<MeshRenderer> ().material.color = newCol;
			else newCol = Color.magenta;

			EventManager.TriggerEvent (eDIA.Events.Interaction.EvHandPose, new eParam ("idle"));

			NextStepFromUserOrSceneOrButtonOrTimerOrWhatever ();
		}

		/// <summary>Wait</summary>
		void TaskStep4 () {

			Invoke ("NextStepFromUserOrSceneOrButtonOrTimerOrWhatever", TaskManager.Instance.taskSettings.GetFloat ("timerWait"));
		}

		/// <summary>Call this from your code to proceed to the next step</summary>
		public void NextStepFromUserOrSceneOrButtonOrTimerOrWhatever () {
			TaskManager.Instance.NextStep ();
		}

		/// <summary>Moves the cube up or down depending on the setting `direction` in the trial settings.</summary>
		IEnumerator MoveCube () {
			float increment = Session.instance.CurrentTrial.settings.GetInt ("direction") == 1 ? 0.001f : -0.001f;

			while (true) {
				theCube.transform.Translate (new Vector3 (0, increment, 0), Space.World);
				yield return new WaitForEndOfFrame ();
			}
		}
	}