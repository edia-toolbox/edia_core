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

	  public EState state = EState.IDLE;
	  EState prevState = EState.IDLE;

	  [Header("Editor Settings")]
	  public bool showLog = false;
	  public Color taskColor = Color.green;

	  [Space(20)]
	  public List<TaskBlock> taskBlocks = new();
	  public List<EBlock> Blocks = new();

	  [Space(20)]
	  [Header("Event hooks\n\nOptional event hooks to use in your task")]
	  public UnityEvent OnSessionStart = null;
	  public UnityEvent OnSessionBreak = null;
	  public UnityEvent OnSessionEnd = null;

	  /// The config instance that holds current experimental configuration
	  [HideInInspector]
	  public ExperimentConfig experimentConfig = new();
	  [HideInInspector]
	  public TaskConfig taskConfig = new TaskConfig();

	  // Helpers
	  [Space(20)]
	  int activeBlockUXF = 0;
	  bool isPauseRequested = false;

	  // UXF Logging
	  UXFDataTable executionOrderLog = new("timestamp", "executed");
	  UXFDataTable markerLog = new("timestamp", "annotation");

	  /// <summary> Currently active step number. </summary>
	  [HideInInspector]
	  public int currentStepNum = -1;

	  Coroutine proceedTimer = null;

	  #endregion // -------------------------------------------------------------------------------------------------------------------------------
	  #region MONO METHODS

	  void Awake() {
		//DontDestroyOnLoad(this);

		EventManager.showLog = true;

		if (!ValidateBlockAssetList()) {
		    Debug.LogError("Block list contains invalid naming");
		    return;
		}

		// Hard reference statemachine links between UXF and EXP
		Session.instance.onSessionBegin.AddListener(OnSessionBeginUXF);
		Session.instance.onSessionEnd.AddListener(OnSessionEndUXF);
		Session.instance.onTrialBegin.AddListener(OnTrialBeginUXF);
		Session.instance.onTrialEnd.AddListener(OnTrialEndUXF);

		// Disable task block script before anything starts to run
		//foreach (TaskBlock t in taskBlocks) {
		//    t.enabled = false;
		//}

		//EventManager.StartListening(eDIA.Events.Config.EvSetExperimentConfig, OnEvSetExperimentConfig);
		//EventManager.StartListening(eDIA.Events.Config.EvSetTaskConfig, OnEvSetTaskConfig);
		EventManager.StartListening(eDIA.Events.StateMachine.EvStartExperiment, OnEvStartExperiment);
		EventManager.StartListening(eDIA.Events.Core.EvQuitApplication, OnEvQuitApplication);

		EventManager.showLog = showLog;
	  }

	  void OnDestroy() {
		//EventManager.StopListening(eDIA.Events.Config.EvSetExperimentConfig, OnEvSetExperimentConfig);
		//EventManager.StopListening(eDIA.Events.Config.EvSetTaskConfig, OnEvSetTaskConfig);
		EventManager.StopListening(eDIA.Events.StateMachine.EvStartExperiment, OnEvStartExperiment);
		EventManager.StopListening(eDIA.Events.StateMachine.EvPauseExperiment, OnEvPauseExperiment);
		EventManager.StopListening(eDIA.Events.Core.EvQuitApplication, OnEvQuitApplication);
	  }

	  private bool ValidateBlockAssetList() {
		bool _succes = true;
		foreach (EBlock tb in Blocks) {
		    string firstPart = tb.name.Split('_')[0];
		    if ((firstPart != "Break") && (firstPart != "Task")) {
			  _succes = false;
		    }

		    tb.gameObject.SetActive(false);
		}
		return _succes;
	  }

	  private void Update() {
		if (state != prevState) {
		    prevState = state;
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
		  experimentConfig.GetParticipantDetailsAsDict(),
		  new UXF.Settings(taskConfig.GetTaskSettingsAsDict())
		);

	  }

	  void OnEvStartExperiment(eParam e) {
		EventManager.StopListening(eDIA.Events.StateMachine.EvStartExperiment, OnEvStartExperiment);

		StartExperiment();
	  }

	  /// <summary>Sets the PauseExperiment flag to true and logs the call for an extra break</summary>
	  void OnEvPauseExperiment(eParam e) {
		AddToExecutionOrderLog("InjectedSessionBreakCall");
		isPauseRequested = true;
	  }

	  void OnEvQuitApplication(eParam obj) {
		AddToLog("Quiting..");
		Application.Quit();
	  }


	  public void EnablePauseButton(bool _onOff) {
		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PAUSE", _onOff.ToString() }));
		EventManager.StartListening("EvPauseExperiment", OnEvPauseExperiment);
	  }

	  public void WaitOnProceed() {
		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "TRUE" }));
		EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, OnEvProceed);
	  }

	  void OnEvProceed(eParam e) {
		Proceed();
	  }

	  public void Proceed() {
		EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, OnEvProceed); // stop listening to avoid doubleclicks
		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "false" })); // disable button, as OnEvProceed might have come from somewhere else than the button itself

		NextTrialStep();
	  }

	  /// <summary> Set system open for calibration call from event or button</summary>
	  /// <param name="onOff"></param>
	  public void EnableEyeCalibrationTrigger(bool _onOff) {
		EventManager.TriggerEvent(eDIA.Events.Eye.EvEnableEyeCalibrationTrigger, new eParam(_onOff));
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
	  #region UXF RELATED HELPERS

	  public void AddToTrialResults(string key, string value) {
		Session.instance.CurrentTrial.result[key] = value;
	  }

	  void AddXRrigTracking() {
		Session.instance.trackedObjects.Add(XRManager.Instance.XRCam.GetComponent<Tracker>());
		Session.instance.trackedObjects.Add(XRManager.Instance.XRRight.GetComponent<Tracker>());
		Session.instance.trackedObjects.Add(XRManager.Instance.XRLeft.GetComponent<Tracker>());
	  }

	  void SaveCustomDataTables() {
		Session.instance.SaveDataTable(executionOrderLog, "executionOrder");
		Session.instance.SaveDataTable(markerLog, "markerLog");
	  }

	  #endregion // -------------------------------------------------------------------------------------------------------------------------------
	  #region STATEMACHINE UXF SESSION

	  /// <summary>Start of the UXF session. </summary>
	  void OnSessionBeginUXF(Session session) {
		OnSessionStart?.Invoke();

		state = EState.RUNNING;
		AddToExecutionOrderLog("OnSessionBegin");
		EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, OnEvStartFirstTrial);

		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateBlockProgress, new eParam(new int[] { Session.instance.currentBlockNum, Session.instance.blocks.Count }));
		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateSessionSummary, new eParam(experimentConfig.GetExperimentSummary()));
		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateProgressInfo, new eParam("Welcome"));

		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "true" }));

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
		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "false" }));

		EnablePauseButton(false);
	  }


	  #endregion // -------------------------------------------------------------------------------------------------------------------------------
	  #region STATEMACHINE BLOCKS

	  void BlockStart() {
		AddToLog("Block Start");

		// Disable old block
		if (Session.instance.currentBlockNum - 1 != 0)
		    taskBlocks[Session.instance.currentBlockNum - 2].enabled = false;

		// enable new block
		taskBlocks[Session.instance.currentBlockNum - 1].enabled = true;
		taskBlocks[Session.instance.currentBlockNum - 1].OnBlockStart();

		// Set new activeBlockUXF value
		activeBlockUXF = Session.instance.currentBlockNum;

		// Update block progress
		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateBlockProgress, new eParam(new int[] { Session.instance.currentBlockNum, Session.instance.blocks.Count }));

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
		    EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateProgressInfo, new eParam(Session.instance.CurrentBlock.settings.GetString("block_name")));
		}
	  }


	  void BlockEnd() {
		AddToLog("Block End");
		taskBlocks[Session.instance.currentBlockNum - 1].OnBlockEnd();

		// Check for block outro flag
		bool hasOutro = Session.instance.CurrentBlock.settings.GetString("outro") != string.Empty;

		// Inject introduction step or continue UXF sequence
		if (hasOutro) {
		    EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, BlockContinueAfterOutro); // listener as it event call can come from any script
		    ShowMessageToUser(Session.instance.CurrentBlock.settings.GetString("outro"), "Block Outro");
		    taskBlocks[Session.instance.currentBlockNum - 1].OnBlockOutro();
		}
		else {
		    BlockCheckAndContinue();
		}
	  }

	  void BlockCheckAndContinue() {
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
	  void ShowMessageToUser(string msg, string description) {
		AddToExecutionOrderLog("ShowMessageToUser");

		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "true" }));
		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateProgressInfo, new eParam("Block Info"));

		EnablePauseButton(false);
		EnableEyeCalibrationTrigger(true);

		if (MessagePanelInVR.Instance != null)
		    MessagePanelInVR.Instance.ShowMessage(msg);
		else Debug.LogError("No MessagePanelInVR instance found");
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
	  #region STATEMACHINE UXF TRIAL

	  /// <summary>catching first button press of user </summary>
	  void OnEvStartFirstTrial(eParam e) {
		EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, OnEvStartFirstTrial);

		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateTrialProgress, new eParam(new int[] { Session.instance.currentTrialNum, Session.instance.Trials.Count() }));

		Session.instance.BeginNextTrial();
	  }

	  /// <summary>Called from UXF session. Begin setting things up for the trial that is about to start </summary>
	  void OnTrialBeginUXF(Trial newTrial) {
		AddToExecutionOrderLog("OnTrialBeginUXF");

		bool isNewBlock = (Session.instance.currentBlockNum != activeBlockUXF) && (Session.instance.currentBlockNum <= Session.instance.blocks.Count);

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
		}
		else {
		    BlockEnd(); // YES
		    return;
		}
	  }


	  #endregion // -------------------------------------------------------------------------------------------------------------------------------
	  #region STATEMACHINE CURRENT TRIAL STEPS

	  void StartTrial() {
		AddToLog("StartTrial");
		taskBlocks[Session.instance.currentBlockNum - 1].OnStartTrial();

		// Update trial progress
		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateTrialProgress, new eParam(new int[] { Session.instance.currentTrialNum, Session.instance.Trials.Count() }));

		currentStepNum = -1;

		// Fire up the task state machine to run the steps of the trial.
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
		if (proceedTimer != null) StopCoroutine(proceedTimer); // Kill timer, if any
		proceedTimer = StartCoroutine("ProceedTimer", duration);
	  }

	  /// <summary>Coroutine as timer as we can kill that to avoid delayed calls in the statemachine</summary>
	  IEnumerator ProceedTimer(float duration) {
		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvStartTimer, new eParam(duration));
		yield return new WaitForSecondsRealtime(duration);

		Proceed();
	  }

	  /// <summary>Call next step in the trial.</summary>
	  void NextTrialStep() {
		if (showLog) AddToLog("Nextstep >");

		if (proceedTimer != null) {
		    StopCoroutine(proceedTimer); // Kill timer, if any
		    EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvStopTimer);
		}

		// In case OnProceed was triggered outside of the button
		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "FALSE" }));

		currentStepNum++;

		if (currentStepNum < taskBlocks[Session.instance.CurrentBlock.number - 1].trialSteps.Count) {
		    InBetweenSteps();

		    // update progress
		    EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateStepProgress, new eParam(new int[] { currentStepNum, taskBlocks[Session.instance.currentBlockNum - 1].trialSteps.Count }));
		    taskBlocks[Session.instance.currentBlockNum - 1].trialSteps[currentStepNum].Invoke();
		}
		else EndTrial();
	  }

	  /// <summary>In Between to steps of the trial, we might want to clean things up a bit.</summary>
	  void InBetweenSteps() {
		taskBlocks[Session.instance.currentBlockNum - 1].OnBetweenSteps(); // In Between to steps of the trial, we might want to clean things up a bit.
		MessagePanelInVR.Instance.HidePanel();
	  }


	  #endregion // -------------------------------------------------------------------------------------------------------------------------------
	  #region BREAK

	  /// <summary>Called from this manager. Invokes onSessionBreak event and starts listener to EvProceed event</summary>
	  void SessionBreak() {
		AddToExecutionOrderLog("SessionBreak");

		EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, SessionResumeAfterBreak);
		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateProgressInfo, new eParam("Break"));

		OnSessionBreak.Invoke();

		EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "true" }));
		// EnableExperimentProceed(true);
		EnablePauseButton(false);
		EnableEyeCalibrationTrigger(true);
	  }

	  /// <summary>Called from EvProceed event. Stops listener, invokes onSessionResume event and calls UXF BeginNextTrial. </summary>
	  void SessionResumeAfterBreak(eParam e) {
		AddToExecutionOrderLog("SessionResume");

		EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, SessionResumeAfterBreak);

		EnableEyeCalibrationTrigger(false);

		//? Why the delay here ?
		Session.instance.Invoke("BeginNextTrialSafe", 0.5f);
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
		executionOrderLog.AddCompleteRow(newRow);
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
		markerLog.AddCompleteRow(newRow);

		EventManager.TriggerEvent(eDIA.Events.DataHandlers.EvSendMarker, new eParam(annotation));
	  }

	  private void AddToLog(string _msg) {

		if (showLog)
		    eDIA.LogUtilities.AddToLog(_msg, "EXP", taskColor);
	  }


	  #endregion  // -------------------------------------------------------------------------------------------------------------------------------
    }

}