using System;
using System.Collections.Generic;
using eDIA;
using UnityEngine;

namespace eDIA {

    [System.Serializable]
    public class EBlock: MonoBehaviour {

        [Header("Debug")]
        public bool showLog = false;
        Color taskColor = Color.blue;

        [HideInInspector][SerializeField]
        public List<Action> trialSteps = new List<Action> ();

        public void AddToTrialSequence (Action methodStep) {
            trialSteps.Add(methodStep);
        }


		//private void AddToLog(string _msg) {
		//	if (showLog)
		//		eDIA.LogUtilities.AddToLog(_msg, "EXP", taskColor);
		//}


		#region EVENT HOOKS

		public virtual void OnBlockStart () {}
        public virtual void OnBlockIntro () {}
        public virtual void OnStartTrial () {}
        public virtual void OnEndTrial () {}
        public virtual void OnBetweenSteps () {}
        public virtual void OnBlockOutro () {}
        public virtual void OnBlockEnd () {}

    #endregion // -------------------------------------------------------------------------------------------------------------------------------
    }
}