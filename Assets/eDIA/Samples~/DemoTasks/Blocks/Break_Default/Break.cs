using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using eDIA;
using UXF;

namespace eDIA {

	public class Break : EBlock {
		void Awake() {
			trialSteps.Add(BreakStep1);
		}

		void BreakStep1() {
			MessagePanelInVR.Instance.ShowMessage(Session.instance.CurrentBlock.settings.GetStringList("_info"));
			Experiment.Instance.WaitOnProceed();
		}
	}
}