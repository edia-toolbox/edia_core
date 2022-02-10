using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UXF;

namespace eDIA {

	public class ExperimentControl : ExperimenterPanel {

		// Default buttons that are always needed for running a experiment
		[Header("Default buttons")]
		public Button btnExperiment = null;
		public Button btnPauseExperiment = null;
		public Button btnProceedExperiment = null;

		[Header("Statemachine panels")]
		public GameObject panelIdle = null;
		public GameObject panelRunning = null;
		public GameObject panelStatus = null;
		public GameObject panelInfo = null;

		[Header("Experiment status")]
		public ExperimenterCanvasStatusSlider trialSlider;
		public ExperimenterCanvasStatusSlider blockSlider;

		[Header("Session info")]
		public TextMeshProUGUI experimentNameField = null;
		public TextMeshProUGUI experimenterField = null;
		public TextMeshProUGUI participantIDField = null;
		public TextMeshProUGUI sessionNumberField = null;

		private void Awake() {
			EventManager.StartListening("EvExperimentInitialised", 	OnEvExperimentInitialised);
			EventManager.StartListening("EvButtonChangeState", 		OnEvButtonChangeState);
			EventManager.StartListening("EvStartExperiment", 		OnEvStartExperiment);

			HidePanel ();
		}

#region EVENT LISTENERS

		void OnEvExperimentInitialised(eParam obj)
		{
			EventManager.StopListening("EvExperimentInitialised", OnEvExperimentInitialised);

			SetupButtons ();
			btnExperiment.interactable = true;

			blockSlider.description = "ready";

			panelIdle.SetActive(true);
			panelRunning.SetActive(false);
			panelStatus.SetActive(false);
			panelInfo.SetActive(false);
			ShowPanel ();

			EventManager.StartListening("EvSetDisplayInformation", OnEvSetDisplayInformation);
		}

		void OnEvStartExperiment (eParam e)
		{
			EventManager.StopListening("EvStartExperiment", OnEvStartExperiment);
			btnExperiment.onClick.RemoveAllListeners();

			// Setting up sliders
			trialSlider.maxValue = Session.instance.LastTrial.number;
			trialSlider.description = "Trials";

			blockSlider.maxValue = Session.instance.blocks.Count;
			blockSlider.description = "Started";

			panelIdle.SetActive(false);
			panelRunning.SetActive(true);
			panelStatus.SetActive(true);

			EventManager.StartListening("EvExperimentInfoUpdate", OnEvExperimentInfoUpdate);
			EventManager.StartListening("EvFinalizeSession", OnEvFinalizeSession);
		}

		private void OnEvSetDisplayInformation(eParam e)
		{
			string[] displayInformation = ExperimentManager.Instance.experimentConfig.GetExperimentDisplayInformation();
			experimentNameField.text = displayInformation[0];
			experimenterField.text = displayInformation[1];
			participantIDField.text = displayInformation[2];
			sessionNumberField.text = displayInformation[3];

			panelInfo.SetActive(true);
		}


		private void OnEvFinalizeSession(eParam obj)
		{
			EventManager.StopListening("EvFinalizeSession", OnEvFinalizeSession);
			EventManager.StopListening("EvSetDisplayInformation", OnEvSetDisplayInformation);

			btnExperiment.transform.GetChild(0).GetComponentInChildren<Text>().text = "Quit";
			btnExperiment.onClick.AddListener(()=>EventManager.TriggerEvent("EvQuitApplication", null));
			btnExperiment.interactable = true;

			panelIdle.SetActive(true);
			panelRunning.SetActive(false);
			panelStatus.SetActive(false);
		}

		void OnEvExperimentInfoUpdate (eParam e) {
			trialSlider.currentValue = Session.instance.currentTrialNum; //TODO not modular yet!
			blockSlider.currentValue = Session.instance.currentBlockNum;

			if (e != null)
				blockSlider.description = e.GetString();
		}

		void OnEvButtonChangeState(eParam e) {

			bool newState = e.GetStrings()[1].ToUpper() == "TRUE";
			Constants.ExperimenterCanvasButtons btn = (Constants.ExperimenterCanvasButtons)int.Parse(e.GetStrings()[0]);

			switch (btn) {
				case Constants.ExperimenterCanvasButtons.EXP_START :
					btnExperiment.interactable = newState;
				break;
				case Constants.ExperimenterCanvasButtons.EXP_PAUSE :
					btnPauseExperiment.interactable = newState;
				break;
				case Constants.ExperimenterCanvasButtons.EXP_PROCEED :
					btnProceedExperiment.interactable = newState;
				break;
			}
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BUTTONPRESSES

		public void BtnNewSessionPressed () {
			EventManager.TriggerEvent("EvNewSession", null);
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------

		void OnDestroy() {
			EventManager.StopListening("EvExperimentInitialised", OnEvExperimentInitialised);
			EventManager.StopListening("EvButtonChangeState", OnEvButtonChangeState);
			EventManager.StopListening("EvFinalizeSession", OnEvFinalizeSession);
			EventManager.StopListening("EvStartExperiment", OnEvFinalizeSession);
		}

		void SetupButtons () {
			btnExperiment.transform.GetChild(0).GetComponentInChildren<Text>().text = "Start Experiment";
			btnExperiment.onClick.AddListener(	()=>EventManager.TriggerEvent("EvStartExperiment", null));
			btnPauseExperiment.onClick.AddListener(	()=>EventManager.TriggerEvent("EvPauseExperiment", null));
			btnProceedExperiment.onClick.AddListener(	()=>EventManager.TriggerEvent("EvProceed", null));
		}

	}
}