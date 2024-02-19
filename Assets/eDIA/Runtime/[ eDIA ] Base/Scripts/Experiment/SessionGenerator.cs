using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using UXF;

namespace eDIA {

	public class SessionGenerator : MonoBehaviour {

		// Internal checkup lists
		public List<XBlockBaseSettings> Tasks = new();
		public List<XBlockSettings> xBlocks = new();
		public List<bool> validatedJsons = new();

		public XBlockSequence _xBlockSequence;

		private void Awake() {
			EventManager.StartListening(eDIA.Events.Config.EvSetSessionInfo, OnEvSetSessionInfo);
			EventManager.StartListening(eDIA.Events.Config.EvSetEBlockSequence, OnEvSetXBlockSequence);
			EventManager.StartListening(eDIA.Events.Config.EvSetTaskDefinitions, OnEvSetBaseDefinitions);
			EventManager.StartListening(eDIA.Events.Config.EvSetEBlockDefinitions, OnEvSetEBlockDefinitions);
		}

		public void Reset() {
			validatedJsons.Clear();
		}

		#region EVENT HANDLING

		private void OnEvSetSessionInfo(eParam param) {

			SessionSettings.sessionInfo = UnityEngine.JsonUtility.FromJson<SessionInfo>(param.GetStrings()[0]);
			SessionSettings.sessionInfo.session_number = int.Parse(param.GetStrings()[1]); // UXF wants an int

			SettingsTuple participantTuple = new SettingsTuple();
			participantTuple.key = "id";
			participantTuple.value = param.GetStrings()[2];
			SessionSettings.sessionInfo.participant_details.Add(participantTuple);

			validatedJsons.Add(true);
			CheckIfReadyAndContinue();
		}

		private void OnEvSetXBlockSequence(eParam param) {
			_xBlockSequence = UnityEngine.JsonUtility.FromJson<XBlockSequence>(param.GetString());
			validatedJsons.Add(true);
			CheckIfReadyAndContinue();
		}

		private void OnEvSetBaseDefinitions(eParam param) {
			foreach (string t in param.GetStrings()) {
				XBlockBaseSettings EBs = UnityEngine.JsonUtility.FromJson<XBlockBaseSettings>(t);
				Tasks.Add(EBs);
			}
			validatedJsons.Add(true);
			CheckIfReadyAndContinue();
		}

		private void OnEvSetEBlockDefinitions(eParam param) {
			foreach (string t in param.GetStrings()) {
				XBlockSettings EBs = UnityEngine.JsonUtility.FromJson<XBlockSettings>(t);
				xBlocks.Add(EBs);
			}
			validatedJsons.Add(true);
			CheckIfReadyAndContinue();
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region DATA GETTERS


		string GetEBlockType(string blockId) {
			return blockId.Split("_")[1]; //TODO: this relies on the structure "EBlock_type_subType_Nr"
		}

		XBlockBaseSettings GetXBlockBaseByBlockId(string blockId) {
			int _index = xBlocks.FindIndex(x => x.blockId.ToLower() == blockId.ToLower()); //TODO checks
			int _returnIndex = Tasks.FindIndex(x => x.subType.ToLower() == xBlocks[_index].subType.ToLower());
			return _returnIndex == -1 ? null : Tasks[_returnIndex];
		}

		XBlockSettings GetXBlockByBlockId(string blockId) {
			int _index = xBlocks.FindIndex(x => x.blockId.ToLower() == blockId.ToLower());
			return _index != -1 ? xBlocks[_index] : null;
		}

		static List<string> GetValuesListByKey(List<SettingsTuple> tupleList, string key) {
			return tupleList
				.Where(st => st.key == key)
				.Select(st => st.value)
				.ToList();
		}


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region VALIDATORS

		void CheckIfReadyAndContinue() { // TODO: Is there a more elegant way of doing this?

			if (validatedJsons.Count == 4) {
				GenerateUXFSequence();

				EventManager.TriggerEvent(eDIA.Events.Config.EvReadyToGo);
				EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateSessionSummary, new eParam(SessionSettings.sessionInfo.GetSessionSummary()));
			}
		}

		// Checks if all entries in the sequence, have their detail config loaded
		bool ValidateBlockList() {
			bool _succes = true;

			// Does all entries in the sequence have their detailed config loaded?
			foreach (string blockId in _xBlockSequence.Sequence) {
				if (GetXBlockByBlockId(blockId) == null) {
					Debug.LogWarningFormat("No details found for <b>{0}</b>", blockId);
					_succes = false;
				}
			}

			return _succes;
		}

		private static bool IsValidKeyForTrialResults(string k) {
			return !Session.instance.settingsToLog.Contains(k) && !k.StartsWith("_");
		}


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region UXF

		/// <summary>
		/// Generates the UXF sequence based on the supplied Json files and database
		/// </summary>
		public void GenerateUXFSequence() {
			// First validate 
			if (!ValidateBlockList()) {
				Debug.Log("Supplied blocklist is invalid");
				return;
			}

			// Loop through BlockList, create blocks
			foreach (var blockId in _xBlockSequence.Sequence) {

				Block newBlock = Session.instance.CreateBlock();

				// Find the according EBlockBase (e.g., Task or Break definition) and get global settings
				XBlockBaseSettings xBlockBase = GetXBlockBaseByBlockId(blockId);
				newBlock.settings.UpdateWithDict(Helpers.GetSettingsTupleListAsDict(xBlockBase.settings));

				newBlock.settings.SetValue("_start", GetValuesListByKey(xBlockBase.instructions, "_start")); //
				newBlock.settings.SetValue("_end", GetValuesListByKey(xBlockBase.instructions, "_end")); //

				XBlockSettings currentXBlock = GetXBlockByBlockId(blockId);
				newBlock.settings.SetValue("blockType", currentXBlock.type.ToLower());
				newBlock.settings.SetValue("blockId", currentXBlock.blockId.ToLower());
				newBlock.settings.SetValue("_assetId", currentXBlock.type.ToLower() + "_" + currentXBlock.subType.ToLower());

				// Add block specific instructions, if any
				foreach (string s in new string[] { "_start", "_end" }) {
					List<string> newList = newBlock.settings.GetStringList(s);
					newList.AddRange(GetValuesListByKey(currentXBlock.instructions, s));
					newBlock.settings.SetValue(s, newList);
				}

				// Continue with settings
				newBlock.settings.UpdateWithDict(Helpers.GetSettingsTupleListAsDict(currentXBlock.settings)); // add block specific settings

				// Add settings and trials specific for Break vs Task EBlocks
				string currentXBlockType = GetEBlockType(blockId);

				switch (currentXBlockType.ToLower()) {
					case "break":
						newBlock.settings.SetValue("breakType", currentXBlock.subType);
						newBlock.settings.SetValue("_info", GetValuesListByKey(currentXBlock.instructions, "_info"));
						Trial breakTrial = newBlock.CreateTrial();
						break;

					case "task":
						newBlock.settings.SetValue("taskType", currentXBlock.subType);

						// Add trials 
						foreach (ValueList row in currentXBlock.trialSettings.valueList) {
							Trial trial = newBlock.CreateTrial();

							for (int i = 0; i < row.values.Count; i++) {
								trial.settings.SetValue(currentXBlock.trialSettings.keys[i], row.values[i]); // set values to trial
							}
						}

						// Log all unique TRIAL settings keys
						foreach (string k in currentXBlock.trialSettings.keys) {
							if (IsValidKeyForTrialResults(k))
								Session.instance.settingsToLog.Add(k);
						}
						break;

					default:
						Debug.LogError($"EBlock type must be either 'Task' or 'Break'; cannot be '{currentXBlockType}'.");
						break;
				}

				// Log all unique and valid TASK setting keys
				foreach (string k in newBlock.settings.Keys) {
					if (IsValidKeyForTrialResults(k))
						Session.instance.settingsToLog.Add(k);
				}
			}
		}
	}

	#endregion // -------------------------------------------------------------------------------------------------------------------------------
}