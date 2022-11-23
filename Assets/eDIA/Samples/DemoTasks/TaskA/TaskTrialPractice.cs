using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UXF;
 
namespace TASK {
	
	[System.Serializable]
	public class TaskTrialPractice : MonoBehaviour {

		[Header (("Task related refs"))]
		public GameObject theCube;
		private Coroutine moveRoutine = null;


		/// <summary>Present Cube</summary>
		public void TaskStep1 () {

			EventManager.TriggerEvent (eDIA.Events.Interaction.EvHandPose, new eParam ("point"));

			ExperimentManager.Instance.EnableExperimentPause (true);
			XRrigUtilities.EnableXRInteraction (false);

			Debug.Log(Session.instance.currentBlockNum);
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

			EventManager.TriggerEvent ("EvShowMessage", new eParam ("Click button to continue"));
		}

		/// <summary>Stop moving, change color</summary>
		public void TaskStep3 () {

			if (moveRoutine != null) {
				StopCoroutine (moveRoutine);
				moveRoutine = null;
			}

			Debug.Log(Session.instance.CurrentBlock.settings.GetString("cubeColors"));

			// Dictionary<string, object> bla = Session.instance.CurrentBlock.settings.GetDict("cubeColors");

			Session.instance.CurrentBlock.settings.GetStringList ("cubeColors");

			Color newCol;
			if (ColorUtility.TryParseHtmlString (Session.instance.CurrentBlock.settings.GetStringList ("cubeColors") [Session.instance.CurrentTrial.settings.GetInt ("color")], out newCol))
				theCube.GetComponent<MeshRenderer> ().material.color = newCol;
			else newCol = Color.magenta;

			EventManager.TriggerEvent (eDIA.Events.Interaction.EvHandPose, new eParam ("idle"));

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
	}

}