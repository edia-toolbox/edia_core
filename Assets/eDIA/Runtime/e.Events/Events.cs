using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA.Events {

	/// <summary>Overview of all events in the system. Easier to reference and no typo mistakes by using them.</summary>
	public static class Core {
		


		/// <summary>Set experiment config. Expects config as JSON string</summary>
		public static string EvSetExperimentConfig 	= "EvSetExperimentConfig";

		/// <summary>Starts the experiment. Expects null</summary>
		public static string EvStartExperiment 		= "EvStartExperiment";

		/// <summary>Injects a break block after current trial. Expects null</summary>
		public static string EvPauseExperiment 		= "EvPauseExperiment";

		/// <summary>Fired when the Experiment manager has initiataled OK. Expects result as bool. </summary>
		public static string EvExperimentInitialised 	= "EvExperimentInitialised";

		/// <summary>Fired by ExperimentManager when a trial has begun. Expects null</summary>
		public static string EvTrialBegin 			= "EvTrialBegin";

		/// <summary>Fired by ExperimentManager when the session had Finialized. Expects null</summary>
		public static string EvFinalizeSession 		= "EvFinalizeSession";

		/// <summary>Fired by ExperimentManager when the session starts a break. Expects null</summary>
		public static string EvSessionBreak 		= "EvSessionBreak";

		/// <summary>Fired by ExperimentManager when the session continues. Expects null</summary>
		public static string EvSessionResume 		= "EvSessionResume";

		/// <summary>Fired by ExperimentManager when a blockintroduction is found. Expects null</summary>
		public static string EvBlockIntroduction 		= "EvBlockIntroduction";

		/// <summary>Fired by ExperimentManager when the session resumes after an i.e. introduction. Expects null</summary>
		public static string EvBlockResume 			= "EvBlockResume";

		/// <summary>Event indicating that the system can proceed, useally from experimenter. Expects null</summary>
		public static string EvProceed 			= "EvProceed";




		/// <summary>Notification that local config files are found on disk. Expects amount as int</summary>
		public static string EvFoundLocalConfigFiles 	= "EvFoundLocalConfigFiles";

		/// <summary>Local config file was submitted. Expects filename as string</summary>
		public static string EvLocalConfigSubmitted 	= "EvLocalConfigSubmitted";


		/// <summary>Use this to alert the user that something went wrong</summary>
		public static string EvSystemHalt 			= "EvSystemHalt";

		/// <summary>Request to show system settings. Expects null</summary>
		public static string EvRequestSystemSettings 	= "EvRequestSystemSettings";

		/// <summary>Open system settings panel. Expects full package of SettingsDeclaration as JSON</summary>
		public static string EvOpenSystemSettings 	= "EvOpenSystemSettings";

		/// <summary>SystemSettings have been updated. Expects full package of SettingsDeclaration as JSON</summary>
		public static string EvUpdateSystemSettings 	= "EvUpdateSystemSettings";


		/// <summary>Broaccast a update in the experiment, useally to show to the user</summary>
		public static string EvUpdateExperimentInfoToUser 	= "EvUpdateExperimentInfoToUser";


	}

	//? ========================================================================================================
	
	public class Interaction {

		/// <summary>The main interactor has changed. Expects a enum PrimaryInteractor as INT</summary>
		public static string EvUpdateInteractiveInteractor 	= "EvUpdateInteractiveInteractor";
 
		/// <summary>Which controllers are active in the application. Expects a enum AvailableController as INT</summary>
		public static string EvUpdateVisableInteractor 		= "EvUpdateVisableInteractor";
 
 		/// <summary>Turn XR hand / controller interaction possibility on or off. Expects boolean</summary>
		public static string EvEnableXRInteraction		= "EvEnableXRInteraction";

 		/// <summary>Shows XR hand / controller on or off. Expects boolean</summary>
		public static string EvShowXRController			= "EvShowXRController";

 		/// <summary>System found XR hands and HMD objects. Expects null</summary>
		public static string EvFoundXRrigReferences		= "EvFoundXRrigReferences";
	}

	//? ========================================================================================================
	
	public class DataHandlers {

		/// <summary>Send a marker to the system, any listener can pick it up and handle it. Expects marker as string</summary>
		public static string EvSendMarker 				= "EvSendMarker";
 

	}



}



/*



EvButtonChangeState
EvSetDisplayInformation
EvExperimentInfoUpdate
EvEnableEyeCalibrationTrigger
-----


EvSetExperimentConfig
EvStartExperiment
EvPauseExperiment
EvFoundLocalConfigFiles
EvLocalConfigSubmitted

*/

