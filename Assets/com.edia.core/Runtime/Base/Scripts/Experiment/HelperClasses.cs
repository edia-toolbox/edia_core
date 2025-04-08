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
        public string Key = string.Empty;

        public string Value = string.Empty;
    }

    /// <summary> List of string values, in a class to make it serializable by JSON</summary>
    [System.Serializable]
    public class ValueList {
        public List<string> Values = new List<string>();
    }

    ///// <summary> Experiment trial settings container</summary>
    [System.Serializable]
    public class TrialSettings {
        [HideInInspector]
        public List<string> Keys = new List<string>();

        public List<ValueList> ValueList = new List<ValueList>();
    }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region UXF SEQUENCE (JSON SERIALIZABLE)

    [System.Serializable]
    public class XBlockSequence {
        public List<string> Sequence = new();
    }

    [System.Serializable]
    public class XBlockBaseSettings {
        public string              Type;
        public string              SubType;
        public List<SettingsTuple> Settings     = new();
        public List<SettingsTuple> Instructions = new();
    }

    //! Task list
    [System.Serializable]
    public class XBlockSettings : XBlockBaseSettings {
        public string        BlockId;
        public TrialSettings TrialSettings = new();
    }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region SESSION SETTINGS

    /// <summary> Experiment config container</summary>
    [System.Serializable]
    public class SessionInfo {
        public string              Experiment         = string.Empty;
        public string              Experimenter       = string.Empty;
        public int                 SessionNumber      = 0;
        public List<SettingsTuple> ParticipantDetails = new();

        //? Class helper methods
        public string[] GetSessionSummary() {
            return new string[] { Experiment, Experimenter, GetParticipantID(), SessionNumber.ToString() };
        }

        public string GetParticipantID() {
            return ParticipantDetails.Find(x => x.Key == "id").Value;
        }

        public Dictionary<string, object> GetParticipantDetailsAsDict() {
            return Helpers.GetSettingsTupleListAsDict(ParticipantDetails);
        }
    }

    public static class SessionSettings {
        public static List<SettingsTuple> Settings     = new();
        public static List<SettingsTuple> Instructions = new();
        public static SessionInfo         SessionInfo  = new();
    }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region HELPERS

    public static class Helpers {

        public static Dictionary<string, object> GetSettingsTupleListAsDict(List<SettingsTuple> list) {
            Dictionary<string, object> tmp = new Dictionary<string, object>();
            foreach (SettingsTuple st in list)
                if (st.Value.Contains(';')) {
                    // it's a list!
                    List<string> stringlist = st.Value.Split(';').ToList();
                    for (int s = 0; s < stringlist.Count; s++) {
                        string newstring = stringlist[s].Replace(" ", string.Empty); // remove spaces 
                        stringlist[s] = newstring;
                    }

                    tmp.Add(st.Key, stringlist);
                }
                else tmp.Add(st.Key, st.Value); // normal string

            return tmp;
        }
    }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region XBLOCK RELATED STATE MACHINE

    /// <summary>One step of a trial</summary>
    [System.Serializable]
    public class TrialStep {
        public string title;
        public Action methodToCall;

        public TrialStep(string title, Action methodToCall) {
            this.title        = title;
            this.methodToCall = methodToCall;
        }
    }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region SYSTEM RELATED

    /// <summary>Container to hold main settings of the application </summary>
    [System.Serializable]
    public class SettingsDeclaration {

        public string VisibleSide     = "NONE";
        public string InteractiveSide = "NONE";

        public string pathToLogfiles = "../logfiles";
    }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

}