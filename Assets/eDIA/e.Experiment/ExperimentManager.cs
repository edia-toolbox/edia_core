using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MiniJSON;
using UXF;
using TTF;
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

		// The config instance that holds current experimental configuration
		// [HideInInspector]
		public ExperimentConfig experimentConfig;

		// Helper to inject introductions before next block starts
		int activeBlockUXF = 0;

		// Events
		[System.Serializable]
		public class DefaultEvent 	: UnityEvent{}

		[System.Serializable]
		public class StringEvent 	: UnityEvent<string>{}

		[System.Serializable]
		public class TrialEvent 	: UnityEvent<Trial>{}

		[SerializeField] 
		[Header("Fired when session break STARTS")]
		public DefaultEvent onSessionBreak;
		
		[SerializeField] 
		[Header("Fired when session break ENDS")]
		public DefaultEvent onSessionResume;

		[SerializeField] 
		[Header("Fired when block has an introduction step")]
		public DefaultEvent onBlockIntroduction;

		[SerializeField] 
		[Header("Fired when introduction is done")]
		public TrialEvent onBlockContinue;


		// UXF related
		Dictionary<string, object> participantDetails   = new Dictionary<string, object>();
		UXF.Settings currentUXFSessionSettings = new Settings();


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region MONO METHODS

		void Awake() {
			EventManager.StartListening("EvSetExperimentConfig", OnEvSetExperimentConfig);
			EventManager.StartListening("EvStartExperiment", OnEvStartExperiment);
		}
		
		void Start() {
			SetExperimentConfig(null);
		}

		void OnDestroy() {
			EventManager.StopListening("EvSetExperimentConfig", OnEvSetExperimentConfig);
			EventManager.StopListening("EvStartExperiment", OnEvStartExperiment);
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EXPERIMENT INFO

		void OnEvSetExperimentConfig( eParam e) {
			SetExperimentConfig( e == null ? LoadDefaultConfig () : e.GetString() );
		}

		string LoadDefaultConfig () {
        		var jsonTextFile = Resources.Load<TextAsset>("DefaultSessionConfigJSON");
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
			
			//TODO: Expand this with multiple inputs
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
					break;
					case "float":
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
				experimentConfig.experimenter 	== string.Empty ? "N.A." : experimentConfig.experimenter, 
				experimentConfig.participantID 	== string.Empty ? "N.A." : experimentConfig.participantID, 
				experimentConfig.sessionNumber, 
				participantDetails, 
				currentUXFSessionSettings
			); 

		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region UXF EVENT HANDLERS 

		void OnSessionBeginUXF() {
			AddToLog("OnSessionBeginUXF");
			Session.instance.Invoke("BeginNextTrial", 0.5f);
		}

		/// <summary>Called from UXF session. </summary>
		void OnPreSessionEndUXF() {
			AddToLog("OnPreSessionEndUXF");
		}

		/// <summary>Called from UXF session. </summary>
		void OnSessionEndUXF() {
			AddToLog("OnSessionEndUXF");
		}

		/// <summary>Called from UXF session. Begin setting things up for the trial that is about to start </summary>
		void OnTrialBeginUXF(Trial trial) {
			AddToLog("OnTrialBeginUXF");
			AddToLog ("Block: " + Session.instance.currentBlockNum + " Trial:" + Session.instance.currentTrialNum); 

			bool showIntroduction = false;

			if ((Session.instance.currentBlockNum != activeBlockUXF) && (Session.instance.currentBlockNum < Session.instance.blocks.Count)) {
				// Check for block introduction flag
				showIntroduction = experimentConfig.hasBlockIntroduction(Session.instance.currentBlockNum);

				// Set new activeBlockUXF value
				activeBlockUXF = Session.instance.currentBlockNum;
			}

			// Inject introduction step or continue UXF sequence
			if (showIntroduction)
				BlockIntroduction ();
			else 
				onBlockContinue.Invoke(Session.instance.CurrentTrial);

		}

		/// <summary>Called from UXF session. </summary>
		void OnTrialEndUXF(Trial endedTrial) {
			AddToLog("OnTrialEndUXF");
			if (Session.instance.isEnding)
				return;

			if (Session.instance.CurrentBlock.lastTrial != Session.instance.CurrentTrial) {
				Session.instance.BeginNextTrialSafe();
				return;
			}

			AddToLog("Reached last trial in block " + Session.instance.currentBlockNum);
			
			// Do we take a break or jump to next block?
			if (experimentConfig.breakAfter.Contains(Session.instance.currentBlockNum)) 
				SessionBreak();
			else {
				Session.instance.BeginNextTrialSafe();
			}
		}

		/// <summary>Called from this manager. </summary>
		void SessionBreak () {
			AddToLog("SessionBreak");
			EventManager.StartListening("EvProceed", SessionResume);
			onSessionBreak.Invoke();
		}

		/// <summary>Called from this manager. </summary>
		void SessionResume (eParam e) {
			EventManager.StopListening("EvProceed", SessionResume);
			onSessionResume.Invoke();
			AddToLog("SessionResume");

			Session.instance.Invoke("BeginNextTrialSafe", 1f);
		}

		/// <summary>Called from this manager. </summary>
		void BlockIntroduction () {
			AddToLog("BlockIntroduction");
			EventManager.StartListening("EvProceed", BlockResume);
			onBlockIntroduction.Invoke();
		}

		/// <summary>Called from this manager. </summary>
		void BlockResume (eParam e) {
			EventManager.StopListening("EvProceed", BlockResume);
			onBlockContinue.Invoke(Session.instance.CurrentTrial);
			AddToLog("BlockResume");
		}

#endregion	// -------------------------------------------------------------------------------------------------------------------------------
#region JSON TO UXF CONVERSION

		/// <summary>/// Convert JSON formatted definition for the seqence into a UXF format to run in the session/// </summary>
		public void GenerateUXFSequence(List<string> _trialSequenceKeys, List<TrialSequenceValues> _trialSequenceValues) {

			Block newBlock = Session.instance.CreateBlock();
			int currentblockNumber = 0;

			foreach (TrialSequenceValues row in _trialSequenceValues) {
				if ((int.Parse(row.values[0])-1) != currentblockNumber) { // -1 as the JSON block_num starts at value 1
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

		private void AddToLog(string _msg) {
			if (showLog)
				eDIA.LogUtilities.AddToLog(_msg, "EXP", taskColor);
		}
		

#endregion	// -------------------------------------------------------------------------------------------------------------------------------
	}

}