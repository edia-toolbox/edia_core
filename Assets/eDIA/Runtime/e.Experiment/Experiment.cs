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

	public class Experiment : Singleton<Experiment> {

		[Header("Editor Settings")]
		public bool showLog = false;
		public Color taskColor = Color.green;

		[Space(20)]
		public List<TaskBlock> taskBlocks = new List<TaskBlock>();

		[Space(20)]
		[Header("Event hooks\n\nOptional event hooks to use in your task")]
		public UnityEvent OnSessionStart = null;
		public UnityEvent OnSessionBreak = null;
		public UnityEvent OnSessionEnd = null;

		/// The config instance that holds current experimental configuration
		[HideInInspector]
		public ExperimentConfig experimentConfig;
		[HideInInspector]
		public TaskConfig taskConfig;

		// Helpers
		[Space(20)]
		int activeBlockUXF = 0;
		bool isPauseRequested = false;

		// UXF Logging
		UXF.UXFDataTable executionOrderLog 	= new UXF.UXFDataTable("timestamp", "executed"); 
		UXF.UXFDataTable markerLog 		= new UXF.UXFDataTable("timestamp", "annotation");

		/// <summary> Currently active step number. </summary>
		[HideInInspector] public int currentStepNum = -1;

		Coroutine stepTimer = null;


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region MONO METHODS

		void Awake() {
			// Disable task block script before anything starts to run
			foreach(TaskBlock t in taskBlocks) {
				t.enabled = false;
			}

			EventManager.StartListening(eDIA.Events.Core.EvFoundLocalConfigFiles, 	OnEvFoundLocalConfigFiles);
			EventManager.StartListening(eDIA.Events.Core.EvSetExperimentConfig, 	OnEvSetExperimentConfig);
			EventManager.StartListening(eDIA.Events.Core.EvStartExperiment, 		OnEvStartExperiment);
			EventManager.StartListening(eDIA.Events.Core.EvQuitApplication, 		OnEvQuitApplication);

			EventManager.showLog = showLog;
		}

		void OnDestroy() {
			EventManager.StopListening(eDIA.Events.Core.EvSetExperimentConfig, 	OnEvSetExperimentConfig);
			EventManager.StopListening(eDIA.Events.Core.EvStartExperiment, 		OnEvStartExperiment);
			EventManager.StopListening(eDIA.Events.Core.EvPauseExperiment, 		OnEvPauseExperiment);
			EventManager.StopListening(eDIA.Events.Core.EvFoundLocalConfigFiles, 	OnEvFoundLocalConfigFiles);
			EventManager.StopListening(eDIA.Events.Core.EvQuitApplication, 		OnEvQuitApplication);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region SETUP CONFIGS 

		/// <summary>Register local mode, listen to submission of config file</summary>
		void OnEvFoundLocalConfigFiles (eParam e) {

			EventManager.StopListening(eDIA.Events.Core.EvFoundLocalConfigFiles, OnEvFoundLocalConfigFiles);		
			AddToLog(e.GetInt() + " local config files added");
			EventManager.StartListening(eDIA.Events.Core.EvLocalConfigSubmitted, OnEvLocalConfigSubmitted);
		}

		/// <summary>Look up given index in the localConfigFiles list and give content of that file to system </summary>
		/// <param name="e">String = filename of the configfile</param>
		void OnEvLocalConfigSubmitted (eParam e) {

			EventManager.StopListening(eDIA.Events.Core.EvLocalConfigSubmitted, OnEvLocalConfigSubmitted);
			
			string filenameExperiment = e.GetStrings()[0] + "_" + e.GetStrings()[1] + ".json"; // combine task string and participant string
			SetExperimentConfig (FileManager.ReadStringFromApplicationPathSubfolder(eDIA.Constants.localConfigDirectoryName + "/Participants", filenameExperiment));

			string filenameTask = e.GetStrings()[0] + ".json"; // task string
			SetTaskConfig (FileManager.ReadStringFromApplicationPathSubfolder(eDIA.Constants.localConfigDirectoryName + "/Tasks", filenameTask));

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

		/// <summary>Set the eDIA experiment settings with the full JSON config string</summary>
		/// <param name="JSONstring">Full config string</param>
		void SetExperimentConfig (string JSONstring) {

			try
			{
				experimentConfig = UnityEngine.JsonUtility.FromJson<ExperimentConfig>(JSONstring);
			}
			catch (System.Exception)
			{
				Debug.Log("Exp Init not ok!");
				throw;
			}

			EventManager.TriggerEvent(eDIA.Events.Core.EvExperimentConfigSet, null);
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateExperimentSummary, new eParam( experimentConfig.GetExperimentSummary()) );

			experimentConfig.isReady = true;
			CheckExperimentReady();
		}

		/// <summary>Set the eDIA experiment settings with the full JSON config string</summary>
		/// <param name="JSONstring">Full config string</param>
		void SetTaskConfig (string JSONstring) {
			// Debug.Log(UnityEngine.JsonUtility.ToJson(experimentConfig, true));

			try
			{
				taskConfig = UnityEngine.JsonUtility.FromJson<TaskConfig>(JSONstring);
				taskConfig.GenerateUXFSequence(); // Generate sequence for UXF
			}
			catch (System.Exception)
			{
				Debug.Log("Task Init not ok!");
				throw;
			}

			EventManager.TriggerEvent(eDIA.Events.Core.EvTaskConfigSet, null);

			taskConfig.isReady = true;
			CheckExperimentReady();
		}

		// TODO: Validate configs and show correct panels in control 
		void CheckExperimentReady () {
			if (experimentConfig.isReady && taskConfig.isReady)
				EventManager.TriggerEvent(eDIA.Events.Core.EvReadyToGo, null);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EXPERIMENT CONTROL

		/// <summary>Starts the experiment</summary>
		public void StartExperiment () {

			AddXRrigTracking();

			Session.instance.Begin( 
				experimentConfig.experiment == string.Empty ? "N.A." : experimentConfig.experiment,  
				experimentConfig.GetParticipantID(), 
				experimentConfig.session_number, 
				experimentConfig.GetParticipantDetailsAsDict(),
				new UXF.Settings(taskConfig.GetTaskSettingsAsDict())
			); 
			
		}

		void OnEvStartExperiment (eParam e) {
			EventManager.StopListening(eDIA.Events.Core.EvStartExperiment, OnEvStartExperiment);
			
			StartExperiment ();
		}
		
		/// <summary>Sets the PauseExperiment flag to true and logs the call for an extra break</summary>
		void OnEvPauseExperiment(eParam e)
		{
			AddToExecutionOrderLog("InjectedSessionBreakCall");
			isPauseRequested = true;
		}

		private void OnEvQuitApplication(eParam obj)
		{
			AddToLog("Quiting..");
			Application.Quit();
		}


		public void EnablePauseButton(bool _onOff) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam( new string[] { "PAUSE", _onOff.ToString() }));
			EventManager.StartListening("EvPauseExperiment", OnEvPauseExperiment);
		}

		public void WaitOnProceed() {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam( new string[] { "PROCEED", "TRUE" }));
			EventManager.StartListening (eDIA.Events.Core.EvProceed, OnEvProceed);
		}

		void OnEvProceed (eParam e) {
			Debug.Log("Exp: OnEvProceed called");
			EventManager.StopListening(eDIA.Events.Core.EvProceed, OnEvProceed); // stop listening to avoid doubleclicks
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam( new string[] { "PROCEED", "false" })); // disable button, as OnEvProceed might have come from somewhere else than the button itself
			NextStep();
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
			EventManager.TriggerEvent(eDIA.Events.Core.EvFinalizeSession, null);
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvExperimentProgressUpdate, new eParam("Finalize Session"));
			Session.instance.End();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region UXF RELATED HELPERS

		public void AddToTrialResults (string key, string value) {
			Session.instance.CurrentTrial.result[key] = value;
		}

		void AddXRrigTracking () {
			
			Session.instance.trackedObjects.Add(XRManager.Instance.XRCam.GetComponent<Tracker>());
			Session.instance.trackedObjects.Add(XRManager.Instance.XRRight.GetComponent<Tracker>());
			Session.instance.trackedObjects.Add(XRManager.Instance.XRLeft.GetComponent<Tracker>());
		}

		void SaveCustomDataTables()
		{
			Session.instance.SaveDataTable(executionOrderLog, "executionOrder");
			Session.instance.SaveDataTable(markerLog, "markerLog");
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region STATEMACHINE UXF SESSION

		/// <summary>Start of the UXF session. </summary>
		void OnSessionBeginUXF() {
			OnSessionStart?.Invoke();
			
			AddToExecutionOrderLog("OnSessionBegin");
			EventManager.StartListening(eDIA.Events.Core.EvProceed, OnEvStartFirstTrial);

			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateExperimentSummary, new eParam(experimentConfig.GetExperimentSummary()));
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvExperimentProgressUpdate, new eParam("Welcome"));
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam( new string[] { "PROCEED", "true" }));

			// eye calibration option enabled
			EnableEyeCalibrationTrigger(true);
		}


		/// <summary>Called from UXF session. </summary>
		void OnSessionEndUXF() {
			OnSessionEnd?.Invoke();

			foreach(TaskBlock t in taskBlocks) {
				t.enabled = false;
			}
			
			AddToExecutionOrderLog("OnSessionEndUXF");

			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvExperimentProgressUpdate, new eParam("End"));
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam( new string[] { "PROCEED", "false" }));
			EnablePauseButton(false);

		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region STATEMACHINE BLOCKS

		void BlockStart () {
			AddToLog("Block Start");

			// Disable old block
			if (Session.instance.currentBlockNum-1 != 0)
				taskBlocks[Session.instance.currentBlockNum-2].enabled = false;

			// enable new block
			taskBlocks[Session.instance.currentBlockNum-1].enabled = true;
			taskBlocks[Session.instance.currentBlockNum-1].OnBlockStart();

			// Set new activeBlockUXF value
			activeBlockUXF = Session.instance.currentBlockNum;

			// Check for block introduction flag
			bool hasIntro = Session.instance.CurrentBlock.settings.GetString("intro") != string.Empty;

			// Inject introduction step or continue UXF sequence
			if (hasIntro) {
				EventManager.StartListening(eDIA.Events.Core.EvProceed, BlockContinueAfterIntro); // listener as it event call can come from any script
				ShowMessageToUser (Session.instance.CurrentBlock.settings.GetString("intro"), "Block Intro");
			}
			else {
				StartTrial();
				EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvExperimentProgressUpdate, new eParam(Session.instance.CurrentBlock.settings.GetString("block_name")));
			}

		}

		void BlockEnd () {
			AddToLog("Block End");
			taskBlocks[Session.instance.currentBlockNum-1].OnBlockEnd();

			// Check for block outro flag
			bool hasOutro = Session.instance.CurrentBlock.settings.GetString("outro") != string.Empty;

			// Inject introduction step or continue UXF sequence
			if (hasOutro) {
				EventManager.StartListening(eDIA.Events.Core.EvProceed, BlockContinueAfterOutro); // listener as it event call can come from any script
				ShowMessageToUser (Session.instance.CurrentBlock.settings.GetString("outro"), "Block Outro");
			}
			else {
				BlockCheckAndContinue();
			}
		}

		void BlockCheckAndContinue () {
			// Is this then the last trial of the session?
			if (Session.instance.LastTrial == Session.instance.CurrentTrial) {
				AddToLog("Reached end of trials ");
				FinalizeSession();
				return;
			}

			// Do we take a break or jump to next block?
			if (taskConfig.breakAfter.Contains(Session.instance.currentBlockNum)) {
				SessionBreak();
				return;
			}

			Session.instance.BeginNextTrialSafe();
		}


		/// <summary>Called from this manager. </summary>
		void ShowMessageToUser (string msg, string description) {
			AddToExecutionOrderLog("ShowMessageToUser");

			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam( new string[] { "PROCEED", "true" }));
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvExperimentProgressUpdate, new eParam("Block Info"));

			EnablePauseButton(false);
			EnableEyeCalibrationTrigger(true);

			if (MessagePanelInVR.Instance != null)
				MessagePanelInVR.Instance.ShowMessage (msg);
			else Debug.LogError("No MessagePanelInVR instance found");
		}

		/// <summary>Called from this manager. </summary>
		void BlockContinueAfterIntro (eParam e) {
			EventManager.StopListening(eDIA.Events.Core.EvProceed, BlockContinueAfterIntro);
			AddToExecutionOrderLog("BlockContinueAfterIntro");
			
			EnableEyeCalibrationTrigger(false);

			StartTrial();
		}

				/// <summary>Called from this manager. </summary>
		void BlockContinueAfterOutro (eParam e) {
			EventManager.StopListening(eDIA.Events.Core.EvProceed, BlockContinueAfterOutro);
			AddToExecutionOrderLog("BlockContinueAfterOutro");
			
			BlockCheckAndContinue();
		}




#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region STATEMACHINE UXF TRIAL
		/// <summary>Called from user input. </summary>
		void OnEvStartFirstTrial (eParam e) {
			EventManager.StopListening(eDIA.Events.Core.EvProceed, OnEvStartFirstTrial);
			Session.instance.BeginNextTrial();
		}

		/// <summary>Called from UXF session. Begin setting things up for the trial that is about to start </summary>
		void OnTrialBeginUXF(Trial newTrial) {
			AddToExecutionOrderLog("OnTrialBegin");

			bool isNewBlock = (Session.instance.currentBlockNum != activeBlockUXF) && (Session.instance.currentBlockNum <= Session.instance.blocks.Count);
			
			if (isNewBlock) {
				BlockStart();
			} else {
				StartTrial();
				EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvExperimentProgressUpdate, new eParam(Session.instance.CurrentBlock.settings.GetString("block_name")));
			}
		}

		/// <summary>Called from UXF session. Checks if to call NextTrial, should start a BREAK before next Block, or End the Session </summary>
		void OnTrialEndUXF(Trial endedTrial) {
			AddToExecutionOrderLog("OnTrialEnd");
			SaveCustomDataTables();

			taskBlocks[Session.instance.currentBlockNum-1].OnEndTrial();

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
			if (Session.instance.CurrentBlock.lastTrial != endedTrial) { // NO
				Session.instance.BeginNextTrialSafe();
				return;
			} else {
				BlockEnd(); // YES
				return;
			}
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region STATEMACHINE CURRENT TRIAL STEPS

		void StartTrial() {

			AddToLog("StartTrial");
			taskBlocks[Session.instance.currentBlockNum-1].OnStartTrial();
			
			currentStepNum = -1;

			// Fire up the task state machine to run the steps of the trial.
			NextStep();
		}

		/// <summary>Called after the task sequence is done </summary>
		void EndTrial() {
			AddToLog("Trial Steps DONE");
			Session.instance.EndCurrentTrial(); // tells UXF to end this trial and fire the event that follows
		}

		/// <summary>Call next step in the trial with delay.</summary>
		/// <param name="duration">Time to wait before proceeding. Expects float</param>
		public void NextStepWithDelay (float duration) {
			if (stepTimer != null) StopCoroutine(stepTimer); // Kill timer, if any

			stepTimer = StartCoroutine("NextStepTimer", duration);
		}

		/// <summary>Coroutine as timer as we can kill that to avoid delayed calls in the statemachine</summary>
		IEnumerator NextStepTimer (float duration) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvStartTimer, new eParam(duration));
			yield return new WaitForSecondsRealtime(duration);
			NextStep();
		}

		/// <summary>Call next step in the trial.</summary>
		public void NextStep() {
			if (stepTimer != null) {
				StopCoroutine(stepTimer); // Kill timer, if any
				EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvStopTimer, null);
			}

			// In case OnProceed was triggered outside of the button
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "FALSE"}));

			currentStepNum ++;

			if (currentStepNum < taskBlocks[Session.instance.CurrentBlock.number-1].trialSteps.Count) {
				OnBetweenSteps();
				taskBlocks[Session.instance.currentBlockNum-1].OnBetweenSteps(); // In Between to steps of the trial, we might want to clean things up a bit.

				// update progress
				EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvExperimentProgressUpdate, new eParam(Session.instance.CurrentBlock.settings.GetString("block_name")));
				taskBlocks[Session.instance.currentBlockNum-1].trialSteps[currentStepNum].Invoke();
			}
			else EndTrial();
		}

		/// <summary>In Between to steps of the trial, we might want to clean things up a bit.</summary>
		void OnBetweenSteps () {
			MessagePanelInVR.Instance.HidePanel();
		}

		
#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BREAK

		/// <summary>Called from this manager. Invokes onSessionBreak event and starts listener to EvProceed event</summary>
		void SessionBreak () {
			AddToExecutionOrderLog("SessionBreak");
				
			EventManager.StartListening(eDIA.Events.Core.EvProceed, SessionResumeAfterBreak);
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvExperimentProgressUpdate, new eParam("Break"));
			
			OnSessionBreak.Invoke();

			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam( new string[] { "PROCEED", "true" }));
			// EnableExperimentProceed(true);
			EnablePauseButton(false);
			EnableEyeCalibrationTrigger(true);
		}

		/// <summary>Called from EvProceed event. Stops listener, invokes onSessionResume event and calls UXF BeginNextTrial. </summary>
		void SessionResumeAfterBreak (eParam e) {
			AddToExecutionOrderLog("SessionResume");

			EventManager.StopListening(eDIA.Events.Core.EvProceed, SessionResumeAfterBreak);

			EnableEyeCalibrationTrigger(false);

			//? Why the delay here ?
			Session.instance.Invoke("BeginNextTrialSafe", 0.5f);
		}

#endregion	// -------------------------------------------------------------------------------------------------------------------------------
#region LOGGING	

		/// <summary>Converts given data to a UXF Table, and stores the data to disk linked to the active trial at the time</summary>
		/// <param name="headers">Headers of the data</param>
		/// <param name="values">Data as List<string>[]</param>
		/// <param name="filename">Name to store the data with</param>
		public void ConvertAndSaveDataToUXF(string[] headers, List<string[]> values, string filename)
		{
			var UXFheaders = headers;
			var data = new UXF.UXFDataTable(UXFheaders);

			foreach (string[] valuerow in values)
			{
				UXFDataRow newRow = new UXFDataRow();
				for(int s=0;s<valuerow.Length;s++) {
					newRow.Add((UXFheaders[s], valuerow[s]));
				}
				data.AddCompleteRow(newRow);
			}

			// Save data
			Session.instance.CurrentTrial.SaveDataTable(data,filename);
		}

		/// <summary>Converts given data to a UXF Table, and stores the data to disk linked to the active trial at the time</summary>
		/// <param name="headers">Headers of the data</param>
		/// <param name="values">Data as List<int></param>
		/// <param name="filename">Name to store the data with</param>
		public void ConvertAndSaveDataToUXF(string[] headers, List<int> values, string filename)
		{
			List<string[]> converted = new List<string[]>();
			
			for(int i=0;i<values.Count;i++) {
				converted.Add(new string[] { (i+1).ToString(), values[i].ToString() });	
			}
			
			ConvertAndSaveDataToUXF(headers, converted, filename);
		}

		
		private void AddToExecutionOrderLog (string description) {
			AddToLog(description);
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