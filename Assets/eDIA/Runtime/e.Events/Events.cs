using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA.Events {

	/// <summary>Overview of all events in the system. Easier to reference and no typo mistakes by using them.</summary>
	public static class Core {
		
		/// <summary>Use this to alert the user that something went wrong</summary>
		public static string EvSystemHalt = "EvSystemHalt";

		/// <summary></summary>
		public static string EvSetExperimentConfig = "EvSetExperimentConfig";

	}
}

/*

EvSetExperimentConfig
EvStartExperiment
EvPauseExperiment
EvFoundLocalConfigFiles
EvLocalConfigSubmitted

EvExperimentInitialised
EvSetDisplayInformation
EvExperimentInfoUpdate
EvTrialBegin
EvButtonChangeState
EvEnableEyeCalibrationTrigger
EvFinalizeSession
EvSessionBreak
EvBlockResume
EvSendMarker




*/

