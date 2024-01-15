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

		public enum EState {
			IDLE,
			RUNNING,
			WAITINGONPROCEED,
			PAUSED,
			ENDED
		}

		public EState State = EState.IDLE;
		EState prevState = EState.IDLE;

		[Header("Editor Settings")]
		public bool ShowLog = false;
		public Color LogColor = Color.green;

		[Space(20)]
		[Header("Experiment ")]
		public List<EBlock> Blocks = new();

		[Space(20)]
		[Header("Event hooks\n\nOptional event hooks to use in your task")]
		public UnityEvent OnSessionStart = null;
		public UnityEvent OnSessionPaused = null;
		public UnityEvent OnSessionEnd = null;

		// Fields
		int _activeSessionBlockNum = 0;
		int _currentStep = -1;
		bool _isPauseRequested = false;
		EBlock _activeEBlock;
		Coroutine _proceedTimer = null;

		// UXF Logging
		UXFDataTable _executionOrderLog = new("timestamp", "executed");
		UXFDataTable _markerLog = new("timestamp", "annotation");
		
		
		//! OLD
		/// The config instance that holds current experimental configuration
		public List<TaskBlock> taskBlocks = new();
		[HideInInspector]
		public SessionInfo experimentConfig = new();
		[HideInInspector]
		public TaskConfig taskConfig = new TaskConfig();


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region MONO METHODS

		void Awake() {

			EventManager.showLog = true;

			// Hard reference statemachine links between UXF and EXP
			Session.instance.onSessionBegin.AddListener(OnSessionBeginUXF);
			Session.instance.onSessionEnd.AddListener(OnSessionEndUXF);
			Session.instance.onTrialBegin.AddListener(OnTrialBeginUXF);
			Session.instance.onTrialEnd.AddListener(OnTrialEndUXF);

			foreach (EBlock eb in Blocks) {
				eb.enabled = false;
				eb.gameObject.SetActive(false);
			}

			EventManager.StartListening(eDIA.Events.StateMachine.EvStartExperiment, OnEvStartExperiment);
			EventManager.StartListening(eDIA.Events.Core.EvQuitApplication, OnEvQuitApplication);

			EventManager.showLog = ShowLog;
		}

		void OnDestroy() {
			EventManager.StopListening(eDIA.Events.StateMachine.EvStartExperiment, OnEvStartExperiment);
			EventManager.StopListening(eDIA.Events.StateMachine.EvPauseExperiment, OnEvPauseExperiment);
			EventManager.StopListening(eDIA.Events.Core.EvQuitApplication, OnEvQuitApplication);
		}

		/// <summary>Called from this manager. </summary>
		void ShowMessageToUser(string msg, string description) {
			AddToExecutionOrderLog("ShowMessageToUser");

			EnableProceedButton(true);
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateProgressInfo, new eParam("Block Info"));

			EnablePauseButton(false);
			EnableEyeCalibrationTrigger(true);

			if (MessagePanelInVR.Instance != null)
				MessagePanelInVR.Instance.ShowMessage(msg);
			else Debug.LogError("No MessagePanelInVR instance found");
		}

		void EnableProceedButton (bool onOff) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", onOff ? "true" : "false" }));
		}

		void UpdateProgressInfo (string info) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateProgressInfo, new eParam(info));
		}

		private void Update() {
			if (State != prevState) {
				prevState = State;
			}
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EXPERIMENT CONTROL

		/// <summary>Starts the experiment</summary>
		void StartExperiment() {
			AddXRrigTracking();

			Session.instance.Begin(
			  experimentConfig.experiment == string.Empty ? "N.A." : experimentConfig.experiment,
			  experimentConfig.GetParticipantID(),
			  experimentConfig.session_number,
			  experimentConfig.GetParticipantDetailsAsDict()
			);


		}

		void OnEvStartExperiment(eParam e) {
			EventManager.StopListening(eDIA.Events.StateMachine.EvStartExperiment, OnEvStartExperiment);

			StartExperiment();
		}

		/// <summary>Sets the PauseExperiment flag to true and logs the call for an extra break</summary>
		void OnEvPauseExperiment(eParam e) {
			AddToExecutionOrderLog("InjectedSessionPauseCall");
			_isPauseRequested = true;
		}

		void OnEvQuitApplication(eParam obj) {
			AddToLog("Quiting..");
			Application.Quit();
		}

		public void EnablePauseButton(bool _onOff) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PAUSE", _onOff.ToString() }));
			EventManager.StartListening("EvPauseExperiment", OnEvPauseExperiment);
		}

		/// <summary> Set system open for calibration call from event or button</summary>
		/// <param name="onOff"></param>
		public void EnableEyeCalibrationTrigger(bool _onOff) {
			EventManager.TriggerEvent(eDIA.Events.Eye.EvEnableEyeCalibrationTrigger, new eParam(_onOff));
		}



#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region STATEMACHINE PROCEED

		public void WaitOnProceed() {
			State = EState.WAITINGONPROCEED;

			EnableProceedButton(true);
			EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, OnEvProceed);
		}

		public void Proceed() {
			EventManager.TriggerEvent(eDIA.Events.StateMachine.EvProceed);
		}

		void OnEvProceed(eParam e) {
			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, OnEvProceed);
			EnableProceedButton(false);

			Continue();
		}

		void Continue() {
			State = EState.RUNNING;

			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, OnEvProceed);
			EnableProceedButton(false);

			NextTrialStep();
		}

		public void EnableExperimentProceed(bool onOff) {
			WaitOnProceed();
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region STATEMACHINE UXF SESSION

		/// <summary>Start of the UXF session. </summary>
		void OnSessionBeginUXF(Session session) {
			OnSessionStart?.Invoke();

			State = EState.RUNNING;
			_activeSessionBlockNum = 0;

			AddToExecutionOrderLog("OnSessionBegin");
			EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, OnEvStartFirstTrial);

			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateBlockProgress, new eParam(new int[] { Session.instance.currentBlockNum, Session.instance.blocks.Count }));
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateSessionSummary, new eParam(experimentConfig.GetSessionSummary()));
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateProgressInfo, new eParam("Welcome"));

			EnableProceedButton(true);

			// eye calibration option enabled
			EnableEyeCalibrationTrigger(true);
		}

		/// <summary>Called from UXF session. </summary>
		void OnSessionEndUXF(Session session) {
			OnSessionEnd?.Invoke();

			foreach (TaskBlock t in taskBlocks) {
				t.gameObject.SetActive(false);
			}

			AddToExecutionOrderLog("OnSessionEndUXF");

			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateProgressInfo, new eParam("End"));
			EnableProceedButton(false);

			EnablePauseButton(false);
		}

		/// <summary>Done with all trial, clean up and call UXF to end this session</summary>
		void FinalizeSession() {
			AddToLog("FinalizeSession");

			// clean
			EventManager.TriggerEvent(eDIA.Events.StateMachine.EvFinalizeSession, null);
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateProgressInfo, new eParam("Finalize Session"));
			Session.instance.End();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region STATEMACHINE BLOCKS

		void BlockStart() {
			AddToLog("Block Start");

			//// Disable old block
			//if (Session.instance.currentBlockNum - 1 != 0)
			//	taskBlocks[Session.instance.currentBlockNum - 2].enabled = false;

			//// enable new block
			//taskBlocks[Session.instance.currentBlockNum - 1].enabled = true;
			//taskBlocks[Session.instance.currentBlockNum - 1].OnBlockStart();

			// Set new storedBlockNum value
			_activeSessionBlockNum = Session.instance.currentBlockNum;

			_activeEBlock = Blocks[Blocks.FindIndex(x => x.name == Session.instance.CurrentBlock.settings.GetString("_assetId"))];
			_activeEBlock.enabled = true;
			_activeEBlock.gameObject.SetActive(true);

			// Update block progress
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateBlockProgress, 
				new eParam(new int[] { Session.instance.currentBlockNum, Session.instance.blocks.Count }));

			// Check for block introduction flag
			bool hasIntro = Session.instance.CurrentBlock.settings.GetString("intro") != string.Empty;

			// Inject introduction step or continue UXF sequence
			if (hasIntro) {
				EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, BlockContinueAfterIntro); // listener as it event call can come from any script
				ShowMessageToUser(Session.instance.CurrentBlock.settings.GetString("intro"), "Block Intro");
				taskBlocks[Session.instance.currentBlockNum - 1].OnBlockIntro();
			}
			else {
				StartTrial();
				EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateProgressInfo, 
					new eParam(Session.instance.CurrentBlock.settings.GetString("block_name")));
			}
		}

		void BlockEnd() {
			AddToLog("Block End");
			taskBlocks[Session.instance.currentBlockNum - 1].OnBlockEnd();

			// Check for block outro flag
			bool hasOutro = Session.instance.CurrentBlock.settings.ContainsKey("outro"); // TODO test if block outtro works 
			
			if (hasOutro) {
				EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, BlockContinueAfterOutro); // listener as it event call can come from any script
				EnableProceedButton(false);
				ShowMessageToUser(Session.instance.CurrentBlock.settings.GetString("outro"), "Block Outro");
				Blocks[Session.instance.currentBlockNum - 1].OnBlockOutro();
			}
			else {
				BlockCheckAndContinue();
			}
		}

		void BlockCheckAndContinue() {

			_activeEBlock.enabled = false;
			_activeEBlock.gameObject.SetActive(false);

			// Is this then the last trial of the session?
			if (Session.instance.LastTrial == Session.instance.CurrentTrial) {
				AddToLog("Reached end of trials ");
				FinalizeSession();
				return;
			}

			Session.instance.BeginNextTrialSafe();
		}

		/// <summary>Called from this manager. </summary>
		void BlockContinueAfterIntro(eParam e) {
			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, BlockContinueAfterIntro);
			AddToExecutionOrderLog("BlockContinueAfterIntro");

			EnableEyeCalibrationTrigger(false);

			StartTrial();
		}

		/// <summary>Called from this manager. </summary>
		void BlockContinueAfterOutro(eParam e) {
			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, BlockContinueAfterOutro);
			AddToExecutionOrderLog("BlockContinueAfterOutro");

			BlockCheckAndContinue();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region UXF TRIALS

		/// <summary>catching first button press of user </summary>
		void OnEvStartFirstTrial(eParam e) {
			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, OnEvStartFirstTrial);

			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateTrialProgress, new eParam(new int[] { Session.instance.currentTrialNum, Session.instance.Trials.Count() }));

			Session.instance.BeginNextTrial();
		}

		/// <summary>Called from UXF session. Begin setting things up for the trial that is about to start </summary>
		void OnTrialBeginUXF(Trial newTrial) {
			AddToExecutionOrderLog("OnTrialBeginUXF");

			bool isNewBlock = (Session.instance.currentBlockNum != _activeSessionBlockNum) && (Session.instance.currentBlockNum <= Session.instance.blocks.Count);

			if (isNewBlock) {
				BlockStart();
			}
			else {
				StartTrial();
				EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateProgressInfo, new eParam(Session.instance.CurrentBlock.settings.GetString("block_name")));
			}
		}

		/// <summary>Called from UXF session. Checks if to call NextTrial, should start a BREAK before next Block, or End the Session </summary>
		void OnTrialEndUXF(Trial endedTrial) {
			AddToExecutionOrderLog("OnTrialEnd");
			SaveCustomDataTables();

			taskBlocks[Session.instance.currentBlockNum - 1].OnEndTrial();

			// Are we ending?
			if (Session.instance.isEnding)
				return;

			// Is there a PAUSE requested right now?
			if (_isPauseRequested) {
				_isPauseRequested = false;

				if (endedTrial == Session.instance.LastTrial)
					return;

				AddToExecutionOrderLog("Injected SessionBreak");
				SessionPause();
				return;
			}

			// Reached last trial in a block?
			if (Session.instance.CurrentBlock.lastTrial != endedTrial) { // NO
				Session.instance.BeginNextTrialSafe();
				return;
			}
			else {
				BlockEnd(); // YES
				return;
			}
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region STATEMACHINE TRIAL STEPS

		void StartTrial() {
			AddToLog("StartTrial");

			taskBlocks[Session.instance.currentBlockNum - 1].OnStartTrial();
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateTrialProgress, new eParam(new int[] { Session.instance.currentTrialNum, Session.instance.Trials.Count() }));
			_currentStep = -1;
			NextTrialStep();
		}

		/// <summary>Called after the task sequence is done </summary>
		void EndTrial() {
			AddToLog("Trial Steps DONE");
			Session.instance.EndCurrentTrial(); // tells UXF to end this trial and fire the event that follows
		}

		/// <summary>Call next step in the trial with delay.</summary>
		/// <param name="duration">Time to wait before proceeding. Expects float</param>
		public void ProceedWithDelay(float duration) {

			if (_proceedTimer != null) StopCoroutine(_proceedTimer); // Kill timer, if any
			_proceedTimer = StartCoroutine("ProceedTimer", duration);
		}

		/// <summary>Coroutine as timer as we can kill that to avoid delayed calls in the statemachine</summary>
		IEnumerator ProceedTimer(float duration) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvStartTimer, new eParam(duration));
			yield return new WaitForSecondsRealtime(duration);

			EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, OnEvProceed);
			Proceed();
		}


		/// <summary>Call next step in the trial.</summary>
		void NextTrialStep() {
			if (ShowLog) AddToLog("Nextstep >");

			if (_proceedTimer != null) {
				StopCoroutine(_proceedTimer); // Kill timer, if any
				EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvStopTimer);
			}

			// In case OnProceed was triggered outside of the button
			EnableProceedButton(false);

			_currentStep++;

			if (_currentStep < _activeEBlock.trialSteps.Count) {
				InBetweenSteps();
				EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateStepProgress, new eParam(new int[] { _currentStep, _activeEBlock.trialSteps.Count }));
				_activeEBlock.trialSteps[_currentStep].Invoke();

			}
			else EndTrial();

			//if (currentStep < taskBlocks[Session.instance.CurrentBlock.number - 1].trialSteps.Count) {
			//	InBetweenSteps();

			//	// update progress
			//	EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateStepProgress, new eParam(new int[] { currentStep, taskBlocks[Session.instance.currentBlockNum - 1].trialSteps.Count }));
			//	taskBlocks[Session.instance.currentBlockNum - 1].trialSteps[currentStep].Invoke();
			//}
			//else EndTrial();
		}

		/// <summary>In Between to steps of the trial, we might want to clean things up a bit.</summary>
		void InBetweenSteps() {
			taskBlocks[Session.instance.currentBlockNum - 1].OnBetweenSteps(); // In Between to steps of the trial, we might want to clean things up a bit.
			MessagePanelInVR.Instance.HidePanel();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region PAUSE

		/// <summary>Called from this manager. Invokes onSessionBreak event and starts listener to EvProceed event</summary>
		void SessionPause() {
			AddToExecutionOrderLog("SessionPaused");

			EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, SessionResumeAfterBreak);
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateProgressInfo, new eParam("Pause"));

			OnSessionPaused.Invoke();

			EnableProceedButton(true);
			EnablePauseButton(false);
			EnableEyeCalibrationTrigger(true);
		}

		/// <summary>Called from EvProceed event. Stops listener, invokes onSessionResume event and calls UXF BeginNextTrial. </summary>
		void SessionResumeAfterBreak(eParam e) {
			AddToExecutionOrderLog("SessionResume");

			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, SessionResumeAfterBreak);

			EnableEyeCalibrationTrigger(false);

			Session.instance.BeginNextTrialSafe();
			//Session.instance.Invoke("BeginNextTrialSafe", 0.5f);
		}

#endregion  // -------------------------------------------------------------------------------------------------------------------------------
#region LOGGING  

		/// <summary>Converts given data to a UXF Table, and stores the data to disk linked to the active trial at the time</summary>
		/// <param name="headers">Headers of the data</param>
		/// <param name="values">Data as List<string>[]</param>
		/// <param name="filename">Name to store the data with</param>
		public void ConvertAndSaveDataToUXF(string[] headers, List<string[]> values, string filename) {
			var UXFheaders = headers;
			var data = new UXF.UXFDataTable(UXFheaders);

			foreach (string[] valuerow in values) {
				UXFDataRow newRow = new UXFDataRow();
				for (int s = 0; s < valuerow.Length; s++) {
					newRow.Add((UXFheaders[s], valuerow[s]));
				}
				data.AddCompleteRow(newRow);
			}

			// Save data
			Session.instance.CurrentTrial.SaveDataTable(data, filename);
		}

		/// <summary>Converts given data to a UXF Table, and stores the data to disk linked to the active trial at the time</summary>
		/// <param name="headers">Headers of the data</param>
		/// <param name="values">Data as List<int></param>
		/// <param name="filename">Name to store the data with</param>
		public void ConvertAndSaveDataToUXF(string[] headers, List<int> values, string filename) {
			List<string[]> converted = new List<string[]>();

			for (int i = 0; i < values.Count; i++) {
				converted.Add(new string[] { (i + 1).ToString(), values[i].ToString() });
			}

			ConvertAndSaveDataToUXF(headers, converted, filename);
		}


		private void AddToExecutionOrderLog(string description) {
			AddToLog(description);
			UXF.UXFDataRow newRow = new UXFDataRow();
			newRow.Add(("timestamp", Time.time)); // Log timestamp
			newRow.Add(("executed", description));
			_executionOrderLog.AddCompleteRow(newRow);
		}

		/// <summary>
		/// Saves a marker with a timestamp
		/// </summary>
		/// <param name="annotation">Annotation to store</param>
		public void SendMarker(string annotation) {
			// Log it in the UXF way
			UXF.UXFDataRow newRow = new UXFDataRow();
			newRow.Add(("timestamp", Time.realtimeSinceStartup)); // Log timestamp
			newRow.Add(("annotation", annotation));
			_markerLog.AddCompleteRow(newRow);

			EventManager.TriggerEvent(eDIA.Events.DataHandlers.EvSendMarker, new eParam(annotation));
		}

		private void AddToLog(string _msg) {

			if (ShowLog)
				eDIA.LogUtilities.AddToLog(_msg, "EXP", LogColor);
		}


#endregion  // -------------------------------------------------------------------------------------------------------------------------------
#region HELPERS

		public void AddToTrialResults(string key, string value) {
			Session.instance.CurrentTrial.result[key] = value;
		}

		void AddXRrigTracking() {
			Session.instance.trackedObjects.Add(XRManager.Instance.XRCam.GetComponent<Tracker>());
			Session.instance.trackedObjects.Add(XRManager.Instance.XRRight.GetComponent<Tracker>());
			Session.instance.trackedObjects.Add(XRManager.Instance.XRLeft.GetComponent<Tracker>());
		}

		void SaveCustomDataTables() {
			Session.instance.SaveDataTable(_executionOrderLog, "executionOrder");
			Session.instance.SaveDataTable(_markerLog, "markerLog");
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------



	}

}