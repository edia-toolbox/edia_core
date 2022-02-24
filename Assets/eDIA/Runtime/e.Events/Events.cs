using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA.Events {

	/// <summary>Overview of all events in the system. Easier to reference and no typo mistakes by using them.</summary>
	public static class Core {
		
		/// <summary>Use this to alert the user that something went wrong</summary>
		public static string EvSystemHalt 			= "EvSystemHalt";

		/// <summary>Set experiment config. Expects config as JSON string</summary>
		public static string EvSetExperimentConfig 	= "EvSetExperimentConfig";

		/// <summary>Starts the experiment. Expects null</summary>
		public static string EvStartExperiment 		= "EvStartExperiment";

		/// <summary>Injects a break block after current trial. Expects null</summary>
		public static string EvPauseExperiment 		= "EvPauseExperiment";

		/// <summary>Notification that local config files are found on disk. Expects amount as int</summary>
		public static string EvFoundLocalConfigFiles 	= "EvFoundLocalConfigFiles";

		/// <summary>Local config file was submitted. Expects filename as string</summary>
		public static string EvLocalConfigSubmitted 	= "EvLocalConfigSubmitted";




		/// <summary>Open system settings panel</summary>
		public static string EvOpenSystemSettings 	= "EvOpenSystemSettings";

		/// <summary>SystemSettings have been updated, apply them</summary>
		public static string EvUpdateSystemSettings 	= "EvUpdateSystemSettings";


	}


	//? ========================================================================================================
	
	public class Interaction {

		/// <summary>SystemSettings have been updated, apply them</summary>
		public static string EvUpdatePrimaryInteractor 	= "EvUpdatePrimaryInteractor";
 
	}
}



/*

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

-----


EvSetExperimentConfig
EvStartExperiment
EvPauseExperiment
EvFoundLocalConfigFiles
EvLocalConfigSubmitted

*/

