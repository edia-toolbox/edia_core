using System;
using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RCAS;

namespace eDIA.Manager {

	/// <summary> In project version of the connector to a remote interface </summary>
	public class ControlConnector : MonoBehaviour {


#region > IN


		private void Awake()
		{
			// * TO APP >>
			
			// State machine
			EventManager.StartListening(eDIA.Events.StateMachine.EvStartExperiment, 	NwEvStartExperiment);
			EventManager.StartListening(eDIA.Events.StateMachine.EvPauseExperiment, 	NwEvPauseExperiment);
			EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, 			NwEvProceed);

			// Features
			EventManager.StartListening(eDIA.Events.Casting.EvEnableCasting, 			NwEvEnableCasting);

			// Configs
			EventManager.StartListening(eDIA.Events.Config.EvSetTaskConfig, 			NwEvSetTaskConfig);
			EventManager.StartListening(eDIA.Events.Config.EvSetExperimentConfig, 		NwEvSetExperimentConfig);
		}



		// * TO APP >>


		private void NwEvStartExperiment(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvStartExperiment);
		}

		private void NwEvPauseExperiment(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvPauseExperiment);
		}

		private void NwEvProceed(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvProceed);
		}

		private void NwEvEnableCasting(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvEnableCasting, obj.GetBool().ToString());
		}

		private void NwEvSetExperimentConfig(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvSetExpConfig, obj.GetString());
		}

		private void NwEvSetTaskConfig(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvSetTaskConfig, obj.GetString());
		}




		/*
			All things going into the framework to control the experiment
		*/




		// * >> TO MANAGER 


	
		[RCAS_RemoteEvent("NwEvTaskConfigSet")]
		static void NwEvTaskConfigSet() {
			EventManager.TriggerEvent(eDIA.Events.Config.EvTaskConfigSet);
		}

		[RCAS_RemoteEvent("NwEvExperimentConfigSet")]
		static void NwEvExperimentConfigSet() {
			EventManager.TriggerEvent(eDIA.Events.Config.EvExperimentConfigSet);
		}

		[RCAS_RemoteEvent("NwEvEnableButton")]
		static void NwEvEnableButton(string[] args) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(args));
		}

		[RCAS_RemoteEvent("NwEvReadyToGo")]
		static void NwEvReadyToGo(string[] args) {
			EventManager.TriggerEvent(eDIA.Events.Config.EvReadyToGo);
		}

		[RCAS_RemoteEvent("NwEvUpdateTrialProgress")]
		static void NwEvUpdateTrialProgress( string[] args ) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateTrialProgress, new eParam(args));
		}

		[RCAS_RemoteEvent("NwEvUpdateBlockProgress")]
		static void NwEvUpdateBlockProgress( string[] args ) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateBlockProgress, new eParam(args));
		}

		[RCAS_RemoteEvent("NwEvUpdateSessionSummary")]
		static void NwEvUpdateSessionSummary( string[] args ) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateSessionSummary, new eParam(args));
		}

		[RCAS_RemoteEvent("NwEvUpdateProgressInfo")]
		static void NwEvUpdateProgressInfo( string[] args ) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateProgressInfo);
		}

		[RCAS_RemoteEvent("NwEvEnableEyeCalibrationTrigger")]
		static void NwEvEnableEyeCalibrationTrigger( string[] args ) {
			EventManager.TriggerEvent(eDIA.Events.Eye.EvEnableEyeCalibrationTrigger, new eParam(args));
		}

		[RCAS_RemoteEvent("NwEvUpdateStepProgress")]
		static void NwEvUpdateStepProgress( string[] args ) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateStepProgress, new eParam(args));
		}

		[RCAS_RemoteEvent("NwEvStartTimer")]
		static void NwEvStartTimer( string[] args ) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvStartTimer, new eParam(args));
		}

		[RCAS_RemoteEvent("NwEvStopTimer")]
		static void NwEvStopTimer( string[] args ) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvStopTimer, new eParam(args));
		}


		/*

		start experiment
		proceed
		inject pause

		supply session config
		supply task config

		stream control: on / off / change cam

		custom controls [ event ( param ) ]  -> i.e. settableheight  (probably always need to be an event)

    public void TriggerEvent(string eventName)
    {
        RCAS_Peer.Instance.TriggerRemoteEvent(eventName);
    }

    public void TriggerEvent(string eventName, string arg)
    {
        RCAS_Peer.Instance.TriggerRemoteEvent(eventName, arg);
    }

    public void TriggerEvent(string eventName, string[] args)
    {
        RCAS_Peer.Instance.TriggerRemoteEvent(eventName, args);
    }

    public void TriggerEvent_Color_To_Custom(TMPro.TMP_InputField color_input)
    {
        RCAS_Peer.Instance.TriggerRemoteEvent("change_color_to_custom", color_input.text);
    }

		*/

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region < OUT

		/*
		// 	All things coming back from the framework towards the manager
		// */
		// public void ListenToSystem () {

		// 	EventManager.StartListening (eDIA.Events.ControlPanel.EvUpdateExperimentSummary, OnEvUpdateExperimentSummary);
		// 	EventManager.StartListening (eDIA.Events.ControlPanel.EvUpdateProgressDescription, OnEvExperimentProgressUpdate);
		// 	EventManager.StartListening (eDIA.Events.ControlPanel.EvStartTimer, OnEvStartTimer);
		// 	EventManager.StartListening (eDIA.Events.ControlPanel.EvStopTimer, OnEvStopTimer);
		// 	EventManager.StartListening (eDIA.Events.ControlPanel.EvEnableButton, OnEvEnableButton);

		// }

		// private void OnEvEnableButton (eParam obj) {
		// 	outField.text = "OnEvEnableButton: " + obj.GetString() + "\n"  + outField.text;
		// }

		// private void OnEvStartTimer (eParam obj) {
		// 	outField.text = "OnEvStartTimer\n" + outField.text;
		// }

		// private void OnEvStopTimer (eParam obj) {
		// 	outField.text = "OnEvStopTimer\n" + outField.text;
		// }

		// private void OnEvExperimentProgressUpdate (eParam obj) {
		// 	outField.text = "OnEvExperimentProgressUpdate: " + obj.GetString() + "\n"  + outField.text;
		// }

		// private void OnEvUpdateExperimentSummary (eParam obj) {
		// 	outField.text = "OnEvUpdateExperimentSummary: " + obj.GetStrings()[0] + "\n" + outField.text;
		// }

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