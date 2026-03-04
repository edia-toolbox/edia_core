using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Edia;


namespace Edia {

	public class Break : XBlock {
		void Awake() {
			trialSteps.Add(BreakStep1);
		}

		void BreakStep1() {
			Experiment.Instance.ShowMessageToUser (Experiment.Instance.CurrentBlock.settings.GetStringList("_info"));

			if (Experiment.Instance.CurrentBlock.settings.GetBool("fadetoblack")) {
				AddToConsoleLog("fade to black");
				XRManager.Instance.HideVR();
			}

			Experiment.Instance.WaitOnProceed();
		}
	}
}