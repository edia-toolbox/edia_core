using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Edia;
using UnityEngine;

namespace Edia {

    [System.Serializable]
    public class XBlock : MonoBehaviour {

        [Header("Debug")]
        [XblockHeader("Xblock")]
        public bool ShowConsoleMessages = false;

        [HideInInspector]
        [SerializeField]
        public List<Action> trialSteps = new List<Action>();

        public void AddToTrialSequence(Action methodStep) {
            trialSteps.Add(methodStep);
        }

        /// <summary>
        /// Adds labeled and colored message to console if `ShowConsoleMessages` is enabled. Handy for debugging.
        /// </summary>
        /// <param name="_msg">Message to show</param>
        public void AddToConsoleLog(string _msg) {
            if (ShowConsoleMessages)
                Edia.LogUtilities.AddToConsoleLog(_msg, this.name);
        }

#region ---- EVENT HOOKS

        public virtual void OnBlockStart() {
        }

        public virtual void OnStartTrial() {
        }

        public virtual void OnEndTrial() {
        }

        public virtual void OnBetweenSteps() {
        }

        public virtual void OnBlockEnd() {
        }

        public virtual void OnBlockOutro() {
        }

#endregion
    }
}