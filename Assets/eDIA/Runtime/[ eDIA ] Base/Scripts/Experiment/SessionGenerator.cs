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
		List<XBlockBaseSettings> Tasks = new();
		List<XBlockSettings> EBlocks = new();
		List<bool> validatedJsons = new();

		XBlockSequence _eBlockSequence;

		private void Awake() {
			EventManager.StartListening(eDIA.Events.Config.EvSetSessionInfo, OnEvSetSessionInfo);
			EventManager.StartListening(eDIA.Events.Config.EvSetEBlockSequence, OnEvSetEBlockSequence);
			EventManager.StartListening(eDIA.Events.Config.EvSetTaskDefinitions, OnEvSetTaskDefinitions);
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

		private void OnEvSetEBlockSequence(eParam param) {
			_eBlockSequence = UnityEngine.JsonUtility.FromJson<XBlockSequence>(param.GetString());
			validatedJsons.Add(true);
			CheckIfReadyAndContinue();
		}

		private void OnEvSetTaskDefinitions(eParam param) {
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
				EBlocks.Add(EBs);
			}
			validatedJsons.Add(true);
			CheckIfReadyAndContinue();
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region DATA GETTERS


		string GetEBlockType(string blockId) {
			return blockId.Split("_")[1]; //TODO: this relies on the structure "EBlock_type_subType_Nr"
		}

		XBlockBaseSettings GetEBlockBaseByBlockId(string blockId) {
			int _index = EBlocks.FindIndex(x => x.blockId.ToLower() == blockId.ToLower()); //TODO checks
			int _returnIndex = Tasks.FindIndex(x => x.subType.ToLower() == EBlocks[_index].subType.ToLower());
			return _returnIndex == -1 ? null : Tasks[_returnIndex];
		}

		XBlockSettings GetEBlockByBlockId(string blockId) {
			int _index = EBlocks.FindIndex(x => x.blockId.ToLower() == blockId.ToLower());
			return _index != -1 ? EBlocks[_index] : null;
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
			foreach (string blockId in _eBlockSequence.Sequence) {
				if (GetEBlockByBlockId(blockId) == null) {
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
			foreach (var blockId in _eBlockSequence.Sequence) {

				Block newBlock = Session.instance.CreateBlock();

				// Find the according EBlockBase (e.g., Task or Break definition) and get global settings
				XBlockBaseSettings eBlockBase = GetEBlockBaseByBlockId(blockId);
				newBlock.settings.UpdateWithDict(Helpers.GetSettingsTupleListAsDict(eBlockBase.settings));

				newBlock.settings.SetValue("_start", GetValuesListByKey(eBlockBase.instructions, "_start")); //
				newBlock.settings.SetValue("_end", GetValuesListByKey(eBlockBase.instructions, "_end")); //

				XBlockSettings currentEBlock = GetEBlockByBlockId(blockId);
				newBlock.settings.SetValue("blockType", currentEBlock.type);
				newBlock.settings.SetValue("blockId", currentEBlock.blockId);
				newBlock.settings.SetValue("_assetId", currentEBlock.type + "_" + currentEBlock.subType);

				// Add block specific instructions, if any
				foreach (string s in new string[] { "_start", "_end" }) {
					List<string> newList = newBlock.settings.GetStringList(s);
					newList.AddRange(GetValuesListByKey(currentEBlock.instructions, s));
					newBlock.settings.SetValue(s, newList);
				}

				// Continue with settings
				newBlock.settings.UpdateWithDict(Helpers.GetSettingsTupleListAsDict(currentEBlock.settings)); // add block specific settings

				// Add settings and trials specific for Break vs Task EBlocks
				string currentEBlockType = GetEBlockType(blockId);

				switch (currentEBlockType.ToLower()) {
					case "break":
						newBlock.settings.SetValue("breakType", currentEBlock.subType);
						newBlock.settings.SetValue("_info", GetValuesListByKey(currentEBlock.instructions, "_info"));
						Trial breakTrial = newBlock.CreateTrial();
						break;

					case "task":
						newBlock.settings.SetValue("taskType", currentEBlock.subType);

						// Add trials 
						foreach (ValueList row in currentEBlock.trialSettings.valueList) {
							Trial trial = newBlock.CreateTrial();

							for (int i = 0; i < row.values.Count; i++) {
								trial.settings.SetValue(currentEBlock.trialSettings.keys[i], row.values[i]); // set values to trial
							}
						}

						// Log all unique TRIAL settings keys
						foreach (string k in currentEBlock.trialSettings.keys) {
							if (IsValidKeyForTrialResults(k))
								Session.instance.settingsToLog.Add(k);
						}
						break;

					default:
						Debug.LogError($"EBlock type must be either 'Task' or 'Break'; cannot be '{currentEBlockType}'.");
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