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
        public TextMeshProUGUI StroopTextfield;
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
					Session.instance.CurrentBlock.settings.GetString("target") == "color" ? 
                        Session.instance.CurrentTrial.settings.GetString("color") : 
                        Session.instance.CurrentTrial.settings.GetString("word")
					);

                int uniqueGeneratedId = s;
				Stimulis[s].GetComponent<Button>().onClick.AddListener(() => StimuliSelected(uniqueGeneratedId));
                Stimulis[s].GetComponent<Button>().interactable = true;
			}

            Color col = Color.white;
            ColorUtility.TryParseHtmlString(Session.instance.CurrentTrial.settings.GetString("color"), out col);

			StroopTextfield.text  = Session.instance.CurrentTrial.settings.GetString("word");
            StroopTextfield.color = col;

			StroopCanvas.Show(true);

			Experiment.Instance.WaitOnProceed();
			Experiment.Instance.ProceedWithDelay(0.1f);
        }


        void WaitForUserInput() {
			XRManager.Instance.EnableXRRayInteraction(true);

			Experiment.Instance.WaitOnProceed();
        }

        public void StimuliSelected (int stimuliIndex) {

            CheckLogResultsProceed(stimuliIndex);
			XRManager.Instance.EnableXRRayInteraction(false);

			Experiment.Instance.Proceed();
		}
        
        void CleanUp() {
            // Clean up
            foreach (var stimuli in Stimulis)
				stimuli.GetComponent<Button>().onClick.RemoveAllListeners();

			StroopCanvas.Show(false);

			Experiment.Instance.WaitOnProceed();
            Experiment.Instance.ProceedWithDelay(1f);
        }


        void CheckLogResultsProceed(int stimuliIndex) {

            Debug.Log($"Correct {Stimulis[stimuliIndex].IsValid}");

            // Log settings
            Experiment.Instance.AddToTrialResults("word", Session.instance.CurrentTrial.settings.GetString("word"));
			Experiment.Instance.AddToTrialResults("color", Session.instance.CurrentTrial.settings.GetString("color"));
			Experiment.Instance.AddToTrialResults("target", Session.instance.CurrentBlock.settings.GetString("target"));

            // Log results
            Experiment.Instance.AddToTrialResults("response", Stimulis[stimuliIndex].GetValue());
        }

		public override void OnBlockEnd() {
			StroopCanvas.Show(false);

			base.OnBlockEnd();
		}
	}
}


