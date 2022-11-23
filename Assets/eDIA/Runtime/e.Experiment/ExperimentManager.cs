using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UXF;

// EXPERIMENT CONTROL 
namespace eDIA {

#region DECLARATIONS

	public class ExperimentManager : Singleton<ExperimentManager> {

		// ---
		[Header("Editor Settings")]
		public bool showLog = false;
		public Color taskColor = Color.green;

		/// <summary> Tuple of strings, using this as this is serializable in the inspector and dictionaries are not</summary>
		[System.Serializable]
		public class SettingsTuple {
			[HideInInspector]
			public string key 	= string.Empty;
			public string value 	= string.Empty;
		}

		[System.Serializable]
		public class ExperimentBlock {
			public string 				name				= string.Empty;
			public string 				introduction 		= string.Empty;
			public List<SettingsTuple>		blockSettings		= new List<SettingsTuple>();
			public List<string> 			trialKeys 			= new List<string>();
			public List<TrialChainValues> 	trialChain 			= new List<TrialChainValues>();
		}


		/// <summary> Temp storage of a list of string to use in another class to generate a two dimensional array basically</summary>
		[System.Serializable]
		public class TrialChainValues {
			public List<string> values = new List<string>();
		}

		/// <summary> container for (de)serializing a list to JSON</summary>
		public class TrialChainValuesContainer {
			public List<TrialChainValues> trialSequenceValues = new List<TrialChainValues>();
		}

		/// <summary> Main container to store sessions config in, either from disk, editor or network </summary>
		[System.Serializable]
		public class ExperimentConfig {
			public string				experiment			= string.Empty;
			public string 				experimenter 		= string.Empty;
			public string 				participantID 		= string.Empty;
			public int 					sessionNumber 		= 0;
			public List<SettingsTuple>		participantInfo 		= new List<SettingsTuple>();
			public List<SettingsTuple>		sessionSettings 		= new List<SettingsTuple>();
			public List<int>				breakAfter			= new List<int>(); 
			public List<ExperimentBlock>		blocks			= new List<ExperimentBlock>();


			public string GetBlockIntroduction () {

				return blocks[Session.instance.currentBlockNum-1].introduction == string.Empty ? string.Empty : blocks[Session.instance.currentBlockNum-1].introduction;
			}

			public string[] GetExperimentSummary() {
				return new string[] { experiment, experimenter, participantID, sessionNumber.ToString() };
			}
		}	

		/// The config instance that holds current experimental configuration
		// [HideInInspector]
		public ExperimentConfig experimentConfig;
		[HideInInspector]
		public bool experimentInitialized = false;

		// Helpers
		[Space(20)]
		public int activeBlockUXF = 0;
		bool isPauseRequested = false;

		// UXF related
		Dictionary<string, object> participantDetails   = new Dictionary<string, object>();
		UXF.Settings currentUXFSessionSettings = new Settings();

		// UXF Logging
		UXF.UXFDataTable executionOrderLog = new UXF.UXFDataTable("timestamp", "executed"); 
		UXF.UXFDataTable markerLog = new UXF.UXFDataTable("timestamp", "annotation"); 

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region MONO METHODS

		void Awake() {
			EventManager.StartListening("EvFoundLocalConfigFiles", OnEvFoundLocalConfigFiles);
			EventManager.StartListening("EvSetExperimentConfig", OnEvSetExperimentConfig);
			EventManager.StartListening("EvStartExperiment", OnEvStartExperiment);
			EventManager.StartListening(eDIA.Events.Core.EvQuitApplication, OnEvQuitApplication);
		}



		void OnDestroy() {
			EventManager.StopListening("EvSetExperimentConfig", OnEvSetExperimentConfig);
			EventManager.StopListening("EvStartExperiment", OnEvStartExperiment);
			EventManager.StopListening("EvPauseExperiment", OnEvPauseExperiment);
			EventManager.StopListening("EvFoundLocalConfigFiles", OnEvFoundLocalConfigFiles);
			EventManager.StopListening(eDIA.Events.Core.EvQuitApplication, OnEvQuitApplication);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EXPERIMENT CONFIG

		/// <summary>Register local mode, listen to submission of config file</summary>
		void OnEvFoundLocalConfigFiles (eParam e) {
			EventManager.StopListening("EvFoundLocalConfigFiles", OnEvFoundLocalConfigFiles);		

			AddToLog(e.GetInt() + " local config files added");
				
			EventManager.StartListening(eDIA.Events.Core.EvLocalConfigSubmitted, OnEvLocalConfigSubmitted);
		}

		/// <summary>Look up given index in the localConfigFiles list and give content of that file to system </summary>
		/// <param name="e">String = filename of the configfile</param>
		void OnEvLocalConfigSubmitted (eParam e) {
			EventManager.StopListening("EvLocalConfigSubmitted", OnEvLocalConfigSubmitted);
			string filename = e.GetStrings()[0] + "_" + e.GetStrings()[1] + ".json"; // combine task string and participant string
			
			SetExperimentConfig (LoadExperimentConfigFromDisk(filename));
		}

		/// <summary> Eventlistener which expects the config as JSON file, triggers default config file load if not. </summary>
		/// <param name="e">JSON config as string</param>
		void OnEvSetExperimentConfig( eParam e) {
			if (e == null) {
				EventManager.TriggerEvent(eDIA.Events.Core.EvSystemHalt, new eParam("No JSON config received!"));
				return;
			}

			SetExperimentConfig( e.GetString() );
		}

		/// <summary>Load JSON configuration locally from configdirectory</summary>
		/// <returns>JSON string</returns>
		string LoadExperimentConfigFromDisk (string fileName) {
			string experimentJSON = FileManager.ReadStringFromApplicationPathSubfolder(eDIA.Constants.localConfigDirectoryName + "/Participants", fileName);

			if (experimentJSON == "ERROR")
				Debug.LogError("Experiment JSON not correctly loaded!");
			
			return experimentJSON;
		}

		/// <summary>Set the eDIA experiment settings with the full JSON config string</summary>
		/// <param name="JSONstring">Full config string</param>
		void SetExperimentConfig (string JSONstring) {
			experimentConfig = UnityEngine.JsonUtility.FromJson<ExperimentConfig>(JSONstring);

			try
			{
				SetSessionSettings ();
				SetParticipantDetails ();
				SetTrialSequence ();

				experimentInitialized = true;
			}
			catch (System.Exception)
			{
				Debug.Log("Init not ok!");
				throw;
			}

			AddToLog("ExperimentInitialized " + experimentInitialized);
			EventManager.TriggerEvent("EvExperimentInitialised", new eParam(experimentInitialized));
			EventManager.TriggerEvent("EvSetDisplayInformation", new eParam( experimentConfig.GetExperimentSummary()) );

		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region SESSION SETTINGS

		/// <summary> Set the sessionsettings to use by UXF</summary>
		void SetSessionSettings () {
			// Add experimenter to settings
			currentUXFSessionSettings.SetValue("experimenter", experimentConfig.experimenter);

			// Convert and add KEYS and VALUES to the UXF settings in order to be logged
			// TrialSequenceValuesContainer trialSequenceValuesContainer = new TrialSequenceValuesContainer();
			// trialSequenceValuesContainer.trialSequenceValues.AddRange(experimentConfig.trialSequenceValues);
			// currentUXFSessionSettings.SetValue("trialSequenceKeys", experimentConfig.trialSequenceKeys);
			// currentUXFSessionSettings.SetValue("trialSequenceValues", UnityEngine.JsonUtility.ToJson(trialSequenceValuesContainer, false));

			// Are there default settings?
			if (experimentConfig.sessionSettings.Count == 0)
				return;
			
			// Add to UXF settings
			foreach(ExperimentManager.SettingsTuple tuple in experimentConfig.sessionSettings) {
				
				if (tuple.value.Contains(',')) { // it's a list!
					List<string> stringlist = tuple.value.Split(',').ToList();
					currentUXFSessionSettings.SetValue(tuple.key, stringlist);	
				} else currentUXFSessionSettings.SetValue(tuple.key, tuple.value);	// normal string
			}
		}

		/// <summary> Set the participant details to use by UXF, if any</summary>
		void SetParticipantDetails () {
			if (experimentConfig.participantInfo.Count == 0)
				return;

			for (int i=0;i<experimentConfig.participantInfo.Count; i++) {
				participantDetails.Add(experimentConfig.participantInfo[i].key, experimentConfig.participantInfo[i].value);
			}
		}

		void SetTrialSequence () {
			GenerateUXFSequence(); // Generate sequence for UXF
		}

		void OnEvStartExperiment (eParam e) {
			EventManager.StopListening("EvStartExperiment", OnEvStartExperiment);
			EventManager.StartListening("EvPauseExperiment", OnEvPauseExperiment);

			StartExperiment ();
		}

		/// <summary>Starts the experiment</summary>
		public void StartExperiment () {
			Session.instance.Begin( 
				experimentConfig.experiment 		== string.Empty ? "N.A." : experimentConfig.experiment,  
				experimentConfig.participantID 	== string.Empty ? "N.A." : experimentConfig.participantID, 
				experimentConfig.sessionNumber, 
				participantDetails, 
				currentUXFSessionSettings
			); 
		}

		void OnEvNewSession (eParam e) {
			// SetExperimentConfig(null);
			EventManager.TriggerEvent("EvSetExperimentConfig", null);
		}

		/// <summary>Sets the PauseExperiment flag to true and logs the call for an extra break</summary>
		void OnEvPauseExperiment(eParam e)
		{
			AddToLog("InjectedSessionBreakCall");
			AddToExecutionOrderLog("InjectedSessionBreakCall");
			isPauseRequested = true;
		}

		private void OnEvQuitApplication(eParam obj)
		{
			AddToLog("Quiting..");
			Application.Quit();
;		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region UXF EVENT HANDLERS

		/// <summary>Start of the UXF session. </summary>
		void OnSessionBeginUXF() {
			AddToLog("OnSessionBeginUXF");
			AddToExecutionOrderLog("OnSessionBegin");
			EventManager.StartListening(eDIA.Events.Core.EvProceed, OnEvStartFirstTrial);

			EventManager.TriggerEvent("EvExperimentProgressUpdate", new eParam("Welcome"));
			// 
			EventManager.TriggerEvent("EvButtonChangeState", new eParam( new string[] { "PROCEED", "true" }));

			// eye calibration option enabled
			EnableEyeCalibrationTrigger(true);
		}

		/// <summary>Called from user input. </summary>
		void OnEvStartFirstTrial (eParam e) {
			EventManager.StopListening(eDIA.Events.Core.EvProceed, OnEvStartFirstTrial);
			Session.instance.Invoke("BeginNextTrial", 0.5f);
		}

		/// <summary>Called from UXF session. </summary>
		void OnSessionEndUXF() {
			AddToLog("OnSessionEndUXF");
			AddToExecutionOrderLog("OnSessionEndUXF");

			EventManager.TriggerEvent("EvExperimentProgressUpdate", new eParam("End"));
			
			EnableExperimentProceed(false);
			EnableExperimentPause(false);
		}

		/// <summary>Called from UXF session. Begin setting things up for the trial that is about to start </summary>
		void OnTrialBeginUXF(Trial newTrial) {
			AddToLog("OnTrialBeginUXF");
			AddToExecutionOrderLog("OnTrialBegin");

			// EventManager.TriggerEvent("EvExperimentProgressUpdate", null); // update sliders in GUI
			
			bool showIntroduction = false;

			if ((Session.instance.currentBlockNum != activeBlockUXF) && (Session.instance.currentBlockNum <= Session.instance.blocks.Count)) {

				//! NEW BLOCK				
				EventManager.TriggerEvent(eDIA.Events.Core.EvBlockStart, null);

				// Check for block introduction flag
				showIntroduction = experimentConfig.GetBlockIntroduction() != string.Empty;
				// Set new activeBlockUXF value
				activeBlockUXF = Session.instance.currentBlockNum;
			}

			// Inject introduction step or continue UXF sequence
			if (showIntroduction)
				BlockIntroduction ();
			else {
				EventManager.TriggerEvent("EvTrialBegin", null);
				EventManager.TriggerEvent("EvExperimentProgressUpdate", new eParam(experimentConfig.blocks[Session.instance.currentBlockNum-1].name));
			}
		}

		/// <summary>Called from UXF session. Checks if to call NextTrial, should start a BREAK before next Block, or End the Session </summary>
		void OnTrialEndUXF(Trial endedTrial) {
			AddToLog("OnTrialEndUXF");
			AddToExecutionOrderLog("OnTrialEnd");
			SaveCustomDataTables();

			// Are we ending?
			if (Session.instance.isEnding)
				return;
			
			// Is there a PAUSE requested right now?
			if (isPauseRequested) {
				isPauseRequested = false;
				
				if (endedTrial == Session.instance.LastTrial)
					return;

				AddToExecutionOrderLog("Injected SessionBreak");
				SessionBreak();
				return;
			}

			// Reached last trial in a block?
			if (Session.instance.CurrentBlock.lastTrial != endedTrial) {
				// TODO Insert block check here
				Session.instance.BeginNextTrialSafe();
				return;
			}

			// Is this then the last trial of the session?
			if (Session.instance.LastTrial == endedTrial) {
				AddToLog("Reached end of trials ");
				FinalizeSession();
				return;
			}

			// Do we take a break or jump to next block?
			if (experimentConfig.breakAfter.Contains(Session.instance.currentBlockNum)) {
				SessionBreak();
				return;
			}
			
			// If we reach here it's just a normal trial and we continue
			Session.instance.BeginNextTrialSafe();
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region STATE MACHINE HELPERS

		public void EnableExperimentPause(bool _onOff) {
			EventManager.TriggerEvent("EvButtonChangeState", new eParam( new string[] { "PAUSE", _onOff.ToString() }));
		}

		public void EnableExperimentProceed(bool _onOff) {
			EventManager.TriggerEvent("EvButtonChangeState", new eParam( new string[] { "PROCEED", _onOff.ToString() }));

			if (_onOff)
				EventManager.StartListening (eDIA.Events.Core.EvProceed, TaskManager.Instance.OnEvProceed);
		}

		/// <summary> Set system open for calibration call from event or button</summary>
		/// <param name="onOff"></param>
		public void EnableEyeCalibrationTrigger (bool _onOff) {
			EventManager.TriggerEvent(eDIA.Events.Eye.EvEnableEyeCalibrationTrigger, new eParam(_onOff));
		}

		/// <summary>Done with all trial, clean up and call UXF to end this session</summary>
		void FinalizeSession ()
		{
			AddToLog("FinalizeSession");
			
			// clean
			EventManager.TriggerEvent("EvFinalizeSession", null);
			EventManager.TriggerEvent("EvExperimentProgressUpdate", new eParam("Finalize Session"));
			Session.instance.End();
		}

		private void SaveCustomDataTables()
		{
			// AddToLog("Savedatatable");
			UXF.Session.instance.SaveDataTable(executionOrderLog, "executionOrder");
			UXF.Session.instance.SaveDataTable(markerLog, "markerLog");
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BREAK

		/// <summary>Called from this manager. Invokes onSessionBreak event and starts listener to EvProceed event</summary>
		void SessionBreak () {
			AddToLog("SessionBreak");
			AddToExecutionOrderLog("SessionBreak");
			EventManager.StartListening(eDIA.Events.Core.EvProceed, SessionResume);
			EventManager.TriggerEvent(eDIA.Events.Core.EvSessionBreak, null);
				
			EventManager.TriggerEvent("EvExperimentProgressUpdate", new eParam("Break"));

			EnableExperimentProceed(true);
			EnableExperimentPause(false);
			EnableEyeCalibrationTrigger(true);
		}

		/// <summary>Called from EvProceed event. Stops listener, invokes onSessionResume event and calls UXF BeginNextTrial. </summary>
		void SessionResume (eParam e) {
			AddToExecutionOrderLog("SessionResume");
			AddToLog("SessionResume");
			EventManager.StopListening(eDIA.Events.Core.EvProceed, SessionResume);
			EventManager.TriggerEvent(eDIA.Events.Core.EvSessionResume, null);

			EventManager.TriggerEvent("EvExperimentProgressUpdate", new eParam("Block"));

			EnableEyeCalibrationTrigger(false);

			Session.instance.Invoke("BeginNextTrialSafe", 0.5f);
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BLOCK INTRODUCTION

		/// <summary>Called from this manager. </summary>
		void BlockIntroduction () {
			AddToLog("BlockIntroduction");
			AddToExecutionOrderLog("BlockIntroduction");
			EventManager.StartListening(eDIA.Events.Core.EvProceed, BlockResume);

			EventManager.TriggerEvent("EvExperimentProgressUpdate", new eParam("Introduction"));

			EnableExperimentProceed(true);
			EnableExperimentPause(false);
			EnableEyeCalibrationTrigger(true);

			EventManager.TriggerEvent("EvBlockIntroduction", null);
		}

		/// <summary>Called from this manager. </summary>
		void BlockResume (eParam e) {
			AddToLog("BlockResume");
			AddToExecutionOrderLog("BlockResume");
			EventManager.StopListening(eDIA.Events.Core.EvProceed, BlockResume);

			EnableEyeCalibrationTrigger(false);

			EventManager.TriggerEvent(eDIA.Events.Core.OnEvBlockResumeAfterIntro,null);
		}

#endregion	// -------------------------------------------------------------------------------------------------------------------------------
#region JSON TO UXF CONVERSION

		/// <summary>/// Convert JSON formatted definition for the seqence into a UXF format to run in the session/// </summary>
		public void GenerateUXFSequence() {

			foreach (ExperimentBlock b in experimentConfig.blocks) {
				
				Block newBlock = Session.instance.CreateBlock();
				newBlock.settings.SetValue("name",b.name);
				newBlock.settings.SetValue("introduction",b.introduction);

				// Assign blocksettings to this UXF block
				
				foreach (SettingsTuple s in b.blockSettings)
					if (s.value.Contains(',')) { // it's a list!
						List<string> stringlist = s.value.Split(',').ToList();
						newBlock.settings.SetValue(s.key, stringlist);	
					} else newBlock.settings.SetValue(s.key, s.value);	// normal string

				foreach (TrialChainValues row in b.trialChain) {
					Trial newTrial = newBlock.CreateTrial();

					for (int i = 0; i < b.trialKeys.Count; i++) {
						newTrial.settings.SetValue(b.trialKeys[i], row.values[i].ToUpper()); // set values to trial
					}
				}
			}

			// TODO how to log different types of trials without having empty columns for each logfile
			// Log all that shizzle
			// for (int i = 1; i < _trialSequenceKeys.Count; i++) {
			// 	Session.instance.settingsToLog.Add(_trialSequenceKeys[i]);
			// }

			AddToLog("Generated UXF Sequence");
		}

		// /// <summary>/// Convert JSON formatted definition for the seqence into a UXF format to run in the session/// </summary>
		// public void GenerateUXFSequence(List<string> _trialSequenceKeys, List<TrialSequenceValues> _trialSequenceValues) {

		// 	Block newBlock = Session.instance.CreateBlock();
		// 	int currentblockNumber = 0;

		// 	foreach (TrialSequenceValues row in _trialSequenceValues) {
		// 		if ((int.Parse(row.values[0])-1) != currentblockNumber) { // -1 as the JSON block_num starts at value 1
		// 			currentblockNumber++;
		// 			newBlock = Session.instance.CreateBlock();
		// 		}

		// 		Trial newTrial = newBlock.CreateTrial();

		// 		for (int i = 0; i < _trialSequenceKeys.Count; i++) {
		// 			newTrial.settings.SetValue(_trialSequenceKeys[i], row.values[i].ToUpper()); // set values to trial
		// 		}
		// 	}

		// 	// Log all that shizzle
		// 	for (int i = 1; i < _trialSequenceKeys.Count; i++) {
		// 		Session.instance.settingsToLog.Add(_trialSequenceKeys[i]);
		// 	}

		// 	AddToLog("Generated UXF Sequence");
		// }

#endregion	// -------------------------------------------------------------------------------------------------------------------------------
#region LOGGING	
		private void AddToExecutionOrderLog (string description) {
			UXF.UXFDataRow newRow = new UXFDataRow();
			newRow.Add(("timestamp", Time.time)); // Log timestamp
			newRow.Add(("executed", description)); 
			executionOrderLog.AddCompleteRow(newRow);
		}

		/// <summary>
		/// Saves a marker with a timestamp
		/// </summary>
		/// <param name="annotation">Annotation to store</param>
		public void SendMarker (string annotation) {

			// Log it in the UXF way
			UXF.UXFDataRow newRow = new UXFDataRow();
			newRow.Add(("timestamp", Time.realtimeSinceStartup)); // Log timestamp
			newRow.Add(("annotation", annotation)); 
			markerLog.AddCompleteRow(newRow);

			EventManager.TriggerEvent(eDIA.Events.DataHandlers.EvSendMarker, new eParam(annotation));
		}

		private void AddToLog(string _msg) {
			if (showLog)
				eDIA.LogUtilities.AddToLog(_msg, "EXP", taskColor);
		}
		

#endregion	// -------------------------------------------------------------------------------------------------------------------------------
	}

}