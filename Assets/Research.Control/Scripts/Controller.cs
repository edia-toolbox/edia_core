using System;
using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UnityEngine.Events;

using UnityEngine.UI;
using TMPro;


namespace eDIA.Manager {

	/// <summary> In project version of the connector to a remote interface </summary>
	public class Controller : MonoBehaviour {

		//* IN



		//* OUT
		public TextMeshProUGUI outField = null;

		public UnityEvent<string> OnTextOutput = null;

		private void Start() {
			ListenToSystem();
		}

#region > IN

		/*
			All things going into the framework to control the experiment
		*/

		//* Control

		public void StartExperiment () {
			EventManager.TriggerEvent (eDIA.Events.Core.EvStartExperiment, null);
		}

		public void PauseExperiment () {
			EventManager.TriggerEvent (eDIA.Events.Core.EvPauseExperiment, null);
		}

		public void ProceedExperiment () {
			EventManager.TriggerEvent (eDIA.Events.Core.EvProceed, null);
		}

		//* Configs

		public void SetExperimentConfig (string configAsJson) {
			Experiment.Instance.SetExperimentConfig (configAsJson);
		}

		public void SetTaskConfig (string configAsJson) {
			Experiment.Instance.SetTaskConfig (configAsJson);
		}

		//* Stream

		public void EnableCasting (bool onOff) {
			EventManager.TriggerEvent (eDIA.Events.Casting.EvEnableCasting, new eParam (onOff));
		}

		//* Custom

		public void SendCustomEvent (string eventName, object eparam) {
			EventManager.TriggerEvent (eventName, new eParam (eparam));
		}

		/*

		start experiment
		proceed
		inject pause

		supply session config
		supply task config

		stream control: on / off / change cam

		custom controls [ event ( param ) ]  -> i.e. settableheight  (probably always need to be an event)

		*/

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region < OUT

		/*
			All things coming back from the framework towards the manager
		*/
		public void ListenToSystem () {

			EventManager.StartListening (eDIA.Events.ControlPanel.EvUpdateExperimentSummary, OnEvUpdateExperimentSummary);
			EventManager.StartListening (eDIA.Events.ControlPanel.EvExperimentProgressUpdate, OnEvExperimentProgressUpdate);
			EventManager.StartListening (eDIA.Events.ControlPanel.EvStartTimer, OnEvStartTimer);
			EventManager.StartListening (eDIA.Events.ControlPanel.EvStopTimer, OnEvStopTimer);
			EventManager.StartListening (eDIA.Events.ControlPanel.EvEnableButton, OnEvEnableButton);

		}

		private void OnEvEnableButton (eParam obj) {
			outField.text = "OnEvEnableButton: " + obj.GetString() + "\n"  + outField.text;
		}

		private void OnEvStartTimer (eParam obj) {
			outField.text = "OnEvStartTimer\n" + outField.text;
		}
		
		private void OnEvStopTimer (eParam obj) {
			outField.text = "OnEvStopTimer\n" + outField.text;
		}

		private void OnEvExperimentProgressUpdate (eParam obj) {
			outField.text = "OnEvExperimentProgressUpdate: " + obj.GetString() + "\n"  + outField.text;
		}

		private void OnEvUpdateExperimentSummary (eParam obj) {
			outField.text = "OnEvUpdateExperimentSummary: " + obj.GetStrings()[0] + "\n" + outField.text;
		}

		/*

		progress update
		session summary
		OK

		starts timer
		stop timer

		logdata (trackers, other, session, etc)
		trial results


		custom controls [ event ( param ) ] 

		video stream


		*/

#endregion // -------------------------------------------------------------------------------------------------------------------------------

	}

}