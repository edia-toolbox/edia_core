using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Edia;
using UXF;

namespace Edia {

	public class Break : XBlock {
		void Awake() {
			trialSteps.Add(BreakStep1);
		}

		void BreakStep1() {
			MessagePanelInVR.Instance.ShowMessage(Session.instance.CurrentBlock.settings.GetStringList("_info"));
			Experiment.Instance.WaitOnProceed();
		}
	}
}