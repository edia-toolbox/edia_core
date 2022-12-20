using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA.Events {


	public static class System {

		/// <summary>Set experiment config. Expects config as JSON string</summary>
		public static string EvCallMainMenu 		= "EvCallMainMenu";


	}

	//? ========================================================================================================

	/// <summary>Overview of all events in the system. Easier to reference and no typo mistakes by using them.</summary>
	public static class Core {



		/// <summary>Use this to alert the user that something went wrong</summary>
		public static string EvSystemHalt 			= "EvSystemHalt";

		/// <summary>Exit application</summary>
		public static string EvQuitApplication 		= "EvQuitApplication";

		/// <summary>Shows a message to the VR user</summary>
		public static string EvShowMessageToUser 		= "EvShowMessageToUser";

	}

	//? ========================================================================================================
	//? 

	/// <summary>All event related to controlling the state machine of the experiment </summary>
	public static class Settings {

		/// <summary>Set storagepath systemwide. Expects full path as string</summary>
		public static string EvSetCustomStoragePath 	= "EvSetCustomStoragePath";

		/// <summary>Request to show system settings. Expects null</summary>
		public static string EvRequestSystemSettings 	= "EvRequestSystemSettings";

		/// <summary>Open system settings panel. Expects full package of SettingsDeclaration as JSON</summary>
		public static string EvOpenSystemSettings 	= "EvOpenSystemSettings";

		/// <summary>SystemSettings have been updated. Expects full package of SettingsDeclaration as JSON</summary>
		public static string EvUpdateSystemSettings 	= "EvUpdateSystemSettings";


	}


	//? ========================================================================================================
	//? 

	/// <summary>All event related to controlling the state machine of the experiment </summary>
	public static class Config {

		/// <summary>Set experiment config. Expects config as JSON string</summary>
		public static string EvSetExperimentConfig 	= "EvSetExperimentConfig";

		/// <summary>Set task config. Expects config as JSON string</summary>
		public static string EvSetTaskConfig 		= "EvSetTaskConfig";

		/// <summary>Fired when the Experiment manager has set the experimentconfig. </summary>
		public static string EvExperimentConfigSet 	= "EvExperimentConfigSet";

		/// <summary>Fired when the Experiment manager has set the taskconfig. </summary>
		public static string EvTaskConfigSet 		= "EvTaskConfigSet";

		/// <summary>Fired when both configs are set </summary>
		public static string EvReadyToGo 			= "EvReadyToGo";

		/// <summary>Notification that local config files are found on disk. Expects amount as int</summary>
		public static string EvFoundLocalConfigFiles 	= "EvFoundLocalConfigFiles";

		/// <summary>Local config file was submitted. Expects filename as string</summary>
		public static string EvLocalConfigSubmitted 	= "EvLocalConfigSubmitted";


	}



	//? ========================================================================================================
	//? 

	/// <summary>All event related to controlling the state machine of the experiment </summary>
	public static class StateMachine {

		/// <summary>Starts the experiment. Expects null</summary>
		public static string EvStartExperiment 		= "EvStartExperiment";

		/// <summary>Injects a break block after current trial. Expects null</summary>
		public static string EvPauseExperiment 		= "EvPauseExperiment";

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
		public static string EvBlockResumeAfterIntro 	= "EvBlockResumeAfterIntro";

		/// <summary>Event indicating that the system can proceed, useally from experimenter. Expects null</summary>
		public static string EvProceed 			= "EvProceed";

		/// <summary>Fired by ExperimentManager when a new block is starting. Expects null</summary>
		public static string EvBlockStart 			= "EvBlockStart";

	}


	//? ========================================================================================================
	//? Onscreen or inworld control panel methods

	/// <summary>All event related to local or remote control </summary>
	public static class ControlPanel {
	
		/// <summary>Set a buttons interactivity, expects string[ [PAUSE/PROCEED], [TRUE/FALSE] ]</summary>
		public static string EvEnableButton 			= "EvEnableButton";

		/// <summary>Start a visual timer animation</summary>
		public static string EvStartTimer 				= "EvStartTimer";

		/// <summary>Stops a visual timer animation</summary>
		public static string EvStopTimer 				= "EvStopTimer";

		/// <summary>Experiment summary as string[]</summary>
		public static string EvUpdateSessionSummary 		= "EvUpdateExperimentSummary";

		/// <summary>Send progress update (trial/block)</summary>
		public static string EvUpdateProgressInfo 	= "EvUpdateProgressDescription";

		/// <summary>Send progress update block, expects [currentblocknum, maxblocks]</summary>
		public static string EvUpdateBlockProgress 		= "EvUpdateBlockProgress";

		/// <summary>Send progress update trial, expects [currenttrialnum, maxtrials]</summary>
		public static string EvUpdateTrialProgress 		= "EvUpdateTrialProgress";

		/// <summary>Send progress update step, expects [currentstepnum, maxsteps]</summary>
		public static string EvUpdateStepProgress 		= "EvUpdateStepProgress";

		/// <summary>Shows message to experimenter canvas. Expects message as string, autohide as bool</summary>
		public static string EvShowMessageBox 			= "EvShowMessageBox";

		// Fired when mouse hovers over a GUI item that has 'tooltip' script on it. Expects null.
		public static string EvShowTooltip 				= "EvShowTooltip";

		// Fired when mouse hovers over a GUI item that has 'tooltip' script on it. Expects null.
		public static string EvHideTooltip 				= "EvHideTooltip";

		/// <summary>Sets the controlpanelmode. Exprects int. 0=hidden, 1=2Dcanvas, 2=3Dcanvas</summary>
		// public static string EvSetControlPanelMode = "EvSetControlPanelMode";



	}

	//? ========================================================================================================
	//? 

	/// <summary>All event related to controlling the state machine of the experiment </summary>
	public static class Network {

		// * TO APP >>

		public static string NwEvSetTaskConfig 		= "NwEvSetTaskConfig";

		public static string NwEvSetExpConfig 		= "NwEvSetExpConfig";

		public static string NwEvStartExperiment 		= "NwEvStartExperiment";

		public static string NwEvPauseExperiment 		= "NwEvPauseExperiment";

		public static string NwEvProceed 			= "NwEvProceed";

		public static string NwEvEnableCasting 		= "NwEvEnableCasting";


		// * TO MANAGER >>

		public static string NwEvTaskConfigSet 		= "NwEvTaskConfigSet";

		public static string NwEvExperimentConfigSet 	= "NwEvExperimentConfigSet";

		public static string NwEvReadyToGo 			= "NwEvReadyToGo";

		public static string NwEvEnableButton 		= "NwEvEnableButton";


		public static string NwEvUpdateStepProgress 		= "NwEvUpdateStepProgress";

		public static string NwEvUpdateTrialProgress 		= "NwEvUpdateTrialProgress";

		public static string NwEvUpdateBlockProgress 		= "NwEvUpdateBlockProgress";

		public static string NwEvUpdateSessionSummary 		= "NwEvUpdateSessionSummary";

		public static string NwEvUpdateProgressInfo 		= "NwEvUpdateProgressInfo";

		public static string NwEvStartTimer 			= "NwEvStartTimer";

		public static string NwEvStopTimer 				= "NwEvStopTimer";


		public static string NwEvEnableEyeCalibrationTrigger 	= "NwEvEnableEyeCalibrationTrigger";

	}



	//? ========================================================================================================
	//? Optional eye package methods

	public static class Eye {
		/// <summary>Whatever EYE package is used, it listens to this. Expects boolean</summary>
		public static string EvEnableEyeCalibrationTrigger 	= "EvEnableEyeCalibrationTrigger";

		/// <summary>Eye calibration request. Expects null</summary>
		public static string EvEyeCalibrationRequested 		= "EvEyeCalibrationRequested";
	}

	//? ========================================================================================================
	//? XR cam and controller related 

	public static class XR {

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

	//? ========================================================================================================
	//? Hands

		/// <summary>Animate the handmodel is this pose, expects string 'idle' 'point' 'fist' ...</summary>
		public static string EvHandPose				= "EvHandPose";

		/// <summary>Handmodel pose reacts live to controller state, expects bool</summary>
		public static string EvEnableCustomHandPoses		= "EvEnableCustomHandPoses";

	}

	//? ========================================================================================================
	
	public static class DataHandlers {

		/// <summary>Send a marker to the system, any listener can pick it up and handle it. Expects marker as string</summary>
		public static string EvSendMarker 				= "EvSendMarker";

	}

	//? ========================================================================================================
	
	public static class Casting {

		/// <summary>Send a marker to the system, any listener can pick it up and handle it. Expects marker as string</summary>
		public static string EvEnableCasting 			= "EvEnableCasting";

	}



}



