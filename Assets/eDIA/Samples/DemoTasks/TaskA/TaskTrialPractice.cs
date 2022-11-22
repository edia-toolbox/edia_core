using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UXF;

namespace TASK {
	
	[System.Serializable]
	public class TastTrialPractice : MonoBehaviour {

		[Header (("Task related refs"))]
		public GameObject theCube;
		private Coroutine moveRoutine = null;


		/// <summary>Present Cube</summary>
		public void TaskStep1 () {

			EventManager.TriggerEvent (eDIA.Events.Interaction.EvHandPose, new eParam ("point"));

			ExperimentManager.Instance.EnableExperimentPause (true);
			XRrigUtilities.EnableXRInteraction (false);

			theCube.gameObject.SetActive (true);
			theCube.transform.position = new Vector3 (0, XRrigUtilities.GetXRcam().position.y, TaskManager.Instance.taskSettings.GetFloat ("distanceCube"));

			TaskManager.Instance.NextStep (TaskManager.Instance.taskSettings.GetFloat ("timerShowCube"));
		}

		/// <summary>Move cube, wait on user input</summary>
		public void TaskStep2 () {

			if (moveRoutine == null) {
				moveRoutine = StartCoroutine ("MoveCube");
			}

			XRrigUtilities.EnableXRInteraction (true);
			ExperimentManager.Instance.EnableExperimentProceed (true); // enable proceed button

			EventManager.TriggerEvent ("EvShowMessage", new eParam ("Click button to continue"));
		}

		/// <summary>Stop moving, change color</summary>
		public void TaskStep3 () {

			if (moveRoutine != null) {
				StopCoroutine (moveRoutine);
				moveRoutine = null;
			}

			Color newCol;
			if (ColorUtility.TryParseHtmlString (TaskManager.Instance.taskSettings.GetStringList ("cubeColors") [Session.instance.CurrentTrial.settings.GetInt ("color")], out newCol))
				theCube.GetComponent<MeshRenderer> ().material.color = newCol;
			else newCol = Color.magenta;

			EventManager.TriggerEvent (eDIA.Events.Interaction.EvHandPose, new eParam ("idle"));

			TaskManager.Instance.NextStep();
		}

		/// <summary>Wait</summary>
		public void TaskStep4 () {
			TaskManager.Instance.NextStep (TaskManager.Instance.taskSettings.GetFloat ("timerWait"));
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

}