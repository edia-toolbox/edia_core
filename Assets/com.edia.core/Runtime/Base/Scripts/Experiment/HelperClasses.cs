using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Edia {

#region SESSION CONFIG (JSON SERIALIZABLE)
    /// !!
    ///     Classes have intentionally lower case field in order to parse from JSON directly. 
    /// !!

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
    /// !!
    ///     Classes have intentionally lower case field in order to parse from JSON directly. 
    /// !!
    
    [System.Serializable]
    public class XBlockSequence {
        public List<string> sequence = new();
    }

    [System.Serializable]
    public class XBlockBaseSettings {
        public string              type;
        public string              subType;
        public List<SettingsTuple> settings     = new();
        public List<SettingsTuple> instructions = new();
    }

    //! Task list
    [System.Serializable]
    public class XBlockSettings : XBlockBaseSettings {
        public string        blockId;
        public TrialSettings trialSettings = new();
    }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region SESSION SETTINGS
    /// !!
    ///     Classes have intentionally lower case field in order to parse from JSON directly. 
    /// !!
    
    /// <summary> Experiment config container</summary>
    [System.Serializable]
    public class SessionInfo {
        public string              experiment         = string.Empty;
        public string              experimenter       = string.Empty;
        public int                 sessionNumber      = 0;
        public List<SettingsTuple> participantDetails = new();

        //? Class helper methods
        public string[] GetSessionSummary() {
            return new string[] { experiment, experimenter, GetParticipantID(), sessionNumber.ToString() };
        }

        public string GetParticipantID() {
            return participantDetails.Find(x => x.key == "id").value;
        }

        public Dictionary<string, object> GetParticipantDetailsAsDict() {
            return Helpers.GetSettingsTupleListAsDict(participantDetails);
        }
    }

    public static class SessionSettings {
        public static SessionInfo         sessionInfo  = new();
        public static List<SettingsTuple> settings = new();
    }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region HELPERS

    public static class Helpers {

        public static Dictionary<string, object> GetSettingsTupleListAsDict(List<SettingsTuple> list) {
            Dictionary<string, object> tmp = new Dictionary<string, object>();
            foreach (SettingsTuple st in list)
                if (st.value.Contains(';')) {
                    // it's a list!
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