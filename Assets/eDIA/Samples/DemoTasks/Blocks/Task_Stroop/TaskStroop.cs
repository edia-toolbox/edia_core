using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using eDIA;
using UXF;
using UnityEngine.UI;

namespace eDia {
    public class TaskStroop : XBlock {

        [Space(20)]
        public ScreenInVR StroopCanvas;
        public TextMeshProUGUI _txtStroopObj;
        public List<StimuliStroop> Stimulis;

        void Awake() {
            trialSteps.Add(SetupTaskEnvironment);
            trialSteps.Add(WaitForUserInput);
            trialSteps.Add(CleanUp);

            StroopCanvas.Show(false);
		}

        public void SetupTaskEnvironment() {

			// Prepare stimuli buttons
			for (int s=0;s<Stimulis.Count;s++) {
                Stimulis[s].Init(
                    Session.instance.CurrentBlock.settings.GetStringList("colors")[s],
                    Session.instance.CurrentBlock.settings.GetStringList("words")[s],
                    Session.instance.CurrentBlock.settings.GetString("target"),
					Session.instance.CurrentBlock.settings.GetString("target") == "color" ? Session.instance.CurrentTrial.settings.GetString("color") : Session.instance.CurrentTrial.settings.GetString("word")
					);

                int forSomeReasonItNeedsANewVar = s;
				Stimulis[s].GetComponent<Button>().onClick.AddListener(() => StimuliSelected(forSomeReasonItNeedsANewVar));
                Stimulis[s].GetComponent<Button>().interactable = true;
			}

            Color col = Color.white;

			if (Session.instance.CurrentBlock.settings.GetString("target") == "color")
                    ColorUtility.TryParseHtmlString(Session.instance.CurrentTrial.settings.GetString("color"), out col);

			_txtStroopObj.text  = Session.instance.CurrentTrial.settings.GetString("word");
            _txtStroopObj.color = col;

			StroopCanvas.Show(true);

			Xperiment.Instance.WaitOnProceed();
			Xperiment.Instance.ProceedWithDelay(0.1f);
        }


        void WaitForUserInput() {
			XRManager.Instance.EnableXRRayInteraction(true);

			Xperiment.Instance.WaitOnProceed();
        }

        public void StimuliSelected (int stimuliIndex) {

            CheckLogResultsProceed(stimuliIndex);
			XRManager.Instance.EnableXRRayInteraction(false);

			Xperiment.Instance.Proceed();
		}
        
        void CleanUp() {
            // Clean up
            foreach (var stimuli in Stimulis)
				stimuli.GetComponent<Button>().onClick.RemoveAllListeners();

			StroopCanvas.Show(false);

			Xperiment.Instance.WaitOnProceed();
            Xperiment.Instance.ProceedWithDelay(1f);
        }


        void CheckLogResultsProceed(int stimuliIndex) {

            Debug.Log($"Correct {Stimulis[stimuliIndex].IsValid}");

            // Log settings
            Xperiment.Instance.AddToTrialResults("word", Session.instance.CurrentTrial.settings.GetString("word"));
			Xperiment.Instance.AddToTrialResults("color", Session.instance.CurrentTrial.settings.GetString("color"));
			Xperiment.Instance.AddToTrialResults("target", Session.instance.CurrentBlock.settings.GetString("target"));

            // Log results
            Xperiment.Instance.AddToTrialResults("response", Stimulis[stimuliIndex].GetValue());
        }

		public override void OnBlockEnd() {
			StroopCanvas.Show(false);

			base.OnBlockEnd();
		}
	}
}


