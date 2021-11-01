using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UXF;
using UnityEngine.Events;

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
			public string type 	= string.Empty;
			public string key 	= string.Empty;
			public string value 	= string.Empty;
		}

		/// <summary> Temp storage of a list of string to use in another class to generate a two dimensional array basically</summary>
		[System.Serializable]
		public class TrialSequenceValues {
			public List<string> values = new List<string>();
		}

		/// <summary> containerfor (de)serializing a list to JSON</summary>
		public class TrialSequenceValuesContainer {
			public List<TrialSequenceValues> trialSequenceValues = new List<TrialSequenceValues>();
		}
 
		/// <summary> Main container to store sessions config in, either from disk, editor or network </summary>
		[System.Serializable]
		public class ExperimentConfig {
			public string				experiment			= string.Empty;
			public string 				experimenter 		= string.Empty;
			public string 				participantID 		= string.Empty;
			public int 					sessionNumber 		= 0;
			public List<SettingsTuple> 		participantInfo 		= new List<SettingsTuple>();
			public List<SettingsTuple>		sessionSettings 		= new List<SettingsTuple>();
			public List<SettingsTuple> 		blockInstructions 	= new List<SettingsTuple>();
			public List<int>				breakAfter			= new List<int>(); 
			public List<string> 			trialSequenceKeys 	= new List<string>();
			public List<TrialSequenceValues> 	trialSequenceValues 	= new List<TrialSequenceValues>();

			public bool hasBlockIntroduction (int _blockNumber) {
				bool itDoes = false;

				if (blockInstructions.Count == 0) // if there are none
					return false;

				foreach (SettingsTuple s in blockInstructions) 
					itDoes = int.Parse(s.key) == _blockNumber ? true : itDoes;
				return itDoes;
			}

			public string GetBlockIntroduction (int _blockNumber) {
				string msg = "NO DATA FOUND";
				foreach (SettingsTuple s in blockInstructions)
					msg = int.Parse(s.key) == _blockNumber ? s.value : msg;
				return msg;
			}
		}	

		// Local JSON configutation file
		[Header("Experiment Settings")]
		[Tooltip("Reference a local JSON config file here")]
		public TextAsset experimentConfigJSON;
		
		/// The config instance that holds current experimental configuration
		// [HideInInspector]
		public ExperimentConfig experimentConfig;

		// Helpers
		[Space(20)]
		public int activeBlockUXF = 0;
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
			SetExperimentConfig( e == null ? LoadDefaultConfig () : e.GetString() );
		}

		/// <summary>Load the default JSON configuration locally</summary>
		/// <returns>JSON string</returns>
		string LoadDefaultConfig () {
			var jsonTextFile = experimentConfigJSON;
			return jsonTextFile.ToString();
		}

		/// <summary>Set the eDIA experiment settings with the full JSON config string</summary>
		/// <param name="JSONstring">Full config string</param>
		void SetExperimentConfig (string JSONstring) {
			experimentConfig = UnityEngine.JsonUtility.FromJson<ExperimentConfig>(JSONstring == null ? LoadDefaultConfig () : JSONstring);
			Debug.Log(UnityEngine.JsonUtility.ToJson(experimentConfig, false));

			SetSessionSettings ();
			SetParticipantDetails ();
			SetTrialSequence ();

			// Convert and add KEYS and VALUES to the UXF settings in order to be saved with the session
			TrialSequenceValuesContainer trialSequenceValuesContainer = new TrialSequenceValuesContainer();
			trialSequenceValuesContainer.trialSequenceValues.AddRange(experimentConfig.trialSequenceValues);

			currentUXFSessionSettings.SetValue("trialSequenceKeys", experimentConfig.trialSequenceKeys);
			currentUXFSessionSettings.SetValue("trialSequenceValues", UnityEngine.JsonUtility.ToJson(trialSequenceValuesContainer, false));
		}

		/// <summary> Set the sessionsettings to use by UXF</summary>
		void SetSessionSettings () {
			
			if (experimentConfig.sessionSettings.Count == 0)
				return;
			
			// Add experimenter to settings
			currentUXFSessionSettings.SetValue("experimenter", experimentConfig.experimenter);

			// fill in the rest
			for (int i=0;i<experimentConfig.sessionSettings.Count; i++) {
				switch (experimentConfig.sessionSettings[i].type) {
					case "string":
						currentUXFSessionSettings.SetValue(experimentConfig.sessionSettings[i].key, experimentConfig.sessionSettings[i].value.ToString());
					break;
					case "strings":
						List<string> stringlist = experimentConfig.sessionSettings[i].value.Split(',').ToList();
						currentUXFSessionSettings.SetValue(experimentConfig.sessionSettings[i].key, stringlist);
					break;
					case "bool":
						currentUXFSessionSettings.SetValue(experimentConfig.sessionSettings[i].key, experimentConfig.sessionSettings[i].value == "true" ? true : false);
					break;
					case "int":
						currentUXFSessionSettings.SetValue(experimentConfig.sessionSettings[i].key, experimentConfig.sessionSettings[i].value);
					break;
					case "float":
						currentUXFSessionSettings.SetValue(experimentConfig.sessionSettings[i].key, experimentConfig.sessionSettings[i].value);
					break;
				}
			}
		}
		
		/// <summary> Set the participant details to use by UXF</summary>
		void SetParticipantDetails () {
			if (experimentConfig.participantInfo.Count > 0) {
				for (int i=0;i<experimentConfig.participantInfo.Count; i++) {
					participantDetails.Add(experimentConfig.participantInfo[i].key, experimentConfig.participantInfo[i].value);
				}
			}
		}

		void SetTrialSequence () {
			GenerateUXFSequence(experimentConfig.trialSequenceKeys, experimentConfig.trialSequenceValues); // Generate sequence for UXF
		}

		void OnEvStartExperiment (eParam e) {
			EventManager.StopListening("EvStartExperiment", OnEvStartExperiment);
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
		void OnTrialBeginUXF(Trial newTrial) {
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

		/// <summary>Called from UXF session. Checks if to call NextTrial, should start a BREAK before next Block, or End the Session </summary>
		void OnTrialEndUXF(Trial endedTrial) {
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

			Debug.Log("> No pause");

			// Reached last trial in a block?
			if (Session.instance.CurrentBlock.lastTrial != endedTrial) {
				Session.instance.BeginNextTrialSafe();
				return;
			}

			AddToLog("> Reached last trial in block " + Session.instance.currentBlockNum);
			
			// Is this then the last trial of the session?
			if (Session.instance.LastTrial == endedTrial) {
				AddToLog("Reached end of trials ");
				Session.instance.preSessionEnd.Invoke(Session.instance);
				return;
			}

			Debug.Log("> Not sessions last trial");

			// Do we take a break or jump to next block?
			if (experimentConfig.breakAfter.Contains(Session.instance.currentBlockNum)) {
				SessionBreak();
				return;
			}
			
			// If we reach here it's just a normal trial and we continue
			Session.instance.BeginNextTrialSafe();

		}

		/// <summary>Called from this manager. Invokes onSessionBreak event and starts listener to EvProceed event</summary>
		void SessionBreak () {
			AddToLog("SessionBreak");
			AddToExecutionOrderLog("SessionBreak");
			EventManager.StartListening("EvProceed", SessionResume);
			EventManager.TriggerEvent("EvSessionBreak", null);
		}

		/// <summary>Called from EvProceed event. Stops listener, invokes onSessionResume event and calls UXF BeginNextTrial. </summary>
		void SessionResume (eParam e) {
			AddToExecutionOrderLog("SessionResume");
			AddToLog("SessionResume");
			EventManager.StopListening("EvProceed", SessionResume);
			EventManager.TriggerEvent("EvSessionResume", null);

			Session.instance.Invoke("BeginNextTrialSafe", 0.5f);
		}

		/// <summary>Called from this manager. </summary>
		void BlockIntroduction () {
			AddToLog("BlockIntroduction");
			AddToExecutionOrderLog("BlockIntroduction");
			EventManager.StartListening("EvProceed", BlockResume);
			EventManager.TriggerEvent("EvBlockIntroduction", null);
		}

		/// <summary>Called from this manager. </summary>
		void BlockResume (eParam e) {
			AddToLog("BlockResume");
			AddToExecutionOrderLog("BlockResume");
			EventManager.StopListening("EvProceed", BlockResume);
			EventManager.TriggerEvent("EvBlockResume",null);
		}

#endregion	// -------------------------------------------------------------------------------------------------------------------------------
#region JSON TO UXF CONVERSION

		/// <summary>/// Convert JSON formatted definition for the seqence into a UXF format to run in the session/// </summary>
		public void GenerateUXFSequence(List<string> _trialSequenceKeys, List<TrialSequenceValues> _trialSequenceValues) {

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
		private void AddToExecutionOrderLog (string description) {
			UXF.UXFDataRow newRow = new UXFDataRow();
			newRow.Add(("start_time", Time.time)); // Log timestamp
			newRow.Add(("executed", description)); 
			executionOrderLog.AddCompleteRow(newRow);
		}

		private void AddToLog(string _msg) {
			if (showLog)
				eDIA.LogUtilities.AddToLog(_msg, "EXP", taskColor);
		}
		

#endregion	// -------------------------------------------------------------------------------------------------------------------------------
	}

}