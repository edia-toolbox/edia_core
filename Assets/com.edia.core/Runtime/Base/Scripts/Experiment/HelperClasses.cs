using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;
using UXF;

namespace Edia {

	#region SESSION CONFIG (JSON SERIALIZABLE)

	// \cond \hiderefs
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
	public class XBlockSequence {
		public List<string> Sequence = new();
	}

	[System.Serializable]
	public class XBlockBaseSettings {
		public string type;
		public string subType;
		public List<SettingsTuple> settings = new();
		public List<SettingsTuple> instructions = new();
	}

	//! Task list
	[System.Serializable]
	public class XBlockSettings : XBlockBaseSettings {
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
				if (st.value.Contains(';')) { // it's a list!
					List<string> stringlist = st.value.Split(';').ToList();
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


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region XBLOCK RELATED STATE MACHINE

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

		public Constants.Manipulator VisibleSide = Constants.Manipulator.NONE;
		public Constants.Manipulator InteractiveSide = Constants.Manipulator.RIGHT;

		public string pathToLogfiles = FileManager.GetCorrectPath() + "/logfiles";
	}

	// \endcond

#endregion // -------------------------------------------------------------------------------------------------------------------------------

}