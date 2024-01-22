using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA.Events {


	public static class System {

		/// <summary>Set experiment config. Expects config as JSON string</summary>
		public const string EvCallMainMenu = "EvCallMainMenu";

	}

	//? ========================================================================================================

	/// <summary>Overview of all events in the system. Easier to reference and no typo mistakes by using them.</summary>
	public static class Core {

		/// <summary>Use this to alert the user that something went wrong</summary>
		public const string EvSystemHalt = "EvSystemHalt";

		/// <summary>Exit application</summary>
		public const string EvQuitApplication = "EvQuitApplication";

		/// <summary>Shows a message to the VR user</summary>
		public const string EvShowMessageToUser = "EvShowMessageToUser";

	}

	//? ========================================================================================================
	//? 

	/// <summary>All event related to controlling the state machine of the experiment </summary>
	public static class Settings {

		/// <summary>Set storagepath systemwide. Expects full path as string</summary>
		public const string EvSetCustomStoragePath = "EvSetCustomStoragePath";

		/// <summary>Request to show system settings. Expects null</summary>
		public const string EvRequestSystemSettings = "EvRequestSystemSettings";

		/// <summary>Open system settings panel. Expects full package of SettingsDeclaration as JSON</summary>
		public const string EvOpenSystemSettings = "EvOpenSystemSettings";

		/// <summary>SystemSettings have been updated. Expects full package of SettingsDeclaration as JSON</summary>
		public const string EvUpdateSystemSettings = "EvUpdateSystemSettings";

	}


	//? ========================================================================================================
	//? 

	/// <summary>All event related to controlling the state machine of the experiment </summary>
	public static class Config {

		/// <summary>Set session info. Expects JSON string</summary>
		public const string EvSetSessionInfo = "EvSetSessionInfo";

		// <summary>Set sessions block sequence. Expects JSON string</summary>
		public const string EvSetEBlockSequence = "EvSetEBlockSequence";

		/// <summary>Set sessions block sequence. Expects JSON string</summary>
		public const string EvSetTaskDefinitions = "EvSetTaskDefinitions";

		/// <summary>Set sessions block sequence. Expects JSON string</summary>
		public const string EvSetEBlockDefinitions = "EvSetEBlockDefinitions";

		/// <summary>Fired when both configs are set </summary>
		public const string EvReadyToGo = "EvReadyToGo";

		/// <summary>Notification that local config files are found on disk. Expects amount as int</summary>
		public const string EvFoundLocalConfigFiles = "EvFoundLocalConfigFiles";

		/// <summary>Local config file was submitted. Expects filename as string</summary>
		public const string EvLocalConfigSubmitted = "EvLocalConfigSubmitted";

	}



	//? ========================================================================================================
	//? 

	/// <summary>All event related to controlling the state machine of the experiment </summary>
	public static class StateMachine {

		/// <summary>Starts the experiment. Expects null</summary>
		public const string EvStartExperiment = "EvStartExperiment";

		/// <summary>Injects a break block after current trial. Expects null</summary>
		public const string EvPauseExperiment = "EvPauseExperiment";

		/// <summary>Fired by ExperimentManager when a trial has begun. Expects null</summary>
		public const string EvTrialBegin = "EvTrialBegin";

		/// <summary>Fired by ExperimentManager when the session had Finialized. Expects null</summary>
		public const string EvFinalizeSession = "EvFinalizeSession";

		/// <summary>Fired by ExperimentManager when the session starts a break. Expects null</summary>
		public const string EvSessionBreak = "EvSessionBreak";

		/// <summary>Fired by ExperimentManager when the session continues. Expects null</summary>
		public const string EvSessionResume = "EvSessionResume";

		/// <summary>Fired by ExperimentManager when a blockintroduction is found. Expects null</summary>
		public const string EvBlockIntroduction = "EvBlockIntroduction";

		/// <summary>Fired by ExperimentManager when the session resumes after an i.e. introduction. Expects null</summary>
		public const string EvBlockResumeAfterIntro = "EvBlockResumeAfterIntro";

		/// <summary>Event indicating that the system can proceed, useally from experimenter. Expects null</summary>
		public const string EvProceed = "EvProceed";

		/// <summary>Fired by ExperimentManager when a new block is starting. Expects null</summary>
		public const string EvBlockStart = "EvBlockStart";

	}


	//? ========================================================================================================
	//? Onscreen or inworld control panel methods

	/// <summary>All event related to local or remote control </summary>
	public static class ControlPanel {

		/// <summary>Set a buttons interactivity, expects string[ [PAUSE/PROCEED], [TRUE/FALSE] ]</summary>
		public const string EvEnableButton = "EvEnableButton";

		/// <summary>Start a visual timer animation</summary>
		public const string EvStartTimer = "EvStartTimer";

		/// <summary>Stops a visual timer animation</summary>
		public const string EvStopTimer = "EvStopTimer";

		/// <summary>Experiment summary as string[]</summary>
		public const string EvUpdateSessionSummary = "EvUpdateExperimentSummary";

		/// <summary>Send progress update (trial/block)</summary>
		public const string EvUpdateProgressStatus = "EvUpdateProgressStatus";

		/// <summary>Send progress update block, expects [currentblocknum, maxblocks]</summary>
		public const string EvUpdateBlockProgress = "EvUpdateBlockProgress";

		/// <summary>Send progress update trial, expects [currenttrialnum, maxtrials]</summary>
		public const string EvUpdateTrialProgress = "EvUpdateTrialProgress";

		/// <summary>Send progress update step, expects [currentstepnum, maxsteps]</summary>
		public const string EvUpdateStepProgress = "EvUpdateStepProgress";

		/// <summary>Shows message to experimenter canvas. Expects message as string, autohide as bool</summary>
		public const string EvShowMessageBox = "EvShowMessageBox";

		// Fired when mouse hovers over a GUI item that has 'tooltip' script on it. Expects null.
		public const string EvShowTooltip = "EvShowTooltip";

		// Fired when mouse hovers over a GUI item that has 'tooltip' script on it. Expects null.
		public const string EvHideTooltip = "EvHideTooltip";

		// Fired when pairing panel gets a connection. Expects int as HMD index
		public const string EvConnectionEstablished = "EvConnectionEstablished ";


		/// <summary>Sets the controlpanelmode. Exprects int. 0=hidden, 1=2Dcanvas, 2=3Dcanvas</summary>
		// public const string EvSetControlPanelMode = "EvSetControlPanelMode";


	}

	//? ========================================================================================================
	//? 

	/// <summary>All event related to controlling the state machine of the experiment </summary>
	public static class Network {

		// * TO APP >>

		public const string NwEvSetSessionInfo = "NwEvSetSessionInfo";

		public const string NwEvSetEBlockSequence = "NwEvSetEBlockSequence";

            public const string NwEvSetTaskDefinitions = "NwEvSetTaskDefinitions";

            public const string NwEvSetEBlockDefinitions = "NwEvSetEBlockDefinitions";

            public const string NwEvStartExperiment = "NwEvStartExperiment";

		public const string NwEvPauseExperiment = "NwEvPauseExperiment";

		public const string NwEvProceed = "NwEvProceed";

		public const string NwEvToggleCasting = "NwEvToggleCasting";


		// * TO MANAGER >>

		public const string NwEvReadyToGo = "NwEvReadyToGo";

		public const string NwEvEnableButton = "NwEvEnableButton";

		public const string NwEvUpdateStepProgress = "NwEvUpdateStepProgress";

		public const string NwEvUpdateTrialProgress = "NwEvUpdateTrialProgress";

		public const string NwEvUpdateBlockProgress = "NwEvUpdateBlockProgress";

		public const string NwEvUpdateSessionSummary = "NwEvUpdateSessionSummary";

		public const string NwEvUpdateProgressInfo = "NwEvUpdateProgressInfo";

		public const string NwEvStartTimer = "NwEvStartTimer";

		public const string NwEvStopTimer = "NwEvStopTimer";


		public const string NwEvEnableEyeCalibrationTrigger = "NwEvEnableEyeCalibrationTrigger";

	}



	//? ========================================================================================================
	//? Optional eye package methods

	public static class Eye {
		/// <summary>Whatever EYE package is used, it listens to this. Expects boolean</summary>
		public const string EvEnableEyeCalibrationTrigger = "EvEnableEyeCalibrationTrigger";

		/// <summary>Eye calibration request. Expects null</summary>
		public const string EvEyeCalibrationRequested = "EvEyeCalibrationRequested";
	}

	//? ========================================================================================================
	//? XR cam and controller related 

	public static class XR {

		/// <summary>The main interactor has changed. Expects a enum PrimaryInteractor as INT</summary>
		public const string EvUpdateInteractiveInteractor = "EvUpdateInteractiveInteractor";

		/// <summary>Which controllers are active in the application. Expects a enum AvailableController as INT</summary>
		public const string EvUpdateVisableInteractor = "EvUpdateVisableInteractor";

		/// <summary>Turn XR hand / controller interaction possibility on or off. Expects boolean</summary>
		public const string EvEnableXRInteraction = "EvEnableXRInteraction";

		/// <summary>Shows XR hand / controller on or off. Expects boolean</summary>
		public const string EvShowXRController = "EvShowXRController";

		/// <summary>System found XR hands and HMD objects. Expects null</summary>
		public const string EvFoundXRrigReferences = "EvFoundXRrigReferences";

		/// <summary>Enable interaction with UI presented on layer 'camoverlay'</summary>
		public const string EvEnableRayForCamOverlayer = "EvEnableRayForCamOverlayer";


		//? ========================================================================================================
		//? Hands

		/// <summary>Animate the handmodel is this pose, expects string 'idle' 'point' 'fist' ...</summary>
		public const string EvHandPose = "EvHandPose";

		/// <summary>Handmodel pose reacts live to controller state, expects bool</summary>
		public const string EvEnableCustomHandPoses = "EvEnableCustomHandPoses";

	}

	//? ========================================================================================================

	public static class DataHandlers {

		/// <summary>Send a marker to the system, any listener can pick it up and handle it. Expects marker as string</summary>
		public const string EvSendMarker = "EvSendMarker";

	}

	//? ========================================================================================================

	public static class Casting {

		/// <summary>Send a marker to the system, any listener can pick it up and handle it. Expects marker as string</summary>
		public const string EvToggleCasting = "EvToggleCasting";

	}



}



