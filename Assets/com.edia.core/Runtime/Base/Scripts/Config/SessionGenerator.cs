using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Edia {

    public class SessionGenerator : MonoBehaviour {

        // Internal checkup lists
        private readonly List<XBlockBaseSettings> _bases     = new();
        private readonly List<XBlockSettings>     _xBlocks   = new();
        private readonly List<bool>               _validated = new();
        private          XBlockSequence           _xBlockSequence;
        private          XBlockBaseSettings       _sessionXblock = new();

        private void Awake() {
            EventManager.StartListening(Edia.Events.Config.EvSetSessionInfo, OnEvSetSessionInfo);
            EventManager.StartListening(Edia.Events.Config.EvSetXBlockSequence, OnEvSetXBlockSequence);
            EventManager.StartListening(Edia.Events.Config.EvSetBaseDefinitions, OnEvSetBaseDefinitions);
            EventManager.StartListening(Edia.Events.Config.EvSetXBlockDefinitions, OnEvSetXBlockDefinitions);

            EventManager.StartListening(Edia.Events.StateMachine.EvSessionEnded, OnFinalizeSession);
        }

        private void OnDestroy() {
            EventManager.StopListening(Edia.Events.Config.EvSetSessionInfo, OnEvSetSessionInfo);
            EventManager.StopListening(Edia.Events.Config.EvSetXBlockSequence, OnEvSetXBlockSequence);
            EventManager.StopListening(Edia.Events.Config.EvSetBaseDefinitions, OnEvSetBaseDefinitions);
            EventManager.StopListening(Edia.Events.Config.EvSetXBlockDefinitions, OnEvSetXBlockDefinitions);
            EventManager.StopListening(Edia.Events.StateMachine.EvSessionEnded, OnFinalizeSession);
        }

        private void OnFinalizeSession(eParam param) {
            Reset();
        }

        public void Reset() {
            _validated.Clear();
        }

#region EVENT HANDLING

        private void OnEvSetSessionInfo(eParam param) {
            try {
                SessionSettings.sessionInfo = JsonUtility.FromJson<SessionInfo>(param.GetStrings()[0]);
            }
            catch (System.Exception e) {
                AddToConsole($"Failed to parse SessionInfo: {e.Message}", LogType.Error);
                EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam($"Failed to parse SessionInfo", false));
                return;
            }

            SessionSettings.sessionInfo.sessionNumber = int.Parse(param.GetStrings()[1]);
            SettingsTuple participantTuple = new() {
                key   = "id",
                value = param.GetStrings()[2]
            };

            SessionSettings.sessionInfo.participant_details.Add(participantTuple);

            _validated.Add(true);
            AddToConsole($"[{_validated.Count} of 5] Session info OK");

            CheckIfReadyAndContinue();
        }

        private void OnEvSetXBlockSequence(eParam param) {
            try {
                _xBlockSequence = JsonUtility.FromJson<XBlockSequence>(param.GetString());
            }
            catch (System.Exception e) {
                AddToConsole($"Failed to parse XBlock sequence: {e.Message}", LogType.Error);
                EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox,
                    new eParam($"Failed to parse XBlock sequence", false));
                return;
            }

            _validated.Add(true);
            AddToConsole($"[{_validated.Count} of 5] Session sequence OK");

            CheckIfReadyAndContinue();
        }

        private void OnEvSetBaseDefinitions(eParam param) {
            foreach (string t in param.GetStrings()) {
                XBlockBaseSettings xBBs = new();

                try {
                    xBBs = JsonUtility.FromJson<XBlockBaseSettings>(t);
                }
                catch (System.Exception e) {
                    AddToConsole($"Failed to parse base definition: {e.Message}", LogType.Error);
                    EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox,
                        new eParam($"Failed to parse base definition", false));
                    return;
                }

                if (xBBs.type.ToLower() == "session") {
                    _sessionXblock = xBBs;
                }
                else _bases.Add(xBBs);
            }

            _validated.Add(true);
            AddToConsole($"[{_validated.Count} of 5] Session base definitions OK");

            CheckIfReadyAndContinue();
        }

        private void OnEvSetXBlockDefinitions(eParam param) {
            foreach (string t in param.GetStrings()) {
                XBlockSettings xBs = new();

                try {
                    xBs = JsonUtility.FromJson<XBlockSettings>(t);
                }
                catch (System.Exception e) {
                    AddToConsole($"Failed to parse Xblock setting: {e.Message}", LogType.Error);
                    EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox,
                        new eParam($"Failed to parse Xblock setting", false));
                    return;
                }

                _xBlocks.Add(xBs);
            }

            _validated.Add(true);
            AddToConsole($"[{_validated.Count} of 5] Session block definitions OK");

            CheckIfReadyAndContinue();
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region DATA GETTERS

        string GetXBlockType(string blockId) {
            return blockId.Split("-")[0];
        }

        XBlockBaseSettings GetXBlockBaseByBlockId(string blockId) {
            int index       = _xBlocks.FindIndex(x => x.blockId.ToLower() == blockId.ToLower());
            int returnIndex = _bases.FindIndex(x => x.subType.ToLower() == _xBlocks[index].subType.ToLower());
            return returnIndex == -1 ? null : _bases[returnIndex];
        }

        XBlockSettings GetXBlockByBlockId(string blockId) {
            int index = _xBlocks.FindIndex(x => x.blockId.ToLower() == blockId.ToLower());
            return index != -1 ? _xBlocks[index] : null;
        }

        static List<string> GetValuesListByKey(List<SettingsTuple> tupleList, string key) {
            return tupleList
                .Where(st => st.key == key)
                .Select(st => st.value)
                .ToList();
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region VALIDATORS

        void CheckIfReadyAndContinue() {
            if (_validated.Count == 4) {
                if (!GenerateSequence()) {
                    AddToConsole("Failed to generate session sequence", LogType.Error);
                    return;
                }

                _validated.Add(true);
                AddToConsole($"[{_validated.Count} of 5] Session generation DONE");

                EventManager.TriggerEvent(Edia.Events.Config.EvReadyToGo);
                EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateSessionSummary, new eParam(SessionSettings.sessionInfo.GetSessionSummary()));
            }
        }

        bool ValidateXBlockList() {
            bool success = true;

            foreach (string blockId in _xBlockSequence.sequence) {
                if (GetXBlockByBlockId(blockId) == null) {
                    AddToConsole($"No detailed info found for <b>{blockId}</b>. Make sure the {blockId}.json exists and has proper values.", LogType.Error);
                    success = false;
                }
            }

            return success;
        }

        private static bool IsValidKeyForTrialResults(string k) {
            return !Experiment.Instance.settingsToLog.Contains(k) && !k.StartsWith("_");
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region SEQUENCE GENERATION

        /// <summary>
        /// Generates the experiment sequence based on the supplied JSON files.
        /// </summary>
        private bool GenerateSequence() {
            if (!ValidateXBlockList()) {
                return false;
            }

            foreach (var blockId in _xBlockSequence.sequence) {
                XBlockBaseSettings xBlockBase = GetXBlockBaseByBlockId(blockId);
                if (xBlockBase == null) {
                    AddToConsole(
                        $"No block definition details found for <b>{blockId}</b>. Is the 'type','subType' and 'blockId' set correctly in the {blockId}.json?",
                        LogType.Error);
                    return false;
                }

                string assetId = xBlockBase.type.ToLower() + "-" + xBlockBase.subType.ToLower();
                if (!Experiment.Instance.IsXblockExecuterListed(assetId)) {
                    var msg = $"Executors list does not contain a gameobject named '<b>{assetId}</b>' ";
                    Experiment.Instance.ShowMessageToExperimenter(msg, false);
                    AddToConsole(msg, LogType.Error);
                    return false;
                }

                Block newBlock = Experiment.Instance.CreateBlock();

                if (xBlockBase.settings.Count > 0) {
                    newBlock.settings.UpdateWithDict(Helpers.GetSettingsTupleListAsDict(xBlockBase.settings));
                }

                XBlockSettings currentXBlock = GetXBlockByBlockId(blockId);
                newBlock.settings.SetValue("blockType", currentXBlock.type.ToLower());
                newBlock.settings.SetValue("blockId", currentXBlock.blockId.ToLower());
                newBlock.settings.SetValue("_assetId", assetId);

                List<string> introMessages = new();
                List<string> outroMessages = new();

                if (xBlockBase.messages is not null && xBlockBase.messages.Count > 0) {
                    var baseIntroMsgs = GetValuesListByKey(xBlockBase.messages, "_intro");
                    var baseOutroMsgs = GetValuesListByKey(xBlockBase.messages, "_outro");

                    if (baseIntroMsgs.Count > 0)
                        introMessages.AddRange(baseIntroMsgs);
                    if (baseOutroMsgs.Count > 0)
                        outroMessages.AddRange(baseOutroMsgs);
                }

                if (currentXBlock.messages is not null && currentXBlock.messages.Count > 0) {
                    var blockIntroMsgs = GetValuesListByKey(currentXBlock.messages, "_intro");
                    var blockOutroMsgs = GetValuesListByKey(currentXBlock.messages, "_outro");

                    if (blockIntroMsgs.Count > 0)
                        introMessages.AddRange(blockIntroMsgs);
                    if (blockOutroMsgs.Count > 0)
                        outroMessages.AddRange(blockOutroMsgs);
                }

                newBlock.settings.SetValue("_hasIntro", introMessages.Count > 0);
                newBlock.settings.SetValue("_hasOutro", outroMessages.Count > 0);

                if (introMessages.Count > 0) {
                    newBlock.settings.SetValue("_intro", introMessages);
                }

                if (outroMessages.Count > 0) {
                    newBlock.settings.SetValue("_outro", outroMessages);
                }

                newBlock.settings.UpdateWithDict(Helpers.GetSettingsTupleListAsDict(currentXBlock.settings));

                switch (GetXBlockType(blockId).ToLower()) {
                    case "break":
                        newBlock.settings.SetValue("subType", currentXBlock.subType);
                        newBlock.settings.SetValue("_info", GetValuesListByKey(currentXBlock.messages, "_info"));
                        newBlock.CreateTrial();
                        break;

                    case "task":
                        newBlock.settings.SetValue("subType", currentXBlock.subType);

                        if (currentXBlock.trialSettings.valueList == null || currentXBlock.trialSettings.valueList.Count == 0) {
                            newBlock.CreateTrial();
                            AddToConsole($"No trial settings found for XBlock <b>{currentXBlock.subType}</b>. Adding an empty trial.");
                        }
                        else {
                            foreach (ValueList row in currentXBlock.trialSettings.valueList) {
                                Trial trial = newBlock.CreateTrial();

                                for (var i = 0; i < row.values.Count; i++) {
                                    if (row.values[i].Contains(';')) {
                                        var stringlist = row.values[i].Split(';').ToList();
                                        for (var s = 0; s < stringlist.Count; s++) {
                                            var newstring = stringlist[s].Replace(" ", string.Empty);
                                            stringlist[s] = newstring;
                                        }

                                        trial.settings.SetValue(currentXBlock.trialSettings.keys[i], stringlist);
                                    }
                                    else {
                                        trial.settings.SetValue(currentXBlock.trialSettings.keys[i], row.values[i]);
                                    }
                                }
                            }
                        }

                        foreach (var k in currentXBlock.trialSettings.keys) {
                            if (IsValidKeyForTrialResults(k))
                                Experiment.Instance.settingsToLog.Add(k);
                        }

                        break;

                    default:
                        var msg = $"XBlock type must be either 'Task' or 'Break'; cannot be <b>'{GetXBlockType(blockId).ToLower()}</b>'.";
                        Experiment.Instance.ShowMessageToExperimenter(msg, true);
                        AddToConsole(msg, LogType.Error);
                        break;
                }

                foreach (string k in newBlock.settings.Keys) {
                    if (IsValidKeyForTrialResults(k)) {
                        Experiment.Instance.settingsToLog.Add(k);
                    }
                }
            }

            // Session wide settings
            foreach (SettingsTuple settingsTuple in _sessionXblock.settings) {
                SessionSettings.settings.Add(new SettingsTuple { key = settingsTuple.key, value = settingsTuple.value });
                if (IsValidKeyForTrialResults(settingsTuple.key))
                    Experiment.Instance.settingsToLog.Add(settingsTuple.key);
            }

            foreach (SettingsTuple instructionTuple in _sessionXblock.messages) {
                SessionSettings.settings.Add(new SettingsTuple { key = instructionTuple.key, value = instructionTuple.value });
            }

            return true;
        }

        private void AddToConsole(string _msg) {
            if (Experiment.Instance.ShowConsoleMessages)
                Edia.Utilities.Log.AddToConsoleLog(_msg, "SessionGenerator");
        }

        private void AddToConsole(string msg, LogType _type) {
            if (_type == LogType.Error) Debug.LogError(msg);
            else if (_type == LogType.Warning) Debug.LogWarning(msg);
            else AddToConsole(msg);
        }
    }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
}
