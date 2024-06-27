using System;
using System.Collections.Generic;
using Edia;
using UnityEngine;

namespace Edia {

	[System.Serializable]
	public class XBlock : MonoBehaviour {

		[Header("Debug")]
		public bool ShowLog = false;
		Color taskColor = Color.blue;
		[HideInInspector]
		[SerializeField]
		public List<Action> trialSteps = new List<Action>();

		public void AddToTrialSequence(Action methodStep) {
			trialSteps.Add(methodStep);
		}

		public void AddToLog(string _msg) {

			if (ShowLog)
				Edia.LogUtilities.AddToLog(_msg, "TASK", taskColor);
		}

		#region EVENT HOOKS

		public virtual void OnBlockStart() { }
		public virtual void OnStartTrial() { }
		public virtual void OnEndTrial() { }
		public virtual void OnBetweenSteps() { }
		public virtual void OnBlockOutro() { }
		public virtual void OnBlockEnd() { }

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}
}