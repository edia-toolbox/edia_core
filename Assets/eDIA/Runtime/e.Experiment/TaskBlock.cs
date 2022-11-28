using System;
using System.Collections.Generic;
using eDIA;
using UnityEngine;

namespace eDIA {

    [System.Serializable]
    public class TaskBlock : MonoBehaviour {

        [Header ("Block name, use the name defined in the config")]
        public string name;

        [SerializeField]//[HideInInspector]
        public List<Action> trialSteps = new List<Action> ();

        public void AddToTrialSequence (Action methodStep) {
            trialSteps.Add(methodStep);
        }

    #region EVENT HOOKS

        public virtual void OnBlockStart () { }
        public virtual void OnBlockIntroduction () { }
        public virtual void OnBlockResumeAfterIntro () { }
        public virtual void OnStartNewTrial () { }
        public virtual void OnBetweenSteps () { }
        public virtual void OnBlockEnd () { }

    #endregion // -------------------------------------------------------------------------------------------------------------------------------
    }
}