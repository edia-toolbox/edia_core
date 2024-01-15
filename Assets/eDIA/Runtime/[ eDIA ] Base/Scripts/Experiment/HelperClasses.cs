using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;
using UXF;

namespace eDIA {

#region SESSION CONFIG (JSON SERIALIZABLE)

	/// <summary> Tuple of strings, this is serializable in the inspector and dictionaries are not</summary>
	[System.Serializable]
	public class SettingsTuple {
		[HideInInspector]
		public string key = string.Empty;
		public string value = string.Empty;
	}

	/// <summary> List of string values, in a class to make it serializable by JSON</summary>
	[System.Serializable]
	public class ValueList {
		public List<string> values = new List<string>();
	}

	///// <summary> Experiment trial settings container</summary>
	[System.Serializable]
	public class TrialSettings {
		[HideInInspector]
		public List<string> keys = new List<string>();
		public List<ValueList> valueList = new List<ValueList>();
	}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region UXF SEQUENCE (JSON SERIALIZABLE)

	[System.Serializable]
	public class EBlockSequence {
		public List<string> Sequence = new();
	}

	[System.Serializable]
	public class EBlockBaseSettings {
		public string type;
		public string subType;
		public List<SettingsTuple> settings = new();
		public List<SettingsTuple> instructions = new();
	}

	//! Task list
	[System.Serializable]
	public class EBlockSettings : EBlockBaseSettings {
		public string blockId;
		public TrialSettings trialSettings = new();
	}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region SESSION SETTINGS

	/// <summary> Experiment config container</summary>
	[System.Serializable]
	public class SessionInfo {
		public string experiment = string.Empty;
		public string experimenter = string.Empty;
		public int session_number = 0;
		public List<SettingsTuple> participant_details = new List<SettingsTuple>();

		//? Class helper methods
		public string[] GetSessionSummary() {
			return new string[] { experiment, experimenter, GetParticipantID(), session_number.ToString() };
		}

		public string GetParticipantID() {
			return participant_details.Find(x => x.key == "id").value;
		}

		public Dictionary<string, object> GetParticipantDetailsAsDict() {
			return Helpers.GetSettingsTupleListAsDict(participant_details);
		}
	}

	public static class SessionSettings {
		public static SessionInfo sessionInfo = new();	
	}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HELPERS

	public static class Helpers {

		public static Dictionary<string, object> GetSettingsTupleListAsDict(List<SettingsTuple> list) {
			Dictionary<string, object> tmp = new Dictionary<string, object>();
			foreach (SettingsTuple st in list)
				if (st.value.Contains(',')) { // it's a list!
					List<string> stringlist = st.value.Split(',').ToList();
					for (int s = 0; s < stringlist.Count; s++) {
						string newstring = stringlist[s].Replace(" ", string.Empty); // remove spaces 
						stringlist[s] = newstring;
					}
					tmp.Add(st.key, stringlist);
				}
				else tmp.Add(st.key, st.value); // normal string

			return tmp;
		}
	}


	/// <summary> Experiment config container</summary>
	[System.Serializable]
	public class TaskConfig {
		//public List<SettingsTuple>		taskSettings 		= new List<SettingsTuple>();
		//public List<int>				breakAfter			= new List<int>(); 
		//public List<ExperimentBlock>		blocks			= new List<ExperimentBlock>();

		//// Local check if this instance is loaded and ready to go
		//public bool 				isReady			= false;

		//? Class helper methods
		//public Dictionary<string,object> GetTaskSettingsAsDict () {
		//	return Helpers.GetSettingsTupleListAsDict(taskSettings);
		//}

		/// <summary>/// Convert JSON formatted definition for the seqence into a UXF format to run in the session/// </summary>
		//public void GenerateUXFSequence() {

		//	// Reorder the taskblock list in the taskmanager
		//	List<TaskBlock> reordered = new List<TaskBlock>();
			
		//	foreach (ExperimentBlock b in blocks) {
		//		reordered.Add(Experiment.Instance.taskBlocks.Find(x => x.block_name == b.block_name));
		//	}

		//	Experiment.Instance.taskBlocks.Clear();
		//	Experiment.Instance.taskBlocks.AddRange(reordered);

		//	// Add block level settings to log
		//	Session.instance.settingsToLog.Add("block_name");

		//	// Convert the Taskconfig into UXF blocks and settings
		//	foreach (ExperimentBlock b in blocks) {
				
		//		Block newBlock = Session.instance.CreateBlock();
		//		newBlock.settings.SetValue("block_name",b.block_name);
		//		newBlock.settings.SetValue("intro",b.intro);
		//		newBlock.settings.SetValue("outro",b.outro);

		//		newBlock.settings.UpdateWithDict( Helpers.GetSettingsTupleListAsDict(b.block_settings) );

		//		foreach (ValueList row in b.trial_settings.valueList) {
		//			Trial newTrial = newBlock.CreateTrial();

		//			for (int i = 0; i < row.values.Count; i++) {
		//				newTrial.settings.SetValue( b.trial_settings.keys[i], row.values[i].ToUpper() ); // set values to trial
		//			}
		//		}

		//		// Log all unique trial settings keys
		//		foreach (string k in b.trial_settings.keys) {
		//			if (!Session.instance.settingsToLog.Contains(k))
		//				Session.instance.settingsToLog.Add(k);
		//		}

		//	}
		//}
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

		public Constants.Interactor VisableInteractor = Constants.Interactor.NONE;
		public Constants.Interactor InteractiveInteractor = Constants.Interactor.RIGHT;
		public int screenResolution = 0;
		public float volume = 50f;
		public Constants.Languages language = Constants.Languages.ENG;

		public string pathToLogfiles = "C:\\";
		public static string localConfigDirectoryName = "Configs";
		
	}


#endregion // -------------------------------------------------------------------------------------------------------------------------------

}