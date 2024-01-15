using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UXF;

namespace eDIA.Manager {

    public class PanelExperimentControl : ExperimenterPanel {

	  // Default buttons that are always needed for running a experiment
	  [Header("Default buttons")]
	  public Button btnExperiment			= null;
	  public Button btnPauseExperiment		= null;
	  public Button btnProceedExperiment	= null;

	  [Header("Statemachine panels")]
	  public GameObject panelIdle			= null;
	  public GameObject panelRunning		= null;
	  public GameObject panelStatus			= null;
	  public GameObject panelInfo			= null;

	  [Header("Experiment status")]
	  public SliderExperimenterStatus stepSlider;
	  public SliderExperimenterStatus trialSlider;
	  public SliderExperimenterStatus blockSlider;
	  public SliderExperimenterStatus timerSlider;
	  public TextMeshProUGUI statusText;

	  [Header("Session info")]
	  public TextMeshProUGUI experimentNameField = null;
	  public TextMeshProUGUI experimenterField = null;
	  public TextMeshProUGUI participantIDField = null;
	  public TextMeshProUGUI sessionNumberField = null;

	  public override void Awake() {
		base.Awake();

		EventManager.StartListening(eDIA.Events.Config.EvReadyToGo, OnEvReadyToGo);
		EventManager.StartListening(eDIA.Events.ControlPanel.EvEnableButton, OnEvEnableButton);
		EventManager.StartListening(eDIA.Events.ControlPanel.EvStartTimer, OnEvStartTimer);
	  }

	  void OnDestroy() {
		EventManager.StopListening(eDIA.Events.ControlPanel.EvStartTimer, OnEvStartTimer);
		EventManager.StopListening(eDIA.Events.ControlPanel.EvStopTimer, OnEvStopTimer);
		EventManager.StopListening(eDIA.Events.Config.EvReadyToGo, OnEvReadyToGo);

		btnExperiment.onClick.RemoveListener(() => EventManager.TriggerEvent(eDIA.Events.StateMachine.EvStartExperiment, null));
		btnPauseExperiment.onClick.RemoveListener(() => EventManager.TriggerEvent(eDIA.Events.StateMachine.EvPauseExperiment, null));
	  }

	  void Start() {
		HidePanel();
	  }

	  #region EVENT LISTENERS

	  void OnEvReadyToGo(eParam obj) {
		EventManager.StopListening(eDIA.Events.Config.EvReadyToGo, OnEvReadyToGo);
		EventManager.StartListening(eDIA.Events.ControlPanel.EvUpdateSessionSummary, OnEvUpdateExperimentSummary);

		panelIdle.SetActive(false);
		panelRunning.SetActive(false);
		panelStatus.SetActive(false);

		SetupButtons();
		btnExperiment.interactable = true;

		statusText.text = "ready";
		ShowPanel();

		EventManager.StartListening(eDIA.Events.StateMachine.EvStartExperiment, OnEvStartExperiment);
	  }

	  void OnEvStartExperiment(eParam e) {
		EventManager.StopListening(eDIA.Events.StateMachine.EvStartExperiment, OnEvStartExperiment);
		btnExperiment.onClick.RemoveAllListeners();

		panelIdle.SetActive(false);
		panelRunning.SetActive(true);
		panelStatus.SetActive(true);
		GetComponent<VerticalLayoutGroup>().enabled = true;

		EventManager.StartListening(eDIA.Events.ControlPanel.EvUpdateProgressStatus, OnEvExperimentProgressUpdate);
		EventManager.StartListening(eDIA.Events.ControlPanel.EvUpdateBlockProgress, OnEvUpdateBlockProgress);
		EventManager.StartListening(eDIA.Events.ControlPanel.EvUpdateTrialProgress, OnEvUpdateTrialProgress);
		EventManager.StartListening(eDIA.Events.ControlPanel.EvUpdateStepProgress, OnEvUpdateStepProgress);
		EventManager.StartListening(eDIA.Events.StateMachine.EvFinalizeSession, OnEvFinalizeSession);
	  }

	  private void OnEvUpdateExperimentSummary(eParam e) {
		string[] displayInformation = e.GetStrings();

		experimentNameField.text = displayInformation[0];
		experimenterField.text = displayInformation[1];
		participantIDField.text = displayInformation[2];
		sessionNumberField.text = displayInformation[3];

		GetComponent<VerticalLayoutGroup>().enabled = true;
	  }


	  private void OnEvFinalizeSession(eParam obj) {
		EventManager.StopListening(eDIA.Events.StateMachine.EvFinalizeSession, OnEvFinalizeSession);
		EventManager.StopListening(eDIA.Events.ControlPanel.EvUpdateSessionSummary, OnEvUpdateExperimentSummary);

		btnExperiment.transform.GetChild(0).GetComponentInChildren<Text>().text = "Quit";
		btnExperiment.onClick.AddListener(() => EventManager.TriggerEvent(eDIA.Events.Core.EvQuitApplication, null));
		btnExperiment.interactable = true;

		panelIdle.SetActive(true);
		panelRunning.SetActive(false);
		panelStatus.SetActive(false);
		GetComponent<VerticalLayoutGroup>().enabled = true;
	  }

	  void OnEvExperimentProgressUpdate(eParam e) {
		statusText.text = e is null ? "" : statusText.text = e.GetString();
	  }

	  private void OnEvUpdateBlockProgress(eParam e) {
		blockSlider.currentValue = e.GetInts()[0] < 0 ? 0 : e.GetInts()[0];
		blockSlider.maxValue = e.GetInts()[1];
	  }

	  private void OnEvUpdateTrialProgress(eParam e) {
		trialSlider.currentValue = e.GetInts()[0] < 0 ? 0 : e.GetInts()[0];
		trialSlider.maxValue = e.GetInts()[1];
	  }

	  private void OnEvUpdateStepProgress(eParam e) {
		stepSlider.currentValue = e.GetInts()[0] < 0 ? 0 : e.GetInts()[0];
		stepSlider.maxValue = e.GetInts()[1];
	  }


	  void OnEvEnableButton(eParam e) {

		bool turnOn = e.GetStrings()[1].ToUpper() == "TRUE";

		//Debug.Log("OnEvEnableButton " + e.GetStrings()[0].ToUpper() + ":" + turnOn);

		switch (e.GetStrings()[0].ToUpper()) {
		    case "PAUSE":
			  SetButtonState(btnPauseExperiment, turnOn);
			  break;
		    case "PROCEED":
			  SetButtonState(btnProceedExperiment, turnOn);
			  break;
		}
	  }

	  public void ProceedBtnCLicked() {
		EventManager.TriggerEvent(eDIA.Events.StateMachine.EvProceed);

	  }

	  void SetButtonState(Button btn, bool state) {
		btn.interactable = state;
	  }

	  void OnEvStartTimer(eParam obj) {
		EventManager.StartListening(eDIA.Events.ControlPanel.EvStopTimer, OnEvStopTimer);
		timerSlider.gameObject.SetActive(true);
		timerSlider.StartAnimation(obj.GetFloat());
	  }

	  private void OnEvStopTimer(eParam obj) {
		EventManager.StopListening(eDIA.Events.ControlPanel.EvStopTimer, OnEvStopTimer);
		timerSlider.StopAnimation();
	  }

	  void SetupButtons() {
		btnExperiment.transform.GetChild(0).GetComponentInChildren<Text>().text = "Start Experiment";
		btnExperiment.onClick.AddListener(() => EventManager.TriggerEvent(eDIA.Events.StateMachine.EvStartExperiment, null));
		btnPauseExperiment.onClick.AddListener(() => EventManager.TriggerEvent(eDIA.Events.StateMachine.EvPauseExperiment, null));
	  }

	  #endregion // -------------------------------------------------------------------------------------------------------------------------------
    }
}