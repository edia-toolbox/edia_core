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
			Experiment.Instance.ShowMessageToUser (Session.instance.CurrentBlock.settings.GetStringList("_info"));

			if (Session.instance.CurrentBlock.settings.GetBool("fadetoblack")) {
				this.Add2Console("fade to black");
				XRManager.Instance.HideVR();
			}

			Experiment.Instance.WaitOnProceed();
		}
	}
}