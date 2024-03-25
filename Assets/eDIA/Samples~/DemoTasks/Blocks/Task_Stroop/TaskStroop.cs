using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Edia;
using UXF;
using Utils;
using UnityEngine.UI;

namespace eDia {
    public class TaskStroop : XBlock {

        [Space(20)]
        public ScreenInVR StroopCanvas;
        public TextMeshProUGUI StroopTextField;
        public List<StroopResponseFields> StroopResponseFields;

        void Awake() {
            trialSteps.Add(SetupTaskEnvironment);
            trialSteps.Add(WaitForUserInput);
            trialSteps.Add(CleanUp);

            StroopCanvas.Show(false);
		}

        public void SetupTaskEnvironment() {

			// Prepare stimuli buttons
			for (int s=0;s<StroopResponseFields.Count;s++) {
                StroopResponseFields[s].Init(
                    Session.instance.CurrentBlock.settings.GetStringList("_colors")[s],
                    Session.instance.CurrentBlock.settings.GetStringList("_words")[s],
                    Session.instance.CurrentBlock.settings.GetString("target"),
					Session.instance.CurrentBlock.settings.GetString("target") == "color" ? 
                        Session.instance.CurrentTrial.settings.GetString("color") : 
                        Session.instance.CurrentTrial.settings.GetString("word")
					);

                int uniqueGeneratedId = s;
				StroopResponseFields[s].GetComponent<Button>().onClick.AddListener(() => StimuliSelected(uniqueGeneratedId));
                StroopResponseFields[s].GetComponent<Button>().interactable = true;
			}

            Color col = Color.white;
            ColorUtility.TryParseHtmlString(Session.instance.CurrentTrial.settings.GetString("color"), out col);
			StroopTextField.text  = Session.instance.CurrentTrial.settings.GetString("word");
            StroopTextField.color = col;

            StroopTextField.fontSize = Session.instance.CurrentBlock.settings.GetInt("font_size");

            StroopCanvas.Show(true);

			Experiment.Instance.WaitOnProceed();
			Experiment.Instance.ProceedWithDelay(0.1f);
        }


        void WaitForUserInput() {
			XRManager.Instance.EnableXRRayInteraction(true);

			Experiment.Instance.WaitOnProceed();
        }

        public void StimuliSelected (int stimuliIndex) {
			this.Add2Console($"Stimuli id:{stimuliIndex} pressed");

			CheckLogResultsProceed(stimuliIndex);
			XRManager.Instance.EnableXRRayInteraction(false);

			Experiment.Instance.Proceed();
		}
        
        void CleanUp() {
            // Clean up
            foreach (var stimuli in StroopResponseFields)
				stimuli.GetComponent<Button>().onClick.RemoveAllListeners();

			StroopCanvas.Show(false);

			Experiment.Instance.WaitOnProceed();
            Experiment.Instance.ProceedWithDelay(1f);
        }


        void CheckLogResultsProceed(int stimuliIndex) {

            // Log settings
            Experiment.Instance.AddToTrialResults("word", Session.instance.CurrentTrial.settings.GetString("word"));
			Experiment.Instance.AddToTrialResults("color", Session.instance.CurrentTrial.settings.GetString("color"));
			Experiment.Instance.AddToTrialResults("target", Session.instance.CurrentBlock.settings.GetString("target"));

            // Log results
            Experiment.Instance.AddToTrialResults("response", StroopResponseFields[stimuliIndex].GetValue());
            Experiment.Instance.AddToTrialResults("response_correct", StroopResponseFields[stimuliIndex].IsValid.ToString());
        }

		public override void OnBlockEnd() {
			StroopCanvas.Show(false);

			base.OnBlockEnd();
		}
	}
}


