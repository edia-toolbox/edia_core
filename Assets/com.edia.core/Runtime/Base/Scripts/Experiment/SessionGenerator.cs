using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UXF;

namespace Edia {

    public class SessionGenerator : MonoBehaviour {

        // Internal checkup lists
        private readonly List<XBlockBaseSettings> _tasks = new();
        private readonly List<XBlockSettings> _xBlocks = new();
        private readonly List<bool> _validatedJsons = new();
        private XBlockSequence _xBlockSequence;

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
            _validatedJsons.Clear();
        }

        #region EVENT HANDLING

        private void OnEvSetSessionInfo(eParam param) {

            SessionSettings.SessionInfo = JsonUtility.FromJson<SessionInfo>(param.GetStrings()[0]);
            SessionSettings.SessionInfo.SessionNumber = int.Parse(param.GetStrings()[1]); // UXF wants an int

            SettingsTuple participantTuple = new() {
                Key = "id",
                Value = param.GetStrings()[2]
            };
            SessionSettings.SessionInfo.ParticipantDetails.Add(participantTuple);

            _validatedJsons.Add(true);
            CheckIfReadyAndContinue();
        }

        private void OnEvSetXBlockSequence(eParam param) {
            _xBlockSequence = JsonUtility.FromJson<XBlockSequence>(param.GetString());
            _validatedJsons.Add(true);
            CheckIfReadyAndContinue();
        }

        private void OnEvSetBaseDefinitions(eParam param) {
            foreach (string t in param.GetStrings()) {
                XBlockBaseSettings xBBs = JsonUtility.FromJson<XBlockBaseSettings>(t);
                
                if (xBBs.Type.ToLower() == "session") { // One of the jsons is the global session info
                    
                    foreach (var settingTuple in xBBs.Settings) {
                        SessionSettings.Settings.Add(settingTuple);
                    }
                    foreach (var instructionTuple in xBBs.Instructions) {
                        SessionSettings.Instructions.Add(instructionTuple);
                    }
                    
                } else _tasks.Add(xBBs);
            }
            
            _validatedJsons.Add(true);
            CheckIfReadyAndContinue();
        }

        private void OnEvSetXBlockDefinitions(eParam param) {
            foreach (string t in param.GetStrings()) {
                XBlockSettings xBs = JsonUtility.FromJson<XBlockSettings>(t);
                _xBlocks.Add(xBs);
            }
            _validatedJsons.Add(true);
            CheckIfReadyAndContinue();
        }

        #endregion // -------------------------------------------------------------------------------------------------------------------------------
        #region DATA GETTERS

        string GetXBlockType(string blockId) {
            return blockId.Split("-")[0];
        }

        XBlockBaseSettings GetXBlockBaseByBlockId(string blockId) {
            int index = _xBlocks.FindIndex(x => x.BlockId.ToLower() == blockId.ToLower());
            int returnIndex = _tasks.FindIndex(x => x.SubType.ToLower() == _xBlocks[index].SubType.ToLower());
            return returnIndex == -1 ? null : _tasks[returnIndex];
        }

        XBlockSettings GetXBlockByBlockId(string blockId) {
            int index = _xBlocks.FindIndex(x => x.BlockId.ToLower() == blockId.ToLower());
            return index != -1 ? _xBlocks[index] : null;
        }

        static List<string> GetValuesListByKey(List<SettingsTuple> tupleList, string key) {
            return tupleList
                .Where(st => st.Key == key)
                .Select(st => st.Value)
                .ToList();
        }


        #endregion // -------------------------------------------------------------------------------------------------------------------------------
        #region VALIDATORS

        void CheckIfReadyAndContinue() { // TODO: Is there a more elegant way of doing this?

            if (_validatedJsons.Count == 4) {
                GenerateUxfSequence();

                EventManager.TriggerEvent(Edia.Events.Config.EvReadyToGo);
                EventManager.TriggerEvent(Edia.Events.ControlPanel.EvUpdateSessionSummary, new eParam(SessionSettings.SessionInfo.GetSessionSummary()));
            }
        }

        // Checks if all entries in the sequence, have their detail config loaded
        bool ValidateBlockList() {
            bool success = true;

            foreach (string blockId in _xBlockSequence.Sequence) {
                if (GetXBlockByBlockId(blockId) == null) {
                    Debug.LogWarningFormat("No details found for <b>{0}</b>", blockId);
                    success = false;
                }
            }

            return success;
        }

        private static bool IsValidKeyForTrialResults(string k) {
            return !Session.instance.settingsToLog.Contains(k) && !k.StartsWith("_");
        }


        #endregion // -------------------------------------------------------------------------------------------------------------------------------
        #region UXF

        /// <summary>
        /// Generates the UXF sequence based on the supplied Json files and database
        /// </summary>
        void GenerateUxfSequence() {
            // First validate 
            if (!ValidateBlockList()) {
                Debug.LogError("Supplied blocklist is invalid");
                return;
            }

            // Loop through BlockList, create blocks
            foreach (var blockId in _xBlockSequence.Sequence) {

                // Find the according XBlockBase (e.g., Task or Break definition) and get global settings
                XBlockBaseSettings xBlockBase = GetXBlockBaseByBlockId(blockId);
                if (xBlockBase == null) {
                    Debug.LogError($"Failed getting details for {blockId} ");
                    return;
                }

                // Is it's XblockExecuter listed in the XBLockExecuters?
                string assetId = xBlockBase.Type.ToLower() + "-" + xBlockBase.SubType.ToLower();
                if (!Experiment.Instance.IsXblockExecuterListed(assetId)) {
                    string msg = $"XblockExecuters list does not contain gameobject named '<b>{assetId}</b>' ";
                    Experiment.Instance.ShowMessageToExperimenter(msg, true);
                    Debug.LogError(msg);
                    return;
                }

                Block newBlock = Session.instance.CreateBlock();

                if (xBlockBase.Settings.Count > 0) {
                    newBlock.settings.UpdateWithDict(Helpers.GetSettingsTupleListAsDict(xBlockBase.Settings));
                }

                newBlock.settings.SetValue("_start", GetValuesListByKey(xBlockBase.Instructions, "_start")); //
                newBlock.settings.SetValue("_end", GetValuesListByKey(xBlockBase.Instructions, "_end")); //

                XBlockSettings currentXBlock = GetXBlockByBlockId(blockId);
                newBlock.settings.SetValue("blockType", currentXBlock.Type.ToLower());
                newBlock.settings.SetValue("blockId", currentXBlock.BlockId.ToLower());
                newBlock.settings.SetValue("_assetId", assetId);

                // Add block specific instructions, if any
                foreach (string s in new string[] { "_start", "_end" }) {
                    List<string> newList = newBlock.settings.GetStringList(s);
                    newList.AddRange(GetValuesListByKey(currentXBlock.Instructions, s));
                    newBlock.settings.SetValue(s, newList);
                }

                // Continue with settings
                newBlock.settings.UpdateWithDict(Helpers.GetSettingsTupleListAsDict(currentXBlock.Settings)); // add block specific settings

                // Add settings and trials specific for Break vs Task XBlocks
                string currentXBlockType = GetXBlockType(blockId);

                switch (currentXBlockType.ToLower()) {
                    case "break":
                        newBlock.settings.SetValue("subType", currentXBlock.SubType);
                        newBlock.settings.SetValue("_info", GetValuesListByKey(currentXBlock.Instructions, "_info"));
                        newBlock.CreateTrial(); // create 1 dummy trial
                        break;

                    case "task":
                        newBlock.settings.SetValue("subType", currentXBlock.SubType);

                        // Add trials 
                        if (currentXBlock.TrialSettings.ValueList == null || currentXBlock.TrialSettings.ValueList.Count == 0) {
                            newBlock.CreateTrial(); // create 1 dummy trial in case of empty settings 
                            AddToConsole($"No trial settings found for XBlock {currentXBlock.SubType}. Adding an empty trial.");
                        } else {
                            foreach (ValueList row in currentXBlock.TrialSettings.ValueList) {
                                Trial trial = newBlock.CreateTrial();

                                for (int i = 0; i < row.Values.Count; i++) {

                                    // Check if value is an array
                                    if (row.Values[i].Contains(';')) {
                                        List<string> stringlist = row.Values[i].Split(';').ToList();
                                        for (int s = 0; s < stringlist.Count; s++) {
                                            string newstring = stringlist[s].Replace(" ", string.Empty); // remove spaces 
                                            stringlist[s] = newstring;
                                        }
                                        trial.settings.SetValue(currentXBlock.TrialSettings.Keys[i], stringlist); // stringlist);
                                    } else {
                                        trial.settings.SetValue(currentXBlock.TrialSettings.Keys[i], row.Values[i]); // set values to trial
                                    }
                                }
                            }
                        }
                        // Log all unique TRIAL settings keys
                        foreach (string k in currentXBlock.TrialSettings.Keys) {
                            if (IsValidKeyForTrialResults(k))
                                Session.instance.settingsToLog.Add(k);
                        }
                        break;

                    default:
                        string msg = $"XBlock type must be either 'Task' or 'Break'; cannot be '{currentXBlockType}'.";
                        Experiment.Instance.ShowMessageToExperimenter(msg, true);
                        Debug.LogError(msg);
                        break;
                }

                // Log all unique and valid TASK setting keys
                foreach (string k in newBlock.settings.Keys) {
                    if (IsValidKeyForTrialResults(k))
                        Session.instance.settingsToLog.Add(k);
                }
            }

            Debug.Log("Passed session generation validation");
        }
        
        private void AddToConsole(string _msg) {

            if (Experiment.Instance.ShowConsoleMessages)
                Edia.LogUtilities.AddToConsoleLog(_msg, "XRManager");
        }
    }

    #endregion // -------------------------------------------------------------------------------------------------------------------------------
}