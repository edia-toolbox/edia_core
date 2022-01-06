using System.Linq;
using System.ComponentModel.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UXF;

namespace eDIA {

	public class ExperimenterCanvas : MonoBehaviour {

		// Default buttons that are always needed for running a experiment
		[Header("Default buttons")]
		public Button btnStartExperiment = null;
		public Button btnPauseExperiment = null;
		public Button btnProceedExperiment = null;
		public Button btnNewSession = null;
		public Button btnEyeCalibration = null;

		public ExperimenterCanvasStatusSlider trialSlider;
		public ExperimenterCanvasStatusSlider blockSlider;


		private void Awake() {
			EventManager.StartListening("EvExperimentInitialised", OnEvExperimentInitialised);
			EventManager.StartListening("EvButtonChangeState", OnEvButtonChangeState);
			EventManager.StartListening("EvStartExperiment", OnEvStartExperiment);
		}


		void OnEvExperimentInitialised(eParam obj)
		{
			EventManager.StopListening("EvExperimentInitialised", OnEvExperimentInitialised);
			SetupButtons ();
			btnStartExperiment.interactable = true;
		}

		void OnEvStartExperiment (eParam e) {
			EventManager.StopListening("EvStartExperiment", OnEvStartExperiment);

			// Setting up sliders
			trialSlider.maxValue = Session.instance.LastTrial.number;
			trialSlider.description = "Trials";

			blockSlider.maxValue = Session.instance.blocks.Count;
			blockSlider.description = "Blocks";

			EventManager.StartListening("EvExperimentInfoUpdate", OnEvExperimentInfoUpdate);
		}

		void OnEvExperimentInfoUpdate (eParam e) {
			trialSlider.currentValue = Session.instance.currentTrialNum;
			blockSlider.currentValue = Session.instance.currentBlockNum;

			if (e != null)
				blockSlider.description = e.GetString();
		}

		public void BtnNewSessionPressed () {
			EventManager.TriggerEvent("EvNewSession", null);
		}

		void OnDestroy() {
			EventManager.StartListening("EvExperimentInitialised", OnEvExperimentInitialised);
			EventManager.StopListening("EvButtonChangeState", OnEvButtonChangeState);
		}

		void SetupButtons () {
			btnStartExperiment.onClick.AddListener(	()=>EventManager.TriggerEvent("EvStartExperiment", null));
			btnPauseExperiment.onClick.AddListener(	()=>EventManager.TriggerEvent("EvPauseExperiment", null));
			btnProceedExperiment.onClick.AddListener(	()=>EventManager.TriggerEvent("EvProceed", null));
			btnEyeCalibration.onClick.AddListener(	()=>EventManager.TriggerEvent("EvEyeCalibrationRequested", null));
		}

		void OnEvButtonChangeState(eParam e) {

			bool newState = e.GetStrings()[1].ToUpper() == "TRUE";
			ExperimenterCanvasButtons btn = (ExperimenterCanvasButtons)int.Parse(e.GetStrings()[0]);

			switch (btn) {
				case ExperimenterCanvasButtons.EXP_START :
					btnStartExperiment.interactable = newState;
				break;
				case ExperimenterCanvasButtons.EXP_PAUSE :
					btnPauseExperiment.interactable = newState;
				break;
				case ExperimenterCanvasButtons.EXP_PROCEED :
					btnProceedExperiment.interactable = newState;
				break;
				case ExperimenterCanvasButtons.SES_NEW :
					btnNewSession.interactable = newState;
				break;
				case ExperimenterCanvasButtons.EYE_CALIBRATION :
					btnEyeCalibration.interactable = newState;
				break;
			}
		}

	}
}