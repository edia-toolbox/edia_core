using Edia.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UXF;

namespace Edia {

#region DECLARATIONS

	/// <summary>
	/// 
	/// </summary>
    public class Experiment : Singleton<Experiment> {

		public enum XState {
			IDLE,
			RUNNING,
			WAITINGONPROCEED,
			PAUSED,
			ENDED
		}

		public XState State = XState.IDLE;
		XState _prevState = XState.IDLE;

		[Header("Editor Settings")]
		public bool ShowLog = false;
		public Color LogColor = Color.green;

		[Header("Experiment Settings")]
		[Tooltip("Enable Position&Rotation tracker from UXF which stores data to session folder. !Might have impact on FPS with long trials.")]
		[HelpBox("<size=14><color=#00ee30><b>INFO:</b></color> UXF Tracking</size>\nEnable Position&Rotation tracker from UXF which stores data to session folder. \nMight have impact on FPS with long trials!", HelpBoxMessageType.Warning)]
		public bool TrackXrRigWithUxf = false;

		[Space(20)]
		[Header("Experiment")]
		[HelpBox("<size=14><color=#00ee30><b>XBlock Executers</b></color></size>\nList of gameobjects containing the functional Xblock code. \nNaming convenction: [type]-[subtype]", HelpBoxMessageType.None)]
		public List<XBlock> XBlockExecuters = new();

		[Space(20)]
		[Header("Event hooks\n\nOptional event hooks to use in your task")]
		public UnityEvent OnSessionStart = null;
		public UnityEvent OnSessionPaused = null;
		public UnityEvent OnSessionEnd = null;

		// Fields
		int _activeSessionBlockNum = 0;
		int _currentStep = -1;
		bool _isPauseRequested = false;
		XBlock _activeXBlock;
		Coroutine _proceedTimer = null;

		// UXF Logging
		UXFDataTable _executionOrderLog = new("timestamp", "executed");
		UXFDataTable _markerLog = new("timestamp", "annotation");
		
		#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region MONO METHODS

		void Awake() {
			EventManager.showLog = ShowLog;
		}

		void OnDestroy() {
			EventManager.StopListening(Edia.Events.StateMachine.EvStartExperiment, OnEvStartExperiment);
			EventManager.StopListening(Edia.Events.StateMachine.EvPauseExperiment, OnEvPauseExperiment);
			EventManager.StopListening(Edia.Events.Core.EvQuitApplication, OnEvQuitApplication);
		}

		void Start() {

			XBlockNamesToLower();

			if(!IsValid()) 
				return;

			EnableAllXBlocks(false);

			// Hard reference statemachine links between UXF and EXP
			Session.instance.onSessionBegin.AddListener(OnSessionBeginUXF);
			Session.instance.onSessionEnd.AddListener(OnSessionEndUXF);
			Session.instance.onTrialBegin.AddListener(OnTrialBeginUXF);
			Session.instance.onTrialEnd.AddListener(OnTrialEndUXF);

			EventManager.StartListening(Edia.Events.Core.EvQuitApplication, OnEvQuitApplication);
			EventManager.StartListening(Edia.Events.StateMachine.EvStartExperiment, OnEvStartExperiment);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region CHECKS

        bool IsValid() {
			bool isValid = true;
			List<string> msgs = new();

			// Are there executers?
			if (XBlockExecuters == null || XBlockExecuters.Count == 0) {
				isValid = false;
				msgs.Add("XBLock Executers list is empty!");
			}

			// Are there executers with the same name?
			var names = XBlockExecuters.Select(g => g.name);
            if (names.Count() != names.Distinct().Count()) {
				msgs.Add("All XBlock Executers need unique names!");
				isValid = false;
			}

			// Are the gameobjects in Experiment.blocks properly named? <type>_<subtype>
			foreach (XBlock g in XBlockExecuters) {
				if (!Regex.IsMatch(g.name, @"^[a-z0-9]+-[a-z0-9\-'().,_]+$")) {
					msgs.Add($"Invalid gameobject (XBlock Executer) naming format found in: <b>{g.name}</b>; must adhere to: <type>-<subtype>");
					isValid = false;
				}
			}

			if (!isValid) { 
				foreach(string s in msgs)
					Debug.LogErrorFormat(s);

				// TODO: Think of a way to alert the user in a build
				//ShowMessageToExperimenter(msg, false);
			}

			return isValid;
		}

		void XBlockNamesToLower() {
			foreach (XBlock g in XBlockExecuters) {
				g.name = g.name.ToLower();
			}
			return;
		}

		void EnableAllXBlocks (bool onOff) {
			foreach (XBlock xb in XBlockExecuters) {
				xb.enabled = onOff;
				xb.gameObject.SetActive(onOff);
			}
		}

		/// <summary>
		/// Retuns true/false depending if the gameobject is found belonging to the assetId given
		/// </summary>
		/// <param name="assetId"></param>
		/// <returns></returns>
		public bool IsXblockExecuterListed(string assetId) {
			return XBlockExecuters.Any(go => go.name == assetId);
		}

		void Reset() {
			State = XState.IDLE;
			_activeSessionBlockNum = 0;
			_currentStep = -1;
			_isPauseRequested = false;
			_prevState = State;
		}

		void Update() {
			if (State != _prevState) {
				_prevState = State;
			}
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region INFORMATION UPDATES

		/// <summary>
		/// Will update the progress textfield in the control panel.
		/// </summary>
		/// <param name="info">Text to show, excludes string before first '_' char</param>
		void UpdateProgressStatus (string info) {
			List<string> infos = new();

			if (info.Contains('_')) {
				infos = info.Split('_').ToList();
				infos.RemoveAt(0);
			}
			else infos.Add(info);

			EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateProgressStatus, new eParam(StringTools.CombineToOneString(infos.ToArray())));
		}
		void UpdateSessionSummary() {
			EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateSessionSummary, new eParam(SessionSettings.sessionInfo.GetSessionSummary()));
		}

		void UpdateBlockProgress() {
			EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateBlockProgress, new eParam(new int[] { Session.instance.currentBlockNum, Session.instance.blocks.Count }));
		}

		void UpdateTrialProgress () {
			if (Session.instance.InTrial)
				EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateTrialProgress, new eParam(new int[] { Session.instance.CurrentTrial.numberInBlock, Session.instance.CurrentBlock.trials.Count() }));
		}

		void UpdateStepProgress () {
			EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateStepProgress, new eParam(new int[] { _currentStep, _activeXBlock.trialSteps.Count }));
		}

		/// <summary>Show a message to the VR user in an overlayed panel. Default: Proceed button ON</summary>
		/// <param name="msg">Message to show</param>
		public void ShowMessageToUser (string msg) {
			ShowMessageToUserGeneric();
			MessagePanelInVR.Instance.ShowMessage(msg);
		}

		/// <summary>Show a list of messages to the VR user, one at a time, in an overlayed panel.</summary>
		/// <param name="msg">Messages to show</param>
		public void ShowMessageToUser (List<string> msgs) {
			ShowMessageToUserGeneric();
			MessagePanelInVR.Instance.ShowMessage(msgs);
		}

		/// <summary>Show a message to the VR user in an overlayed panel for a certain duration.</summary>
		/// <param name="msg">Message to show</param>
		/// <param name="duration">Time to show the message</param>
		public void ShowMessageToUser (string msg, float duration) {
			ShowMessageToUserGeneric();
			MessagePanelInVR.Instance.ShowMessage (msg, duration);
		}

		/// <summary>Show a message to the VR user in an overlayed panel. </summary>
		/// <param name="msg">Message to show</param>
		/// <param name="showButton">False = no button</param>
		public void ShowMessageToUser(string msg, bool showButton) {
			ShowMessageToUserGeneric();
			MessagePanelInVR.Instance.ShowMessage(msg);
			
			if (!showButton)
				MessagePanelInVR.Instance.HideMenu();
		}

		void ShowMessageToUserGeneric () {
			AddToExecutionOrderLog("ShowMessageToUser");
			EnableProceedButton(true);
			EnablePauseButton(false);
			EnableEyeCalibrationTrigger(true);
		}

		void HideMessagePanelButtons () {
			MessagePanelInVR.Instance.HideMenu();
		}

		/// <summary>
		/// Shows a message in the controlpanel.
		/// </summary>
		/// <param name="msg">Message to show</param>
		/// <param name="autohide">Autohide the message</param>
		public void ShowMessageToExperimenter (string msg, bool autohide) {
			EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam(msg, autohide));
		}

		/// <summary>Shows a message in the controlpanel. Autohides the panel.</summary>
		/// <param name="msg">Message to show</param>
		public void ShowMessageToExperimenter(string msg) {
			EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam(msg, true));
		}


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EXPERIMENT CONTROL

		/// <summary>Starts the experiment</summary>
		void StartExperiment() {
			ConfigureXRrigTracking();

			Session.instance.Begin(
			  SessionSettings.sessionInfo.experiment == string.Empty ? "N.A." : SessionSettings.sessionInfo.experiment,
			  SessionSettings.sessionInfo.GetParticipantID(),
			  SessionSettings.sessionInfo.session_number,
			  SessionSettings.sessionInfo.GetParticipantDetailsAsDict()
			);

			UpdateProgressStatus("Session started");
			EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateBlockProgress, new eParam(new int[] { 0, Session.instance.blocks.Count }));
			EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateTrialProgress, new eParam(new int[] { 0, 0 }));
		}

		void OnEvStartExperiment(eParam e) {
			EventManager.StopListening(Edia.Events.StateMachine.EvStartExperiment, OnEvStartExperiment);

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
			EventManager.TriggerEvent(Edia.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PAUSE", _onOff.ToString() }));
			EventManager.StartListening("EvPauseExperiment", OnEvPauseExperiment);
		}

		/// <summary> Set system open for calibration call from event or button</summary>
		/// <param name="onOff"></param>
		public void EnableEyeCalibrationTrigger(bool _onOff) {
			EventManager.TriggerEvent(Edia.Events.Eye.EvEnableEyeCalibrationTrigger, new eParam(_onOff));
		}


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region STATEMACHINE PROCEED

		void EnableProceedButton(bool onOff) {
			EventManager.TriggerEvent(Edia.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", onOff ? "true" : "false" }));
		}

		/// <summary>Enables Controller 'Proceed' button, and waits OnEvProceed event</summary>
		public void WaitOnProceed() {
			State = XState.WAITINGONPROCEED;

			EnableProceedButton(true);
			EventManager.StartListening(Edia.Events.StateMachine.EvProceed, OnEvProceed);
		}

		/// <summary>Triggers OnEvProceed event</summary>
		public void Proceed() {
			EventManager.TriggerEvent(Edia.Events.StateMachine.EvProceed);
		}

		void OnEvProceed(eParam e) {
			EventManager.StopListening(Edia.Events.StateMachine.EvProceed, OnEvProceed);
			EnableProceedButton(false);

			Continue();
		}

		void Continue() {
			State = XState.RUNNING;

			EventManager.StopListening(Edia.Events.StateMachine.EvProceed, OnEvProceed);
			EnableProceedButton(false);			

			NextTrialStep();
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region STATEMACHINE UXF SESSION

		/// <summary>Start of the UXF session. </summary>
		void OnSessionBeginUXF(Session session) {
			OnSessionStart?.Invoke();

			State = XState.RUNNING;
			_activeSessionBlockNum = 0;

			AddToExecutionOrderLog("OnSessionBegin");
			EventManager.StartListening(Edia.Events.StateMachine.EvProceed, OnEvStartFirstTrial);

			UpdateBlockProgress();
			UpdateSessionSummary();
			UpdateProgressStatus("Welcome");

			EnableProceedButton(true);

			// eye calibration option enabled
			EnableEyeCalibrationTrigger(true);
		}

		/// <summary>Called from UXF session. </summary>
		void OnSessionEndUXF(Session session) {
			OnSessionEnd?.Invoke();

			EnableAllXBlocks(false);

			AddToExecutionOrderLog("OnSessionEndUXF");

			UpdateProgressStatus("End");
			EnableProceedButton(false);

			EnablePauseButton(false);

			HideMessagePanelButtons();

			EventManager.TriggerEvent(Edia.Events.StateMachine.EvSessionEnded, null);

			Reset();
		}

		/// <summary>Done with all trial, clean up and call UXF to end this session</summary>
		void FinalizeSession() {
			AddToLog("FinalizeSession");

			// clean
			UpdateProgressStatus("Finalizing Session");
			Session.instance.End();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region STATEMACHINE BLOCKS

		void BlockStart() {
			AddToLog("Block Start");

			// Set new storedBlockNum value
			_activeSessionBlockNum = Session.instance.currentBlockNum;
			_activeXBlock = XBlockExecuters[XBlockExecuters.FindIndex(x => x.name == Session.instance.CurrentBlock.settings.GetString("_assetId"))];
			_activeXBlock.enabled = true;
			_activeXBlock.gameObject.SetActive(true);
			_activeXBlock.OnBlockStart();

			// Update block progress
			UpdateBlockProgress();

			// Check for block introduction flag
			bool hasIntro = Session.instance.CurrentBlock.settings.GetStringList("_start").Count > 0;

			// Inject introduction step or continue UXF sequence
			if (hasIntro) {
				EventManager.StartListening(Edia.Events.StateMachine.EvProceed, BlockContinueAfterIntro); // listener as it event call can come from any script
				ShowMessageToUser (Session.instance.CurrentBlock.settings.GetStringList("_start"));
				UpdateProgressStatus(Session.instance.CurrentBlock.settings.GetString("blockId") + " Introduction");
			}
			else {
				StartTrial();
				UpdateProgressStatus(Session.instance.CurrentBlock.settings.GetString("blockId"));
			}
		}

		void BlockEnd() {
			_activeXBlock.OnBlockEnd();

			// Check for block outro flag
			bool hasOutro = Session.instance.CurrentBlock.settings.GetStringList("_end").Count > 0;

			if (hasOutro) {
				EventManager.StartListening(Edia.Events.StateMachine.EvProceed, BlockContinueAfterOutro); // listener as it event call can come from any script
				EnableProceedButton(true);
				ShowMessageToUser(Session.instance.CurrentBlock.settings.GetStringList("_end"));
				UpdateProgressStatus("Block Outro");
				_activeXBlock.OnBlockOutro();
			}
			else {
				BlockCheckAndContinue();
			}
		}

		void BlockCheckAndContinue() {

			_activeXBlock.enabled = false;
			_activeXBlock.gameObject.SetActive(false);

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
			EventManager.StopListening(Edia.Events.StateMachine.EvProceed, BlockContinueAfterIntro);
			AddToExecutionOrderLog("BlockContinueAfterIntro");

			UpdateProgressStatus(Session.instance.CurrentBlock.settings.GetString("blockId"));
			EnableEyeCalibrationTrigger(false);

			StartTrial();
		}

		/// <summary>Called from this manager. </summary>
		void BlockContinueAfterOutro(eParam e) {
			EventManager.StopListening(Edia.Events.StateMachine.EvProceed, BlockContinueAfterOutro);
			AddToExecutionOrderLog("BlockContinueAfterOutro");

			BlockCheckAndContinue();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region UXF TRIALS

		/// <summary>Catching first button press of user </summary>
		void OnEvStartFirstTrial(eParam e) {
			EventManager.StopListening(Edia.Events.StateMachine.EvProceed, OnEvStartFirstTrial);
			UpdateTrialProgress();

			Session.instance.BeginNextTrial();
		}

		/// <summary>Called from UXF session. Begin setting things up for the trial that is about to start </summary>
		void OnTrialBeginUXF(Trial newTrial) {
			AddToExecutionOrderLog("OnTrialBeginUXF");

			XRManager.Instance.ShowVRInstantly(); // TODO: This is now assuming that we want VR visable at the start of each trial.

			bool isNewBlock = (Session.instance.currentBlockNum != _activeSessionBlockNum) && (Session.instance.currentBlockNum <= Session.instance.blocks.Count);

			if (isNewBlock) {
				BlockStart();
			}
			else {
				StartTrial();
				UpdateProgressStatus(Session.instance.CurrentBlock.settings.GetString("blockId"));
			}
		}

		/// <summary>Called from UXF session. Checks if to call NextTrial, should start a BREAK before next Block, or End the Session </summary>
		void OnTrialEndUXF(Trial endedTrial) {
			AddToExecutionOrderLog("OnTrialEnd");
			SaveCustomDataTables();

			_activeXBlock.OnEndTrial();

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

			_activeXBlock.OnStartTrial();
			UpdateTrialProgress();
			
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
			_proceedTimer = StartCoroutine(ProceedTimer(duration));
		}

		/// <summary>Coroutine as timer as we can kill that to avoid delayed calls in the statemachine</summary>
		IEnumerator ProceedTimer(float duration) {
			EventManager.TriggerEvent(Edia.Events.ControlPanel.EvStartTimer, new eParam(duration));
			yield return new WaitForSecondsRealtime(duration);

			Proceed();
		}


		/// <summary>Call next step in the trial.</summary>
		void NextTrialStep() {
			if (ShowLog) AddToLog("Nextstep >");

			if (_proceedTimer != null) {
				StopCoroutine(_proceedTimer); // Kill timer, if any
				EventManager.TriggerEvent(Edia.Events.ControlPanel.EvStopTimer);
			}

			// In case OnProceed was triggered outside of the button
			EnableProceedButton(false);

			_currentStep++;

			if (_currentStep < _activeXBlock.trialSteps.Count) {
				InBetweenSteps();
				UpdateStepProgress();
				_activeXBlock.trialSteps[_currentStep].Invoke();

			}
			else EndTrial();
		}

		/// <summary>In Between to steps of the trial, we might want to clean things up a bit.</summary>
		void InBetweenSteps() {
			_activeXBlock.OnBetweenSteps(); // In Between to steps of the trial, we might want to clean things up a bit.
			MessagePanelInVR.Instance.HidePanel();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region PAUSE

		/// <summary>Called from this manager. Invokes onSessionBreak event and starts listener to EvProceed event</summary>
		void SessionPause() {
			AddToExecutionOrderLog("SessionPaused");

			EventManager.StartListening(Edia.Events.StateMachine.EvProceed, SessionResumeAfterBreak);
			UpdateProgressStatus("Pause");

			OnSessionPaused.Invoke();

			EnableProceedButton(true);
			EnablePauseButton(false);
			EnableEyeCalibrationTrigger(true);
		}

		/// <summary>Called from EvProceed event. Stops listener, invokes onSessionResume event and calls UXF BeginNextTrial. </summary>
		void SessionResumeAfterBreak(eParam e) {
			AddToExecutionOrderLog("SessionResume");

			EventManager.StopListening(Edia.Events.StateMachine.EvProceed, SessionResumeAfterBreak);

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
		/// Saves a marker with a timestamp to disk. Also fires EvStoreMarker event with parameter.
		/// </summary>
		/// <param name="annotation">Annotation to store</param>
		public void StoreMarker(string annotation) {
			// Log it in the UXF way
			UXF.UXFDataRow newRow = new UXFDataRow();
			newRow.Add(("realtime", Time.realtimeSinceStartup)); // Log timestamp in realtime (indep. of time scaling and application idling)
			newRow.Add(("time", Time.time)); // Log timestamp at beginning of frame (affected by time scaling and app idling)
			newRow.Add(("annotation", annotation));
			_markerLog.AddCompleteRow(newRow);

			EventManager.TriggerEvent(Edia.Events.DataHandlers.EvStoreMarker, new eParam(annotation));
		}

		private void AddToLog(string _msg) {

			if (ShowLog)
				Edia.LogUtilities.AddToLog(_msg, "EXP", LogColor);
		}


#endregion  // -------------------------------------------------------------------------------------------------------------------------------
#region HELPERS

		/// <summary>
		/// Add a <key><value> pain to the trial results table
		/// </summary>
		/// <param name="key">Name of the key</param>
		/// <param name="value">Value in string format</param>
		public void AddToTrialResults(string key, string value) {
			Session.instance.CurrentTrial.result[key] = value;
		}

		void ConfigureXRrigTracking() {
			XRManager.Instance.XRCam.GetComponent<PositionRotationTracker>().enabled = TrackXrRigWithUxf;
			XRManager.Instance.XRLeft.GetComponent<PositionRotationTracker>().enabled = TrackXrRigWithUxf;
			XRManager.Instance.XRRight.GetComponent<PositionRotationTracker>().enabled = TrackXrRigWithUxf;

			if (!TrackXrRigWithUxf)
				return;

			Session.instance.trackedObjects.Add(XRManager.Instance.XRCam.GetComponent<PositionRotationTracker>());
			Session.instance.trackedObjects.Add(XRManager.Instance.XRLeft.GetComponent<PositionRotationTracker>());
			Session.instance.trackedObjects.Add(XRManager.Instance.XRRight.GetComponent<PositionRotationTracker>());
		}

		void SaveCustomDataTables() {
			Session.instance.SaveDataTable(_executionOrderLog, "executionOrder");
			Session.instance.SaveDataTable(_markerLog, "markerLog");
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}
}