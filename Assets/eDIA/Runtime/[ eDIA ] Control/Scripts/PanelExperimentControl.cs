using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UXF;

namespace eDIA {

	public class PanelExperimentControl : ExperimenterPanel {

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
		public SliderExperimenterStatus trialSlider;
		public SliderExperimenterStatus blockSlider;

		[Header("Session info")]
		public TextMeshProUGUI experimentNameField = null;
		public TextMeshProUGUI experimenterField = null;
		public TextMeshProUGUI participantIDField = null;
		public TextMeshProUGUI sessionNumberField = null;

		public override void Awake() {

			base.Awake();

			EventManager.StartListening(eDIA.Events.Core.EvExperimentConfigSet, 	OnEvExperimentConfigSet);
			EventManager.StartListening(eDIA.Events.Core.EvTaskConfigSet, 		OnEvTaskConfigSet);
			EventManager.StartListening(eDIA.Events.Core.EvReadyToGo,			OnEvReadyToGo);
			EventManager.StartListening("EvButtonChangeState", 		OnEvButtonChangeState);
			EventManager.StartListening("EvStartExperiment", 		OnEvStartExperiment);

		}


		void Start() {
			HidePanel();


		}

#region EVENT LISTENERS

		void OnEvExperimentConfigSet(eParam obj)
		{
			EventManager.StopListening(eDIA.Events.Core.EvExperimentConfigSet, OnEvExperimentConfigSet);

			GetComponent<VerticalLayoutGroup>().enabled = true;
			ShowPanel();

			panelIdle.SetActive(false);
			panelRunning.SetActive(false);
			panelStatus.SetActive(false);

			EventManager.StartListening("EvSetDisplayInformation", OnEvDisplaySessionInfo);
		}

		void OnEvTaskConfigSet(eParam obj)
		{
			EventManager.StopListening(eDIA.Events.Core.EvTaskConfigSet, OnEvTaskConfigSet);

			GetComponent<VerticalLayoutGroup>().enabled = true;
			ShowPanel();
		}

		void OnEvReadyToGo(eParam obj)
		{
			EventManager.StopListening(eDIA.Events.Core.EvReadyToGo,			OnEvReadyToGo);

			SetupButtons ();
			btnExperiment.interactable = true;

			blockSlider.description = "ready";

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
			GetComponent<VerticalLayoutGroup>().enabled = true;

			EventManager.StartListening("EvDisplaySessionInfo", OnEvDisplaySessionInfo);
			EventManager.StartListening("EvExperimentProgressUpdate", OnEvExperimentProgressUpdate);
			EventManager.StartListening("EvFinalizeSession", OnEvFinalizeSession);
		}

		private void OnEvDisplaySessionInfo(eParam e)
		{
			string[] displayInformation = e.GetStrings();
			
			experimentNameField.text = displayInformation[0];
			experimenterField.text = displayInformation[1];
			participantIDField.text = displayInformation[2];
			sessionNumberField.text = displayInformation[3];

			panelInfo.SetActive(true);
			GetComponent<VerticalLayoutGroup>().enabled = true;
		}


		private void OnEvFinalizeSession(eParam obj)
		{
			EventManager.StopListening("EvFinalizeSession", OnEvFinalizeSession);
			EventManager.StopListening("EvSetDisplayInformation", OnEvDisplaySessionInfo);

			btnExperiment.transform.GetChild(0).GetComponentInChildren<Text>().text = "Quit";
			btnExperiment.onClick.AddListener( ()=> EventManager.TriggerEvent(eDIA.Events.Core.EvQuitApplication, null));
			btnExperiment.interactable = true;

			panelIdle.SetActive(true);
			panelRunning.SetActive(false);
			panelStatus.SetActive(false);
			GetComponent<VerticalLayoutGroup>().enabled = true;
		}

		void OnEvExperimentProgressUpdate (eParam e) {
			trialSlider.currentValue = Session.instance.currentTrialNum; //TODO not modular yet! so won't work if it's on a remote tablet or something
			blockSlider.currentValue = Session.instance.currentBlockNum;

			blockSlider.description = e == null ? "" : blockSlider.description = e.GetString();
		}

		void OnEvButtonChangeState(eParam e) {

			bool newState = e.GetStrings()[1].ToUpper() == "TRUE";

			switch (e.GetStrings()[0].ToUpper()) {
				case "PAUSE" :
					btnPauseExperiment.interactable = newState;
				break;
				case "PROCEED" :
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
			EventManager.StopListening("EvExperimentConfigSet", OnEvExperimentConfigSet);
			EventManager.StopListening("EvFinalizeSession", OnEvFinalizeSession);
			EventManager.StopListening("EvStartExperiment", OnEvFinalizeSession);

			btnExperiment.onClick.RemoveListener(		()=>EventManager.TriggerEvent("EvStartExperiment", null));
			btnPauseExperiment.onClick.RemoveListener(	()=>EventManager.TriggerEvent("EvPauseExperiment", null));
			btnProceedExperiment.onClick.RemoveListener(	()=>EventManager.TriggerEvent(eDIA.Events.Core.EvProceed, null));

		}

		void SetupButtons () {
			btnExperiment.transform.GetChild(0).GetComponentInChildren<Text>().text = "Start Experiment";
			btnExperiment.onClick.AddListener(		()=>EventManager.TriggerEvent("EvStartExperiment", null));
			btnPauseExperiment.onClick.AddListener(	()=>EventManager.TriggerEvent("EvPauseExperiment", null));
			btnProceedExperiment.onClick.AddListener(	()=>EventManager.TriggerEvent(eDIA.Events.Core.EvProceed, null));
		}

	}
}