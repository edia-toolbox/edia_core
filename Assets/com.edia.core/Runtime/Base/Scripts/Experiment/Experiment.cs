using Edia.Utilities;
using Edia.Data;
using Edia.IO;
using Edia.Tracking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Edia {

#region DECLARATIONS

    /// <summary>
    /// Main manager for the experiment.
    /// Handles the state machine and the full experiment lifecycle (session, blocks, trials).
    /// Single API for all task logic via Experiment.Instance.*
    /// </summary>
    [EdiaHeader("EDIA CORE", "Experiment Executor", "This executes the experiment based on the defined XBlock executors ")]
    public class Experiment : Singleton<Experiment> {

        private enum States {
            Idle,
            Running,
            WaitingOnProceed,
            Paused,
        }

        private States State      = States.Idle;
        private States _prevState = States.Idle;

        public List<XBlock> Executors = new();

        [Header("Debug")]
        public bool ShowConsoleMessages = false;

        public bool ShowEventMessages = false;

        [Header("Data")]
        public bool storeSessionSettings = true;
        public bool storeParticipantDetails = true;
        [Tooltip("Enable to automatically safely end the session when the application is quitting.")]
        public bool endOnQuit = true;

        // State machine fields
        private int       _activeSessionBlockNum = 0;
        private int       _currentStep           = -1;
        private bool      _isPauseRequested      = false;
        private XBlock    _activeXBlock;
        private Coroutine _proceedTimer = null;

        // Logging
        private DataTable _executionOrderLog = new("timestamp", "executed");
        private DataTable _markerLog         = new("timestamp", "annotation");

        // --- Session-level state (absorbed from UXF.Session) ---
        [HideInInspector] public List<Block> blocks = new List<Block>();
        [HideInInspector] public List<string> customHeaders = new List<string>();
        [HideInInspector] public List<string> settingsToLog = new List<string>();
        [HideInInspector] public List<Tracker> trackedObjects = new List<Tracker>();

        public Settings settings { get; private set; }
        public string experimentName { get; private set; }
        public string ppid { get; private set; }
        public int sessionNumber { get; private set; }
        public Dictionary<string, object> participantDetails { get; private set; }

        public int currentTrialNum = 0;
        public int currentBlockNum = 0;

        public bool hasInitialised { get; private set; } = false;
        public bool isEnding { get; private set; } = false;

        public bool InTrial { get { return (currentTrialNum != 0) && (CurrentTrial.status == TrialStatus.InProgress); } }

        public Trial CurrentTrial { get { return GetTrial(); } }
        public Trial NextTrial { get { return GetNextTrial(); } }
        public Trial PrevTrial { get { return GetPrevTrial(); } }
        public Trial FirstTrial { get { return GetFirstTrial(); } }
        public Trial LastTrial { get { return GetLastTrial(); } }
        public Block CurrentBlock { get { return GetBlock(); } }
        public IEnumerable<Trial> Trials { get { return blocks.SelectMany(b => b.trials); } }

        static List<string> baseHeaders = new List<string> { "experiment", "ppid", "session_num", "trial_num", "block_num", "trial_num_in_block", "start_time", "end_time" };
        public List<string> Headers { get { return baseHeaders.Concat(settingsToLog).Concat(customHeaders).Distinct().ToList(); } }

        /// <summary>
        /// FileSaver instance for writing data to disk. Managed by Experiment lifecycle.
        /// </summary>
        [HideInInspector] public FileSaver fileSaver;

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region MONO METHODS

        private void Awake() {
            EventManager.showLog = ShowEventMessages;
        }

        private void OnDestroy() {
            EventManager.StopListening(Edia.Events.StateMachine.EvStartExperiment, OnEvStartExperiment);
            EventManager.StopListening(Edia.Events.StateMachine.EvPauseExperiment, OnEvPauseExperiment);
            EventManager.StopListening(Edia.Events.Core.EvQuitApplication, OnEvQuitApplication);
        }

        private void Start() {
            if (!IsValid())
                return;

            EnableAllXBlocks(false);

            EventManager.StartListening(Edia.Events.Core.EvQuitApplication, OnEvQuitApplication);
            EventManager.StartListening(Edia.Events.StateMachine.EvStartExperiment, OnEvStartExperiment);
        }

        void OnApplicationQuit() {
            if (endOnQuit && hasInitialised) {
                EndSession();
            }
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region CHECKS

        private bool IsValid() {
            bool         isValid = true;
            List<string> msgs    = new();

            if (Executors == null || Executors.Count == 0 || Executors.All(x => x == null)) {
                isValid = false;
                msgs.Add("XBLock Executers list is empty!");
            }

            if (isValid)
                XBlockNamesToLower();

            var names = Executors.Select(g => g.name);
            if (isValid && names.Count() != names.Distinct().Count()) {
                msgs.Add("All XBlock Executers need unique names!");
                isValid = false;
            }

            foreach (XBlock g in Executors) {
                if (isValid && !Regex.IsMatch(g.name, @"^[a-z0-9]+-[a-z0-9\-'().,_]+$")) {
                    msgs.Add($"Invalid gameobject (XBlock Executer) naming format found in: <b>{g.name}</b>; must adhere to: <type>-<subtype>");
                    isValid = false;
                }
            }

            if (!isValid) {
                foreach (string s in msgs)
                    Debug.LogErrorFormat(s);
            }

            return isValid;
        }

        private void XBlockNamesToLower() {
            foreach (XBlock g in Executors) {
                g.name = g.name.ToLower();
            }
        }

        private void EnableAllXBlocks(bool onOff) {
            foreach (XBlock xb in Executors) {
                xb.enabled = onOff;
                xb.gameObject.SetActive(onOff);
            }
        }

        public bool IsXblockExecuterListed(string assetId) {
            return Executors.Any(go => go.name == assetId);
        }

        private void Reset() {
            _activeSessionBlockNum = 0;
            _currentStep           = -1;
            _isPauseRequested      = false;
            State                  = States.Idle;
            _prevState             = State;
        }

        private void Update() {
            if (State != _prevState) {
                _prevState = State;
            }
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region SESSION LIFECYCLE

        /// <summary>
        /// Create and return a Block, which gets automatically added to blocks list.
        /// </summary>
        public Block CreateBlock() {
            return new Block(0);
        }

        /// <summary>
        /// Create and return a Block with a given number of trials.
        /// </summary>
        public Block CreateBlock(int numberOfTrials) {
            if (numberOfTrials >= 0)
                return new Block((uint)numberOfTrials);
            else
                throw new Exception("Invalid number of trials supplied");
        }

        /// <summary>
        /// Begins the experiment session. Initializes the FileSaver and session state.
        /// </summary>
        public void BeginSession(string experimentName, string participantId, int sessionNumber = 1,
            Dictionary<string, object> participantDetails = null, Settings settings = null) {

            this.experimentName = experimentName;
            this.ppid = participantId;
            this.sessionNumber = sessionNumber;
            this.participantDetails = participantDetails ?? new Dictionary<string, object>();
            this.settings = settings ?? Settings.empty;

            // Initialize FileSaver
            string storagePath = SystemSettings.Instance.Settings.pathToLogfiles;
            if (SystemSettings.Instance.isRemote)
                storagePath = Application.dataPath;

            fileSaver = new FileSaver(storagePath);
            fileSaver.SetUp();

            hasInitialised = true;
            Debug.Log("[Experiment] Beginning session.");

            // Directly call session begin handler (no more UXF events)
            OnSessionBegin();
        }

        /// <summary>
        /// Ends the experiment session. Saves all results and cleans up.
        /// </summary>
        public void EndSession() {
            if (hasInitialised) {
                isEnding = true;
                if (InTrial) {
                    try { CurrentTrial.End(); }
                    catch (Exception e) { Debug.LogException(e); }
                }

                SaveResults();

                if (storeSessionSettings) {
                    string json = DictToJson(settings.baseDict);
                    fileSaver.HandleJSONSerializableObject(json, experimentName, ppid, sessionNumber, "settings", DataType.Settings);
                }

                if (storeParticipantDetails && participantDetails != null && participantDetails.Count > 0) {
                    DataTable ppDetailsTable = new DataTable(participantDetails.Keys.ToArray());
                    var row = new DataRow();
                    foreach (var kvp in participantDetails) row.Add((kvp.Key, kvp.Value));
                    ppDetailsTable.AddCompleteRow(row);
                    fileSaver.HandleDataTable(ppDetailsTable, experimentName, ppid, sessionNumber, "participant_details", DataType.ParticipantDetails);
                }

                // Clean up FileSaver — forces completion of pending tasks
                try { fileSaver.CleanUp(); }
                catch (Exception e) { Debug.LogException(e); }

                // Fire session ended, then call the handler
                OnSessionEnd();

                currentTrialNum = 0;
                currentBlockNum = 0;
                blocks = new List<Block>();
                settingsToLog = new List<string>();
                customHeaders = new List<string>();
                trackedObjects = new List<Tracker>();
                hasInitialised = false;

                Debug.Log("[Experiment] Ended session.");
                isEnding = false;
            }
        }

        /// <summary>
        /// Ends the current trial.
        /// </summary>
        public void EndCurrentTrial() {
            CurrentTrial.End();
            // Directly call the trial end handler
            OnTrialEnd(CurrentTrial);
        }

        /// <summary>
        /// Begins the next trial.
        /// </summary>
        public void BeginNextTrial() {
            NextTrial.Begin();
            // Directly call the trial begin handler
            OnTrialBegin(CurrentTrial);
        }

        /// <summary>
        /// Begins next trial if one exists.
        /// </summary>
        public void BeginNextTrialSafe() {
            if (CurrentTrial != LastTrial) {
                BeginNextTrial();
            }
        }

        /// <summary>
        /// Saves a DataTable at session level.
        /// </summary>
        public void SaveDataTable(DataTable table, string dataName, DataType dataType = DataType.OtherSessionData) {
            fileSaver.HandleDataTable(table, experimentName, ppid, sessionNumber, dataName, dataType);
        }

        private void SaveResults() {
            Trial.WaitForTasks();

            HashSet<string> resultsHeaders = new HashSet<string>();
            foreach (Trial t in Trials)
                if (t.result != null)
                    foreach (string key in t.result.Keys)
                        resultsHeaders.Add(key);

            DataTable table = new DataTable(Trials.Count(), resultsHeaders.ToArray());
            foreach (Trial t in Trials) {
                if (t.result != null) {
                    DataRow row = new DataRow();
                    foreach (string h in resultsHeaders) {
                        if (t.result.ContainsKey(h) && t.result[h] != null)
                            row.Add((h, t.result[h]));
                        else
                            row.Add((h, string.Empty));
                    }
                    table.AddCompleteRow(row);
                }
            }

            fileSaver.HandleDataTable(table, experimentName, ppid, sessionNumber, "trial_results", DataType.TrialResults);
        }

        Trial GetTrial() {
            if (currentTrialNum == 0)
                throw new NoSuchTrialException("There is no trial zero. Did you try to perform operations on the current trial before the first one started?");
            return Trials.ToList()[currentTrialNum - 1];
        }

        Trial GetTrial(int trialNumber) {
            return Trials.ToList()[trialNumber - 1];
        }

        Trial GetNextTrial() {
            try {
                return Trials.ToList()[currentTrialNum];
            }
            catch (ArgumentOutOfRangeException) {
                throw new NoSuchTrialException("There is no next trial. Reached the end of trial list.");
            }
        }

        Trial GetPrevTrial() {
            try {
                return Trials.ToList()[currentTrialNum - 2];
            }
            catch (ArgumentOutOfRangeException) {
                throw new NoSuchTrialException("There is no previous trial. Probably at the start of session.");
            }
        }

        Trial GetFirstTrial() {
            if (blocks.Count == 0) throw new NoSuchTrialException("There is no first trial because no blocks have been created!");
            if (blocks[0].trials.Count == 0) throw new NoSuchTrialException("There is no first trial. No trials exist in the first block.");
            return blocks[0].trials[0];
        }

        Trial GetLastTrial() {
            if (blocks.Count == 0) throw new NoSuchTrialException("There is no last trial because no blocks have been created!");
            int i = blocks.Count - 1;
            while (i >= 0) {
                Trial last = blocks[i].lastTrial;
                if (last != null) return last;
                i--;
            }
            throw new NoSuchTrialException("There is no last trial, blocks are present but they are all empty.");
        }

        Block GetBlock() {
            return blocks[currentBlockNum - 1];
        }

        public Block GetBlock(int blockNumber) {
            return blocks[blockNumber - 1];
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region INFORMATION UPDATES

        private void UpdateProgressStatus(string info) {
            List<string> infos = new();

            if (info.Contains('-')) {
                infos = info.Split('-').ToList();
                infos.RemoveAt(0);
            }
            else infos.Add(info);

            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateProgressStatus, new eParam(StringTools.CombineToOneString(infos.ToArray())));
        }

        private void UpdateSessionSummary() {
            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateSessionSummary, new eParam(SessionSettings.sessionInfo.GetSessionSummary()));
        }

        private void UpdateBlockProgress() {
            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateBlockProgress,
                new eParam(new int[] { currentBlockNum, blocks.Count }));
        }

        private void UpdateTrialProgress() {
            if (InTrial)
                EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateTrialProgress,
                    new eParam(new int[] { CurrentTrial.numberInBlock, CurrentBlock.trials.Count() }));
        }

        private void UpdateStepProgress() {
            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateStepProgress, new eParam(new int[] { _currentStep, _activeXBlock.trialSteps.Count }));
        }

        /// <summary>Show a message to the VR user in an overlayed panel. Default: Proceed button ON</summary>
        public void ShowMessageToUser(string msg) {
            ShowMessageToUserGeneric();
            MessagePanelInVR.Instance.ShowMessage(msg);
        }

        /// <summary>Show a list of messages to the VR user, one at a time, in the MessagePanelInVR.</summary>
        public void ShowMessageToUser(List<string> msgs) {
            ShowMessageToUserGeneric();
            MessagePanelInVR.Instance.ShowMessage(msgs);
        }

        /// <summary>Show a message to the VR user in the MessagePanelInVR for a certain duration. Does NOT auto proceed</summary>
        public void ShowMessageToUser(string msg, float duration) {
            ShowMessageToUserGeneric();
            MessagePanelInVR.Instance.ShowMessage(msg, duration);
        }

        /// <summary>Show a message to the VR user in the MessagePanelInVR.</summary>
        public void ShowMessageToUser(string msg, bool showButton) {
            ShowMessageToUserGeneric();
            MessagePanelInVR.Instance.ShowMessage(msg);
            if (!showButton)
                MessagePanelInVR.Instance.HideMenu();
        }

        private void ShowMessageToUserGeneric() {
            AddToExecutionOrderLog("ShowMessageToUser");
            EnableProceedButton(true);
            EnablePauseButton(false);
            EnableEyeCalibrationTrigger(true);
        }

        public void HideMessagePanelMenu() {
            MessagePanelInVR.Instance.HideMenu();
        }

        public void HideMessagePanelToUser() {
            MessagePanelInVR.Instance.HidePanel();
        }

        public void ShowMessageToExperimenter(string msg, bool autohide) {
            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam(msg, autohide));
        }

        public void ShowMessageToExperimenter(string msg) {
            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam(msg, true));
        }

        public void AddTrialInfoToPanel(string key, string value) {
            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowTrialInfo, new eParam(new string[] { key, value }));
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region EXPERIMENT CONTROL

        private void StartExperiment() {
            BeginSession(
                SessionSettings.sessionInfo.experiment == string.Empty ? "N.A." : SessionSettings.sessionInfo.experiment,
                SessionSettings.sessionInfo.GetParticipantID(),
                SessionSettings.sessionInfo.sessionNumber,
                SessionSettings.sessionInfo.GetParticipantDetailsAsDict(),
                new Settings(Helpers.GetSettingsTupleListAsDict(SessionSettings.settings))
            );

            UpdateProgressStatus("Session started");
            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateBlockProgress, new eParam(new int[] { 0, blocks.Count }));
            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateTrialProgress, new eParam(new int[] { 0, 0 }));
        }

        private void OnEvStartExperiment(eParam e) {
            EventManager.StopListening(Edia.Events.StateMachine.EvStartExperiment, OnEvStartExperiment);
            StartExperiment();
        }

        private void OnEvPauseExperiment(eParam e) {
            AddToExecutionOrderLog("InjectedSessionPauseCall");
            _isPauseRequested = true;
        }

        private void OnEvQuitApplication(eParam obj) {
            AddToConsole("Quiting..");
            Application.Quit();
        }

        public void EnablePauseButton(bool _onOff) {
            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PAUSE", _onOff.ToString() }));
            EventManager.StartListening(Edia.Events.StateMachine.EvPauseExperiment, OnEvPauseExperiment);
        }

        public void EnableEyeCalibrationTrigger(bool _onOff) {
            EventManager.TriggerEvent(Edia.Events.Eye.EvEnableEyeCalibrationTrigger, new eParam(_onOff));
        }

        public void ShowTimerInController(float duration) {
            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvStartTimer, new eParam(duration));
        }

        public void StopTimerInController() {
            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvStopTimer, null);
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region STATEMACHINE PROCEED

        public void EnableProceedButton(bool onOff) {
            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", onOff ? "true" : "false" }));
        }

        public void WaitOnProceed() {
            State = States.WaitingOnProceed;
            EnableProceedButton(true);
            EventManager.StartListening(Edia.Events.StateMachine.EvProceed, OnEvProceed);
        }

        public void Proceed() {
            EventManager.TriggerEvent(Edia.Events.StateMachine.EvProceed);
        }

        private void OnEvProceed(eParam e) {
            EventManager.StopListening(Edia.Events.StateMachine.EvProceed, OnEvProceed);
            EnableProceedButton(false);
            Continue();
        }

        private void Continue() {
            State = States.Running;
            EventManager.StopListening(Edia.Events.StateMachine.EvProceed, OnEvProceed);
            EnableProceedButton(false);
            NextTrialStep();
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region STATEMACHINE SESSION

        private void OnSessionBegin() {
            State                  = States.Running;
            _activeSessionBlockNum = 0;

            AddToExecutionOrderLog("OnSessionBegin");
            EventManager.StartListening(Edia.Events.StateMachine.EvProceed, OnEvStartFirstTrial);

            UpdateBlockProgress();
            UpdateSessionSummary();
            UpdateProgressStatus("Welcome");

            EnableProceedButton(true);
            EnableEyeCalibrationTrigger(true);

            bool showMsg = settings.ContainsKey("_intro");
            if (showMsg)
                ShowMessageToUser(settings.GetString("_intro"));
            else {
                Proceed();
            }
        }

        private void OnSessionEnd() {
            EnableAllXBlocks(false);

            AddToExecutionOrderLog("OnSessionEnd");

            UpdateProgressStatus("End");
            EnableProceedButton(false);
            EnablePauseButton(false);

            EventManager.TriggerEvent(Edia.Events.StateMachine.EvSessionEnded, null);

            Reset();

            bool showMsg = settings.ContainsKey("_outro");
            if (showMsg) {
                ShowMessageToUser(settings.GetString("_outro"));
                HideMessagePanelMenu();
            }
            else {
                Proceed();
            }
        }

        private void FinalizeSession() {
            AddToConsole("FinalizeSession");
            UpdateProgressStatus("Finalizing Session");
            EndSession();
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region STATEMACHINE XBLOCKS

        private void BlockStart() {
            AddToConsole("Block Start");

            _activeSessionBlockNum = currentBlockNum;
            _activeXBlock          = Executors[Executors.FindIndex(x => x.name == CurrentBlock.settings.GetString("_assetId"))];
            _activeXBlock.enabled  = true;
            _activeXBlock.gameObject.SetActive(true);
            _activeXBlock.OnBlockStart();

            UpdateBlockProgress();

            if (CurrentBlock.settings.GetBool("_hasIntro")) {
                EventManager.StartListening(Edia.Events.StateMachine.EvProceed, BlockContinueAfterIntro);
                ShowMessageToUser(CurrentBlock.settings.GetStringList("_intro"));
                UpdateProgressStatus(CurrentBlock.settings.GetString("blockId") + " Introduction");
            }
            else {
                StartTrial();
                UpdateProgressStatus(CurrentBlock.settings.GetString("blockId"));
            }
        }

        private void BlockEnd() {
            _activeXBlock.OnBlockEnd();

            if (CurrentBlock.settings.GetBool("_hasOutro")) {
                EventManager.StartListening(Edia.Events.StateMachine.EvProceed, BlockContinueAfterOutro);
                EnableProceedButton(true);
                ShowMessageToUser(CurrentBlock.settings.GetStringList("_outro"));
                UpdateProgressStatus("Block Outro");
                _activeXBlock.OnBlockOutro();
            }
            else {
                BlockCheckAndContinue();
            }
        }

        private void BlockCheckAndContinue() {
            _activeXBlock.enabled = false;
            _activeXBlock.gameObject.SetActive(false);

            if (LastTrial == CurrentTrial) {
                AddToConsole("Reached end of trials ");
                FinalizeSession();
                return;
            }

            BeginNextTrialSafe();
        }

        private void BlockContinueAfterIntro(eParam e) {
            EventManager.StopListening(Edia.Events.StateMachine.EvProceed, BlockContinueAfterIntro);
            AddToExecutionOrderLog("BlockContinueAfterIntro");
            UpdateProgressStatus(CurrentBlock.settings.GetString("blockId"));
            EnableEyeCalibrationTrigger(false);
            StartTrial();
        }

        private void BlockContinueAfterOutro(eParam e) {
            EventManager.StopListening(Edia.Events.StateMachine.EvProceed, BlockContinueAfterOutro);
            AddToExecutionOrderLog("BlockContinueAfterOutro");
            BlockCheckAndContinue();
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region TRIALS

        private void OnEvStartFirstTrial(eParam e) {
            EventManager.StopListening(Edia.Events.StateMachine.EvProceed, OnEvStartFirstTrial);
            UpdateTrialProgress();
            BeginNextTrial();
        }

        private void OnTrialBegin(Trial newTrial) {
            AddToExecutionOrderLog("OnTrialBegin");

            XRManager.Instance.ShowVRInstantly();

            bool isNewBlock = (currentBlockNum != _activeSessionBlockNum) &&
                              (currentBlockNum <= blocks.Count);

            if (isNewBlock) {
                BlockStart();
            }
            else {
                StartTrial();
                UpdateProgressStatus(CurrentBlock.settings.GetString("blockId"));
            }
        }

        private void OnTrialEnd(Trial endedTrial) {
            AddToExecutionOrderLog("OnTrialEnd");
            SaveCustomDataTables();

            EventManager.TriggerEvent(Edia.Events.StateMachine.EvTrialEnd);

            _activeXBlock.OnEndTrial();

            if (isEnding)
                return;

            if (_isPauseRequested) {
                _isPauseRequested = false;
                if (endedTrial == LastTrial)
                    return;
                AddToExecutionOrderLog("Injected SessionBreak");
                SessionPause();
                return;
            }

            if (CurrentBlock.lastTrial != endedTrial) {
                BeginNextTrialSafe();
                return;
            }
            else {
                BlockEnd();
                return;
            }
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region STATEMACHINE TRIAL STEPS

        private void StartTrial() {
            AddToConsole("StartTrial");

            _activeXBlock.OnStartTrial();
            UpdateTrialProgress();

            _currentStep = -1;
            NextTrialStep();
        }

        private void EndTrial() {
            AddToConsole("Trial Steps DONE");
            EndCurrentTrial();
        }

        public void ProceedWithDelay(float duration) {
            if (_proceedTimer != null) StopCoroutine(_proceedTimer);
            _proceedTimer = StartCoroutine(ProceedTimer(duration));
        }

        private IEnumerator ProceedTimer(float duration) {
            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvStartTimer, new eParam(duration));
            yield return new WaitForSecondsRealtime(duration);
            Proceed();
        }

        private void NextTrialStep() {
            if (ShowConsoleMessages) AddToConsole("Nextstep >");

            if (_proceedTimer != null) {
                StopCoroutine(_proceedTimer);
                EventManager.TriggerEvent(Edia.Events.ControlPanel.EvStopTimer);
            }

            EnableProceedButton(false);

            _currentStep++;

            if (_currentStep < _activeXBlock.trialSteps.Count) {
                InBetweenSteps();
                UpdateStepProgress();
                AddToExecutionOrderLog(_activeXBlock.trialSteps[_currentStep].Method.Name);
                _activeXBlock.trialSteps[_currentStep].Invoke();
            }
            else EndTrial();
        }

        private void InBetweenSteps() {
            _activeXBlock.OnBetweenSteps();
            MessagePanelInVR.Instance.HidePanel();
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region PAUSE

        private void SessionPause() {
            AddToExecutionOrderLog("SessionPaused");

            EventManager.StartListening(Edia.Events.StateMachine.EvProceed, SessionResumeAfterBreak);
            UpdateProgressStatus("Pause");

            EnableProceedButton(true);
            EnablePauseButton(false);
            EnableEyeCalibrationTrigger(true);

            bool showMsg = settings.ContainsKey("_pause");
            if (showMsg)
                ShowMessageToUser(settings.GetString("_pause"));
        }

        private void SessionResumeAfterBreak(eParam e) {
            AddToExecutionOrderLog("SessionResume");
            EventManager.StopListening(Edia.Events.StateMachine.EvProceed, SessionResumeAfterBreak);
            EnableEyeCalibrationTrigger(false);
            BeginNextTrialSafe();
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region LOGGING

        /// <summary>Converts given data to a DataTable, and stores the data to disk linked to the active trial.</summary>
        public void ConvertAndSaveData(string[] headers, List<string[]> values, string filename) {
            var data = new DataTable(headers);

            foreach (string[] valuerow in values) {
                DataRow newRow = new DataRow();
                for (int s = 0; s < valuerow.Length; s++) {
                    newRow.Add((headers[s], valuerow[s]));
                }
                data.AddCompleteRow(newRow);
            }

            CurrentTrial.SaveDataTable(data, filename);
        }

        private void AddToExecutionOrderLog(string description) {
            AddToConsole(description);
            DataRow newRow = new DataRow();
            newRow.Add(("timestamp", Time.time));
            newRow.Add(("executed", description));
            _executionOrderLog.AddCompleteRow(newRow);
        }

        /// <summary>
        /// Saves a marker with a timestamp to disk. Also fires EvStoreMarker event.
        /// </summary>
        public void StoreMarker(string annotation) {
            DataRow newRow = new DataRow();
            newRow.Add(("timestamp", Time.time));
            newRow.Add(("annotation", annotation));
            _markerLog.AddCompleteRow(newRow);

            EventManager.TriggerEvent(Edia.Events.DataHandlers.EvStoreMarker, new eParam(annotation));
        }

        private void AddToConsole(string _msg) {
            if (ShowConsoleMessages)
                Log.AddToConsoleLog(_msg, "EXP");
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region HELPERS

        /// <summary>
        /// Add a key-value pair to the trial results table.
        /// </summary>
        public void AddToTrialResults(string key, string value) {
            CurrentTrial.result[key] = value;
        }

        private void SaveCustomDataTables() {
            SaveDataTable(_executionOrderLog, "executionOrder");
            SaveDataTable(_markerLog, "markerLog");
        }

        /// <summary>
        /// Simple JSON serialization for a flat dictionary. Used to save session settings.
        /// </summary>
        private static string DictToJson(Dictionary<string, object> dict) {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("{");
            int i = 0;
            foreach (var kvp in dict) {
                string valueStr;
                if (kvp.Value == null)
                    valueStr = "null";
                else if (kvp.Value is string s)
                    valueStr = "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
                else if (kvp.Value is bool b)
                    valueStr = b ? "true" : "false";
                else if (kvp.Value is int || kvp.Value is float || kvp.Value is double || kvp.Value is long)
                    valueStr = kvp.Value.ToString();
                else
                    valueStr = "\"" + kvp.Value.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

                sb.Append($"  \"{kvp.Key}\": {valueStr}");
                if (i < dict.Count - 1) sb.Append(",");
                sb.AppendLine();
                i++;
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

    }
}
