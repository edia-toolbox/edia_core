using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using eDIA;
using UXF;
using Utils;

namespace eDia {
    public class TaskStroop : EBlock {

        public GameObject StroopCanvas;

        public TextMeshProUGUI _txtStroopObj;

        IDisposable _ButtonPressEventListener;


        void Awake() {
            trialSteps.Add(TrialStep1);
            trialSteps.Add(TrialStep2);
            trialSteps.Add(TrialStep3);

            StroopCanvas.SetActive(false);
        }

        void Start() {
			// _txtStroopObj = StroopCanvas.GetComponentInChildren<TextMeshProUGUI>();

			foreach (string s in Session.instance.CurrentBlock.settings.GetStringList("_start")) {
                this.Add2Console(s);
            }
        }


        private void OnEnable() {

        }

        private void OnDisable() {
            if (_ButtonPressEventListener != null) {
                _ButtonPressEventListener.Dispose();
            }
        }

        public void TrialStep1() {

            Debug.Log("TrialStep1");
            //Debug.Log($"Trial: {_trialIndex}");

            // Get the word and it's color from settings
            // set the properties
            // Show the word
            // Proceed
            //Color col = _trials[_trialIndex].ColValue;
            //string name = _trials[_trialIndex].ColName;

            Color col = Color.white;
			Color newcol;
			if (ColorUtility.TryParseHtmlString(Session.instance.CurrentTrial.settings.GetString("color"), out newcol))
				col = newcol;

            foreach (var s in Session.instance.CurrentBlock.settings.Keys) {
                this.Add2Console(s);
            }

            string name = Session.instance.CurrentTrial.settings.GetString("word");

			StroopCanvas.SetActive(true);
            _txtStroopObj.text = name;
            _txtStroopObj.color = col;
            Experiment.Instance.ProceedWithDelay(0.1f);

        }

        void TrialStep2() {
            // Wait for response
            // Evaluate response
            // Store results
            // Proceed

            var response = "";
            response = ListenToInput();
            //return;
        }

        void TrialStep3() {
            // Clean up
            StroopCanvas.SetActive(false);
            Experiment.Instance.WaitOnProceed();
            Experiment.Instance.Proceed();
            return;
        }

        string ListenToInput() {
            var response = "";
            _ButtonPressEventListener = InputSystem.onAnyButtonPress.CallOnce(
                (ctrl) => {
                    InterpretInputNew(ctrl, out response, CheckLogResultsProceed);
                }
            );

            return response;
        }


        void CheckLogResultsProceed(string response) {

            _ButtonPressEventListener.Dispose();

            // Evaluate results
            //var invDict = invertDict();

            string target;
            string targetDomain = Session.instance.CurrentTrial.settings.GetString("target").ToLower();
            
            target = Session.instance.CurrentTrial.settings.GetString(targetDomain).ToLower();

            if (targetDomain == "color") {
                response = Session.instance.CurrentTrial.settings.GetString(response).ToLower();
            }

            if (response == target) {
                Debug.Log("Correct");
            } else {
                Debug.Log($"Incorrect --- Correct answer: {target} --- Your answer: {response}");
            }

            // Log settings
            Experiment.Instance.AddToTrialResults("word", Session.instance.CurrentTrial.settings.GetString("word"));
			Experiment.Instance.AddToTrialResults("color", Session.instance.CurrentTrial.settings.GetString("color"));
			Experiment.Instance.AddToTrialResults("target", Session.instance.CurrentTrial.settings.GetString("target"));

            // Log results
			Experiment.Instance.AddToTrialResults("response",response);
            
            // Proceed
            Experiment.Instance.WaitOnProceed();
			Experiment.Instance.Proceed();

            return;
        }


        void InterpretInputNew(InputControl control, out string response, Action<string> callback) {
            Debug.Log($"Key pressed: {control.displayName.ToLower()}");
            var input = control.displayName.ToLower();
            switch (input) {
                case "r":
                    response = "red";
                    break;
                case "g":
                    response = "green";
                    break;
                case "b":
                    response = "blue";
                    break;
                case "y":
                    response = "yellow";
                    break;
                case "p":
                    response = "pink";
                    break;
                case "o":
                    response = "orange";
                    break;
                default:
                    response = "";
                    ListenToInput();
                    break;
            }
            if (response != "") {
                callback(response);
            }

        }
    }
}


