using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using UXF;

namespace eDIA
{
	public class ExperimentGenerator : MonoBehaviour
	{
		public TextAsset EBSequenceJson;
		public List<TextAsset> TasksTextAssets = new();
		public List<TextAsset> TaskBlockTextAssets = new();
		public List<bool> validatedJsons = new();

		[Space(20)]
		public EBlockSequence EBSequence;
		//public BreakBlockDetails breakBlockSettings0;

		[System.Serializable]
		public class EBlockSequence {
			public List<string> Sequence = new();
		}

		[System.Serializable]
		public class EBlockBase {
			public string type;
			public string subType;
			public List<SettingsTuple> settings = new ();
			public List<SettingsTuple> instructions = new ();
		}

		//! Task list
		[System.Serializable]
		public class EBlock : EBlockBase {
		public string blockId;
			public TrialSettings trialSettings = new ();
		}

		// Internal checkup lists
		public List<EBlockBase> Tasks = new();
		public List<EBlock> EBlocks = new();

		// Quick and dirty conversion methods
		public void SetEBSequence(string jsonString) {
			EBSequence = UnityEngine.JsonUtility.FromJson<EBlockSequence>(jsonString);
			validatedJsons.Add(true);
			CheckIfReady();
		}

		public void SetTaskDefinitions(List<string> jsonStrings) {
			foreach (string t in jsonStrings) {
				EBlockBase tba = UnityEngine.JsonUtility.FromJson<EBlockBase>(t);
				Tasks.Add(tba);
			}
			validatedJsons.Add(true);
			CheckIfReady();
		}

		public void SetEBlockDefinitions(List<string> jsonStrings) {
			foreach (string t in jsonStrings) {
				EBlock tba = UnityEngine.JsonUtility.FromJson<EBlock>(t);
				EBlocks.Add(tba);
			}
			validatedJsons.Add(true);
			CheckIfReady();
		}
		
		void CheckIfReady() {
			if (validatedJsons.Count == 3) {
				Generate();
			}
		}

		public void StartExperiment () {
			UXF.Session.instance.Begin("partyanimalexp","partyhansi");
		}

		// ----------


		string GetEBlockType(string blockId) {
			return blockId.Split("_")[1]; //TODO: this relies on the structure "EBlock_type_subType_Nr"
		}

		EBlockBase GetEBlockBaseByBlockId(string blockId) {
			int _index = EBlocks.FindIndex(x => x.blockId == blockId); //TODO checks
			int _returnIndex = Tasks.FindIndex(x => x.subType == EBlocks[_index].subType);
			return _returnIndex == -1 ? null : Tasks[_returnIndex];
		}

		EBlock GetEBlockByBlockId(string blockId) {
			int _index = EBlocks.FindIndex(x => x.blockId == blockId);
			return _index != -1 ? EBlocks[_index] : null;
		}

		bool ValidateBlockList () {
			bool _succes = false;

			foreach (string blockId in EBSequence.Sequence) {
				if (GetEBlockByBlockId(blockId) != null) {
					_succes = true;
				}
			}

			return _succes;
		}

		static List<string> GetValuesListByKey(List<SettingsTuple> tupleList, string key) {
			return tupleList
				.Where(st => st.key == key)
				.Select(st => st.value)
				.ToList();
		}

		/// <summary>
		/// Generates the UXF sequence based on the supplied Json files and database
		/// </summary>
		public void Generate()
		{
			// First validate 
			if (!ValidateBlockList()) {
				Debug.Log("Supplied blocklist is invalid");
				return;
			}

			// Loop through BlockList, create blocks
			foreach (var blockId in EBSequence.Sequence) {

				Block newBlock = Session.instance.CreateBlock();

		    // Find the according EBlockBase (e.g., Task or Break definition) and get global settings
		    EBlockBase eBlockBase = GetEBlockBaseByBlockId(blockId);
		    newBlock.settings.UpdateWithDict(Helpers.GetSettingsTupleListAsDict(eBlockBase.settings));
		    
				newBlock.settings.SetValue("_start", GetValuesListByKey(eBlockBase.instructions, "_start")); //
		    newBlock.settings.SetValue("_end", GetValuesListByKey(eBlockBase.instructions, "_end")); //

		    EBlock currentEBlock = GetEBlockByBlockId(blockId);
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

		    switch (currentEBlockType) {
					case "Break":
						newBlock.settings.SetValue("breakType", currentEBlock.subType);
						newBlock.settings.SetValue("_info", GetValuesListByKey(currentEBlock.instructions, "_info"));
				Trial breakTrial = newBlock.CreateTrial();
				break;

					case "Task":
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

		private static bool IsValidKeyForTrialResults(string k) {
			return !Session.instance.settingsToLog.Contains(k) && !k.StartsWith("_");
		}
	}

}