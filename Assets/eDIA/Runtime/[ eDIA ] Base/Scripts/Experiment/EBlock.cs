using System;
using System.Collections.Generic;
using eDIA;
using UnityEngine;

namespace eDIA {

	[System.Serializable]
	public class EBlock : MonoBehaviour {

		[Header("Debug")]
		public bool showLog = false;
		Color taskColor = Color.blue;

		[HideInInspector]
		[SerializeField]
		public List<Action> trialSteps = new List<Action>();

		public void AddToTrialSequence(Action methodStep) {
			trialSteps.Add(methodStep);
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