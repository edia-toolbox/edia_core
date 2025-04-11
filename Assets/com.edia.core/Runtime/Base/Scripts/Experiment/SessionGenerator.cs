using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UXF;

namespace Edia {

    public class SessionGenerator : MonoBehaviour {

        // Internal checkup lists
        public readonly  List<XBlockBaseSettings> _bases          = new();
        public readonly  List<XBlockSettings>     _xBlocks        = new();
        private readonly List<bool>               _validated = new();
        public           XBlockSequence           _xBlockSequence;
        public           XBlockBaseSettings       _sessionXblock = new();

        private void Awake() {
            EventManager.StartListening(Edia.Events.Config.EvSetSessionInfo, OnEvSetSessionInfo);
            EventManager.StartListening(Edia.Events.Config.EvSetXBlockSequence, OnEvSetXBlockSequence);
            EventManager.StartListening(Edia.Events.Config.EvSetBaseDefinitions, OnEvSetBaseDefinitions);
            EventManager.StartListening(Edia.Events.Config.EvSetXBlockDefinitions, OnEvSetXBlockDefinitions);

            EventManager.StartListening(Edia.Events.StateMachine.EvSessionEnded, OnFinalizeSession);
        }

        private void OnFinalizeSession(eParam param) {
            Reset();
        }

        public void Reset() {
            _validated.Clear();
        }

#region EVENT HANDLING

        private void OnEvSetSessionInfo(eParam param) {
            SessionSettings.sessionInfo               = JsonUtility.FromJson<SessionInfo>(param.GetStrings()[0]);
            SessionSettings.sessionInfo.sessionNumber = int.Parse(param.GetStrings()[1]); // UXF wants an int

            SettingsTuple participantTuple = new() {
                key   = "id",
                value = param.GetStrings()[2]
            };
            SessionSettings.sessionInfo.participantDetails.Add(participantTuple);
    
            
            _validated.Add(true);
            AddToConsole($"[{_validated.Count} of 5] Session info OK");
            
            CheckIfReadyAndContinue();
        }

        private void OnEvSetXBlockSequence(eParam param) {
            _xBlockSequence = JsonUtility.FromJson<XBlockSequence>(param.GetString());

            AddToConsole($"[{_validated.Count} of 5] Session sequence OK");
            
            _validated.Add(true);
            CheckIfReadyAndContinue();
        }

        private void OnEvSetBaseDefinitions(eParam param) {
            foreach (string t in param.GetStrings()) {
                XBlockBaseSettings xBBs = JsonUtility.FromJson<XBlockBaseSettings>(t);

                if (xBBs.type.ToLower() == "session") { // One of the jsons is the global session info
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
                XBlockSettings xBs = JsonUtility.FromJson<XBlockSettings>(t);
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
                if (!GenerateUxfSequence()) {
                    AddToConsole("Failed to generate session sequence", LogType.Error);
                    return;
                }

                _validated.Add(true);
                AddToConsole($"[{_validated.Count} of 5] Session generation DONE", LogType.Error);
                
                EventManager.TriggerEvent(Edia.Events.Config.EvReadyToGo);
                EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateSessionSummary, new eParam(SessionSettings.sessionInfo.GetSessionSummary()));
            }
        }

        // Checks if all entries in the sequence, have their detail config loaded
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

        /// <summary>
        /// Validates if a given key is suitable for trial results by checking
        /// it is not part of the session's settings to log and does not start with an underscore.
        /// </summary>
        /// <param name="k">The key to be validated.</param>
        /// <return>Returns true if the key is valid for trial results, otherwise false.</return>
        private static bool IsValidKeyForTrialResults(string k) {
            return !Session.instance.settingsToLog.Contains(k) && !k.StartsWith("_");
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region UXF CONVERSION

        /// <summary>
        /// Generates the UXF sequence based on the supplied Json files and database
        /// </summary>
        private bool GenerateUxfSequence() {
            if (!ValidateXBlockList()) {
                // EventManager.TriggerEvent(Edia.Events.Config.EvSessionInitialisationFailed);
                return false;
            }

            // Loop through BlockList, create blocks
            foreach (var blockId in _xBlockSequence.sequence) {
                // Find the according XBlockBase (e.g., Task or Break definition) and get global settings
                XBlockBaseSettings xBlockBase = GetXBlockBaseByBlockId(blockId);
                if (xBlockBase == null) {
                    AddToConsole($"Failed getting details for {blockId} ", LogType.Error);
                    return false;
                }

                // Is it's XblockExecuter listed in the XBLockExecuters?
                string assetId = xBlockBase.type.ToLower() + "-" + xBlockBase.subType.ToLower();
                if (!Experiment.Instance.IsXblockExecuterListed(assetId)) {
                    var msg = $"Executors list does not contain a gameobject named '<b>{assetId}</b>' ";
                    Experiment.Instance.ShowMessageToExperimenter(msg, false);
                    AddToConsole(msg, LogType.Error);
                    return false;
                }

                Block newBlock = Session.instance.CreateBlock();

                if (xBlockBase.settings.Count > 0) {
                    newBlock.settings.UpdateWithDict(Helpers.GetSettingsTupleListAsDict(xBlockBase.settings));
                }

                newBlock.settings.SetValue("_start", GetValuesListByKey(xBlockBase.instructions, "_start")); //
                newBlock.settings.SetValue("_end", GetValuesListByKey(xBlockBase.instructions, "_end")); //

                XBlockSettings currentXBlock = GetXBlockByBlockId(blockId);
                newBlock.settings.SetValue("blockType", currentXBlock.type.ToLower());
                newBlock.settings.SetValue("blockId", currentXBlock.blockId.ToLower());
                newBlock.settings.SetValue("_assetId", assetId);

                // Add block specific instructions, if any
                foreach (string s in new string[] { "_start", "_end" }) {
                    var newList = newBlock.settings.GetStringList(s);
                    newList.AddRange(GetValuesListByKey(currentXBlock.instructions, s));
                    newBlock.settings.SetValue(s, newList);
                }

                // Continue with settings
                newBlock.settings.UpdateWithDict(Helpers.GetSettingsTupleListAsDict(currentXBlock.settings)); // add block specific settings

                switch (GetXBlockType(blockId).ToLower()) {
                    case "break":
                        newBlock.settings.SetValue("subType", currentXBlock.subType);
                        newBlock.settings.SetValue("_info", GetValuesListByKey(currentXBlock.instructions, "_info"));
                        newBlock.CreateTrial(); // create 1 dummy trial
                        break;

                    case "task":
                        newBlock.settings.SetValue("subType", currentXBlock.subType);

                        // Add trials 
                        if (currentXBlock.trialSettings.valueList == null || currentXBlock.trialSettings.valueList.Count == 0) {
                            newBlock.CreateTrial(); // create 1 dummy trial in case of empty settings 
                            AddToConsole($"No trial settings found for XBlock {currentXBlock.subType}. Adding an empty trial.");
                        }
                        else {
                            foreach (ValueList row in currentXBlock.trialSettings.valueList) {
                                Trial trial = newBlock.CreateTrial();

                                for (var i = 0; i < row.values.Count; i++) {
                                    // Check if value is an array
                                    if (row.values[i].Contains(';')) {
                                        var stringlist = row.values[i].Split(';').ToList();
                                        for (var s = 0; s < stringlist.Count; s++) {
                                            var newstring = stringlist[s].Replace(" ", string.Empty); // remove spaces 
                                            stringlist[s] = newstring;
                                        }

                                        trial.settings.SetValue(currentXBlock.trialSettings.keys[i], stringlist); // stringlist);
                                    }
                                    else {
                                        trial.settings.SetValue(currentXBlock.trialSettings.keys[i], row.values[i]); // set values to trial
                                    }
                                }
                            }
                        }

                        // Log all unique TRIAL settings keys
                        foreach (var k in currentXBlock.trialSettings.keys) {
                            if (IsValidKeyForTrialResults(k))
                                Session.instance.settingsToLog.Add(k);
                        }

                        break;

                    default:
                        var msg = $"XBlock type must be either 'Task' or 'Break'; cannot be '{GetXBlockType(blockId).ToLower()}'.";
                        Experiment.Instance.ShowMessageToExperimenter(msg, true);
                        AddToConsole(msg, LogType.Error);
                        break;
                }

                // Log all unique and valid TASK setting keys
                foreach (string k in newBlock.settings.Keys) {
                    if (IsValidKeyForTrialResults(k))
                        Session.instance.settingsToLog.Add(k);
                }
            }

            // Set UXF.Session.instance.settings 
            foreach (SettingsTuple tuple in _sessionXblock.settings) {
                Session.instance.settings.SetValue(tuple.key, tuple.value);
            }

            foreach (SettingsTuple instructionTuple in _sessionXblock.instructions ) {
                Session.instance.settings.SetValue(instructionTuple.key, instructionTuple.value);
            }

            return true;
        }

        private void AddToConsole(string _msg) {
            if (Experiment.Instance.ShowConsoleMessages)
                Edia.LogUtilities.AddToConsoleLog(_msg, "SessionGenerator");
        }

        private void AddToConsole(string msg, LogType _type) {
            if (_type == LogType.Error) Debug.LogError(msg);
            else if (_type == LogType.Warning) Debug.LogWarning(msg);
            else AddToConsole(msg, _type);
        }
    }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
}