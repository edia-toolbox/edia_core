using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;
using UXF;

namespace eDIA {

#region EXPERIMENT CONFIG (JSON SERIALIZABLE)

	/// <summary> Tuple of strings, this is serializable in the inspector and dictionaries are not</summary>
	[System.Serializable]
	public class SettingsTuple {
		[HideInInspector]
		public string key 			= string.Empty;
		public string value 			= string.Empty;
	}

	/// <summary> List of string values, in a class to make it serializable by JSON</summary>
	[System.Serializable]
	public class ValueList {
		public List<string> values = new List<string>();
	}

	/// <summary> Experiment trial settings container</summary>
	[System.Serializable]
	public class TrialSettings {
		[HideInInspector]
		public List<string> 			keys 				= new List<string>();
		public List<ValueList>			valueList 			= new List<ValueList>();
	}

	/// <summary> Experiment block container  </summary>
	[System.Serializable]
	public class ExperimentBlock {
		public string 				name				= string.Empty;
		public string 				introduction 		= string.Empty;
		public List<SettingsTuple>		blockSettings		= new List<SettingsTuple>();
		public TrialSettings			trialSettings		= new TrialSettings();
	}


	/// <summary> Experiment config container</summary>
	[System.Serializable]
	public class ExperimentConfig {
		public string				experiment			= string.Empty;
		public string 				experimenter 		= string.Empty;
		public int 					sessionNumber 		= 0;
		public List<SettingsTuple>		participantDetails 	= new List<SettingsTuple>();

		// Local check if this instance is loaded and ready to go
		public bool 				isReady			= false;

		//? Class helper methods
		public string[] GetExperimentSummary() {
			return new string[] { experiment, experimenter, GetParticipantID (), sessionNumber.ToString() };
		}

		public string GetParticipantID () {
			return participantDetails.Find(x=>x.key=="ID").value;
		}

		public Dictionary<string,object> GetParticipantDetailsAsDict () {
			return Helpers.GetSettingsTupleListAsDict(participantDetails);
		}

	}

	/// <summary> Experiment config container</summary>
	[System.Serializable]
	public class TaskConfig {
		public List<SettingsTuple>		taskSettings 		= new List<SettingsTuple>();
		public List<int>				breakAfter			= new List<int>(); 
		public List<ExperimentBlock>		blocks			= new List<ExperimentBlock>();

		// Local check if this instance is loaded and ready to go
		public bool 				isReady			= false;

		//? Class helper methods
		public Dictionary<string,object> GetTaskSettingsAsDict () {
			return Helpers.GetSettingsTupleListAsDict(taskSettings);
		}

		/// <summary>/// Convert JSON formatted definition for the seqence into a UXF format to run in the session/// </summary>
		public void GenerateUXFSequence() {

			// Reorder the taskblock list in the taskmanager
			List<TaskBlock> reordered = new List<TaskBlock>();
			
			foreach (ExperimentBlock b in blocks) {
				reordered.Add(Experiment.Instance.taskBlocks.Find(x => x.name == b.name));
			}

			Experiment.Instance.taskBlocks.Clear();
			Experiment.Instance.taskBlocks.AddRange(reordered);

			// Convert the Taskconfig into UXF blocks and settings
			foreach (ExperimentBlock b in blocks) {
				
				Block newBlock = Session.instance.CreateBlock();
				newBlock.settings.SetValue("name",b.name);
				newBlock.settings.SetValue("introduction",b.introduction);

				newBlock.settings.UpdateWithDict( Helpers.GetSettingsTupleListAsDict(b.blockSettings) );

				foreach (ValueList row in b.trialSettings.valueList) {
					Trial newTrial = newBlock.CreateTrial();

					for (int i = 0; i < row.values.Count; i++) {
						newTrial.settings.SetValue( b.trialSettings.keys[i], row.values[i].ToUpper() ); // set values to trial
					}
				}

				// Log all keys
				foreach (string k in b.trialSettings.keys)
					Session.instance.settingsToLog.Add(k);

			}
		}
	}

	public static class Helpers {

		public static Dictionary<string,object> GetSettingsTupleListAsDict (List<SettingsTuple> list) {
			Dictionary<string,object> tmp = new Dictionary<string, object>();
			foreach (SettingsTuple st in list)
				if (st.value.Contains(',')) { // it's a list!
					List<string> stringlist = st.value.Split(',').ToList();
					tmp.Add(st.key, stringlist);
				} else tmp.Add(st.key, st.value);	// normal string
				
			return tmp;
		}
	}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK RELATED STATE MACHINE

	/// <summary>One step of a trial</summary>
	[System.Serializable]
	public class TrialStep	{
		public string title;
		public Action methodToCall;

		public TrialStep ( string title, Action methodToCall) {
			this.title 		= title;
			this.methodToCall = methodToCall;
		}
	}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region SYSTEM RELATED

	/// <summary>Container to hold main settings of the application </summary>
	[System.Serializable]
	public class SettingsDeclaration {

		public Constants.Interactor VisableInteractor = Constants.Interactor.BOTH;
		public Constants.Interactor InteractiveInteractor = Constants.Interactor.RIGHT;
		public int screenResolution = 0;
		public float volume = 50f;
		public Constants.Languages language = Constants.Languages.ENG;

		public string pathToLogfiles = "logfiles";
		public static string localConfigDirectoryName = "Configs";
		
	}


#endregion // -------------------------------------------------------------------------------------------------------------------------------

}