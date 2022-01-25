using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA {

	public class EyeCalibrationTrigger : MonoBehaviour {

		void Start () {
			EventManager.StartListening("EvEyeCalibrationRequested", OnEvEyeCalibrationRequested);
		}

		void OnDestroy() {
			EventManager.StopListening("EvEyeCalibrationRequested", OnEvEyeCalibrationRequested);
		}

		void OnEvEyeCalibrationRequested (eParam e) {
			Debug.Log("Eye calibration called");
			// Call eye calibration on the system

			// VIVE
			// ViveSR.anipal.Eye.SRanipal_Eye.LaunchEyeCalibration();

			// PICO


			// FOCUS

		}
	}
}