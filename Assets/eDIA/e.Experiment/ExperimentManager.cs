using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UXF;
using UnityEngine.Events;
using eDIA.EditorUtils;

// EXPERIMENT CONTROL 
namespace eDIA {

#region DECLARATIONS

	/// <summary> Core manager of the experiment. Singleton so available throughout the system. </summary>
	public class ExperimentManager : Singleton<ExperimentManager> {

		// ---
		[Header("Editor Settings")]
		public bool showLog = false;
		public Color taskColor = Color.green;

		/// <summary> Tuple of strings. Serializable in the inspector and dictionaries are not</summary>
		[System.Serializable]
		public class SettingsTuple {
			[HideInInspector]
			public string key 	= string.Empty;
			public string value 	= string.Empty;
		}

		/// <summary> Stringlist with trial setting values</summary>
		[System.Serializable]
		public class TrialSequenceValues {
			public List<string> values = new List<string>();
		}

		/// <summary> Container for (de)serializing a list to JSON</summary>
		public class TrialSequenceValuesContainer {
			public List<TrialSequenceValues> trialSequenceValues = new List<TrialSequenceValues>();
		}

		/// <summary> Main container to store sessions config, either from disk, editor or network </summary>
		[System.Serializable]
		public class ExperimentConfig {
			public string				experiment			= string.Empty;
			public string 				experimenter 		= string.Empty;
			public string 				participantID 		= string.Empty;
			public int 					sessionNumber 		= 0;
			public List<SettingsTuple>		participantInfo 		= new List<SettingsTuple>();
			public List<SettingsTuple>		sessionSettings 		= new List<SettingsTuple>();
			public List<SettingsTuple> 		blockInstructions 	= new List<SettingsTuple>();
			public List<int>				breakAfter			= new List<int>(); 
			public List<string> 			trialSequenceKeys 	= new List<string>();
			public List<TrialSequenceValues> 	trialSequenceValues 	= new List<TrialSequenceValues>();

			/// <summary>Does the given block have a introduction text defined </summary>
			/// <param name="_blockNumber"></param>
			/// <returns>Boolean True if any</returns>
			public bool hasBlockIntroduction (int _blockNumber) {
				bool itDoes = false;

				if (blockInstructions.Count == 0) // if there are none
					return false;

				foreach (SettingsTuple s in blockInstructions) 
					itDoes = int.Parse(s.key) == _blockNumber ? true : itDoes;
				return itDoes;
			}

			/// <summary> Get the string value for blockintroduction of given blocknumber</summary>
			/// <param name="_blockNumber"></param>
			/// <returns>Introduction text as string</returns>
			public string GetBlockIntroduction (int _blockNumber) {
				string msg = "NO DATA FOUND";
				foreach (SettingsTuple s in blockInstructions)
					msg = int.Parse(s.key) == _blockNumber ? s.value : msg;
				return msg;
			}
		}	

		/// The config instance that holds current experimental configuration
		[HideInInspector]
		public ExperimentConfig experimentConfig;

		// Helpers
		[Space(20)]
		int activeBlockUXF = 0;
		bool isPauseRequested = false;

		// UXF related
		Dictionary<string, object> participantDetails   = new Dictionary<string, object>();
		UXF.Settings currentUXFSessionSettings = new Settings();
		UXF.UXFDataTable executionOrderLog = new UXF.UXFDataTable("start_time", "executed"); 

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region MONO METHODS

		void Awake() {
			EventManager.StartListening("EvSetExperimentConfig", OnEvSetExperimentConfig);
			EventManager.StartListening("EvStartExperiment", OnEvStartExperiment);
			EventManager.StartListening("EvPauseExperiment", OnEvPauseExperiment);
		}
		
		void Start() {
			SetExperimentConfig(null);
		}

		void OnDestroy() {
			EventManager.StopListening("EvSetExperimentConfig", OnEvSetExperimentConfig);
			EventManager.StopListening("EvStartExperiment", OnEvStartExperiment);
			EventManager.StopListening("EvPauseExperiment", OnEvPauseExperiment);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EXPERIMENT INFO

		void OnEvSetExperimentConfig( eParam e) {
			SetExperimentConfig( e == null ? LoadExperimentConfigFromDisk () : e.GetString() );
		}

		/// <summary>Load the default JSON configuration locally</summary>
		/// <returns>JSON string</returns>
		protected string LoadExperimentConfigFromDisk () {
			string experimentJSON = FileManager.ReadStringFromApplicationPath("ExperimentConfig.json");

			if (experimentJSON == "ERROR")
				Debug.LogError("Experiment JSON not correctly loaded!");
			
			return experimentJSON;

			// TODO: There should be a HALT option in the framework, to halt the complete application and rset back
		}

		/// <summary>Set the eDIA experiment settings with the full JSON config string</summary>
		/// <param name="JSONstring">Full config string</param>
		protected void SetExperimentConfig (string JSONstring) {
			experimentConfig = UnityEngine.JsonUtility.FromJson<ExperimentConfig>(JSONstring == null ? LoadExperimentConfigFromDisk () : JSONstring);
			// Debug.Log(UnityEngine.JsonUtility.ToJson(experimentConfig, true));

			SetSessionSettings ();
			SetParticipantDetails ();
			SetTrialSequence ();
		}

		/// <summary> Set the sessionsettings to use by UXF</summary>
		protected void SetSessionSettings () {
			// Add experimenter to settings
			currentUXFSessionSettings.SetValue("experimenter", experimentConfig.experimenter);

			// Convert and add KEYS and VALUES to the UXF settings in order to be logged
			TrialSequenceValuesContainer trialSequenceValuesContainer = new TrialSequenceValuesContainer();
			trialSequenceValuesContainer.trialSequenceValues.AddRange(experimentConfig.trialSequenceValues);
			currentUXFSessionSettings.SetValue("trialSequenceKeys", experimentConfig.trialSequenceKeys);
			currentUXFSessionSettings.SetValue("trialSequenceValues", UnityEngine.JsonUtility.ToJson(trialSequenceValuesContainer, false));

			// Are there default settings?
			if (experimentConfig.sessionSettings.Count == 0)
				return;
			
			// Add to UXF settings
			for (int i=0;i<experimentConfig.sessionSettings.Count; i++) {
				currentUXFSessionSettings.SetValue(experimentConfig.sessionSettings[i].key, experimentConfig.sessionSettings[i].value);
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

		/// <summary> Setting trial sequence by calling UXF sequence generation </summary>
		void SetTrialSequence () {
			GenerateUXFSequence(experimentConfig.trialSequenceKeys, experimentConfig.trialSequenceValues); // Generate sequence for UXF
		}

		/// <summary>Event listener to start the experiment</summary>
		void OnEvStartExperiment (eParam e) {
			EventManager.StopListening("EvStartExperiment", OnEvStartExperiment);
			StartExperiment ();
		}

		/// <summary>Starts the experiment</summary>
		void StartExperiment () {
			Session.instance.Begin( 
				experimentConfig.experiment 		== string.Empty ? "N.A." : experimentConfig.experiment,  
				experimentConfig.participantID 	== string.Empty ? "N.A." : experimentConfig.participantID, 
				experimentConfig.sessionNumber, 
				participantDetails, 
				currentUXFSessionSettings
			); 
		}

		/// <summary>Sets the PauseExperiment flag to true and logs the call for an extra break</summary>
		void OnEvPauseExperiment(eParam e)
		{
			AddToExecutionOrderLog("BreakInjectionMoment");
			isPauseRequested = true;
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region UXF EVENT HANDLERS 

		/// <summary>Start of the UXF session. </summary>
		void OnSessionBeginUXF() {
			AddToLog("OnSessionBeginUXF");
			AddToExecutionOrderLog("OnSessionBegin");
			EventManager.StartListening("EvProceed", OnEvStartFirstTrial);
		}

		/// <summary>Called from user input. </summary>
		void OnEvStartFirstTrial (eParam e) {
			EventManager.StopListening("EvProceed", OnEvStartFirstTrial);
			Session.instance.Invoke("BeginNextTrial", 0.5f);
		}

		/// <summary>Called from UXF session. </summary>
		void OnPreSessionEndUXF() {
			AddToLog("OnPreSessionEndUXF");
			EventManager.StartListening("EvProceed", OnEvFinaliseSession);
		}

		/// <summary>Called from user input. </summary>
		void OnEvFinaliseSession (eParam e) {
			EventManager.StopListening("EvProceed", OnEvFinaliseSession);
			UXF.Session.instance.SaveDataTable(executionOrderLog, "executionOrder");
			Session.instance.End();
		}

		/// <summary>Called from UXF session. </summary>
		void OnSessionEndUXF() {
			AddToLog("OnSessionEndUXF");
		}

		/// <summary>Called from UXF session. Begin setting things up for the trial that is about to start </summary>
		/// <param name="newTrial">Trial passed on by UXF</param>
		protected void OnTrialBeginUXF(Trial newTrial) {
			AddToLog("OnTrialBeginUXF");
			AddToExecutionOrderLog("OnTrialBegin");

			bool showIntroduction = false;

			if ((Session.instance.currentBlockNum != activeBlockUXF) && (Session.instance.currentBlockNum <= Session.instance.blocks.Count)) {
				// Check for block introduction flag
				showIntroduction = experimentConfig.hasBlockIntroduction(Session.instance.currentBlockNum);
				// Set new activeBlockUXF value
				activeBlockUXF = Session.instance.currentBlockNum;
			}

			// Inject introduction step or continue UXF sequence
			if (showIntroduction)
				BlockIntroduction ();
			else {
				EventManager.TriggerEvent("EvTrialBegin", null);
			}

		}

		/// <summary>Called from UXF session. Checks if to call NextTrial, BREAK, PAUSE or End the Session </summary>
		/// <param name="endedTrial">Trial passed on by UXF</param>
		protected void OnTrialEndUXF(Trial endedTrial) {
			AddToLog("OnTrialEndUXF");
			AddToExecutionOrderLog("OnTrialEnd");
			
			// Are we ending?
			if (Session.instance.isEnding)
				return;
			
			// Is there a PAUSE requested right now?
			if (isPauseRequested) {
				isPauseRequested = false;
				AddToExecutionOrderLog("Injected SessionBreak");
				SessionBreak();
				return;
			}

			// Debug.Log("> No pause");

			// Reached last trial in a block?
			if (Session.instance.CurrentBlock.lastTrial != endedTrial) {
				Session.instance.BeginNextTrialSafe();
				return;
			}

			// AddToLog("> Reached last trial in block " + Session.instance.currentBlockNum);
			
			// Is this then the last trial of the session?
			if (Session.instance.LastTrial == endedTrial) {
				AddToLog("Reached end of trials ");
				Session.instance.preSessionEnd.Invoke(Session.instance);
				return;
			}

			// Debug.Log("> Not sessions last trial");

			// Do we take a break or jump to next block?
			if (experimentConfig.breakAfter.Contains(Session.instance.currentBlockNum)) {
				SessionBreak();
				return;
			}
			
			// If we reach here it's just a normal trial and we continue
			Session.instance.BeginNextTrialSafe();

		}

		/// <summary>Starts Experiment BREAK and triggers corresponding event. Awaits <c>EvProceed</c> event to continue.</summary>
		protected void SessionBreak () {
			AddToLog("SessionBreak");
			AddToExecutionOrderLog("SessionBreak");
			EventManager.StartListening("EvProceed", SessionResume);
			EventManager.TriggerEvent("EvSessionBreak", null);
		}

		/// <summary>Handles continue trigger while in BREAK</summary>
		protected void SessionResume (eParam e) {
			AddToExecutionOrderLog("SessionResume");
			AddToLog("SessionResume");
			EventManager.StopListening("EvProceed", SessionResume);
			EventManager.TriggerEvent("EvSessionResume", null);

			Session.instance.Invoke("BeginNextTrialSafe", 0.5f);
		}

		/// <summary>Starts INTRODUCTION and triggers corresponding event. Awaits <c>EvProceed</c> event to continue.</summary>
		protected void BlockIntroduction () {
			AddToLog("BlockIntroduction");
			AddToExecutionOrderLog("BlockIntroduction");
			EventManager.StartListening("EvProceed", BlockResume);
			EventManager.TriggerEvent("EvBlockIntroduction", null);
		}

		/// <summary>Handles continue trigger while in INTRODUCTION</summary>
		protected void BlockResume (eParam e) {
			AddToLog("BlockResume");
			AddToExecutionOrderLog("BlockResume");
			EventManager.StopListening("EvProceed", BlockResume);
			EventManager.TriggerEvent("EvBlockResume",null);
		}

#endregion	// -------------------------------------------------------------------------------------------------------------------------------
#region JSON TO UXF CONVERSION

		/// <summary> Generates trial sequence with the given keys and values for each trial in <c>Trial.Settings</c> </summary>
		/// <param name="_trialSequenceKeys">List of string keys</param>
		/// <param name="_trialSequenceValues">List of string values</param>
		protected void GenerateUXFSequence(List<string> _trialSequenceKeys, List<TrialSequenceValues> _trialSequenceValues) {

			Block newBlock = Session.instance.CreateBlock();
			int currentblockNumber = 0;

			foreach (TrialSequenceValues row in _trialSequenceValues) {
				if ((int.Parse(row.values[0])-1) != currentblockNumber) { // -1 as the JSON block_num starts at value 1
					currentblockNumber++;
					newBlock = Session.instance.CreateBlock();
				}

				Trial newTrial = newBlock.CreateTrial();

				for (int i = 0; i < _trialSequenceKeys.Count; i++) {
					newTrial.settings.SetValue(_trialSequenceKeys[i], row.values[i].ToUpper()); // set values to trial
				}
			}

			// Log all that shizzle
			for (int i = 1; i < _trialSequenceKeys.Count; i++) {
				Session.instance.settingsToLog.Add(_trialSequenceKeys[i]);
			}

			AddToLog("Generated UXF Sequence");
		}

#endregion	// -------------------------------------------------------------------------------------------------------------------------------
#region MISC	
		/// <summary> Add to experiment execution log. Auto inserts timestamp. </summary>
		/// <param name="description">Message to log</param>
		protected void AddToExecutionOrderLog (string description) {
			UXF.UXFDataRow newRow = new UXFDataRow();
			newRow.Add(("start_time", Time.time)); // Log timestamp
			newRow.Add(("executed", description)); 
			executionOrderLog.AddCompleteRow(newRow);
		}

		private void AddToLog(string _msg) {
			if (showLog)
				LogUtilities.AddToLog(_msg, "EXP", taskColor);
		}
		

#endregion	// -------------------------------------------------------------------------------------------------------------------------------
	}

}