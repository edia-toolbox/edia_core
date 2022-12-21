using System;
using System.Collections;
using System.Collections.Generic;
using eDIA;
using eDIA.Utilities;
using RCAS;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace eDIA.Manager
{

	/// <summary> In project version of the connector to a remote interface </summary>
	public class ControlConnector : MonoBehaviour
	{


		#region TO APP >>

		private void Awake()
		{
			// * TO APP >>

			// State machine
			EventManager.StartListening(eDIA.Events.StateMachine.EvStartExperiment, NwEvStartExperiment);
			EventManager.StartListening(eDIA.Events.StateMachine.EvPauseExperiment, NwEvPauseExperiment);
			EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, 		NwEvProceed);

			// Features
			EventManager.StartListening(eDIA.Events.Casting.EvEnableCasting, 		NwEvEnableCasting);

			// Configs
			EventManager.StartListening(eDIA.Events.Config.EvSetTaskConfig, 		NwEvSetTaskConfig);
			EventManager.StartListening(eDIA.Events.Config.EvSetExperimentConfig, 	NwEvSetExperimentConfig);
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


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region FROM APP <<

		// * FROM APP <<

		// Configs

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvTaskConfigSet)]
		static void NwEvTaskConfigSet()
		{
			EventManager.TriggerEvent(eDIA.Events.Config.EvTaskConfigSet);
		}

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvExperimentConfigSet)]
		static void NwEvExperimentConfigSet()
		{
			EventManager.TriggerEvent(eDIA.Events.Config.EvExperimentConfigSet);
		}

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvReadyToGo)]
		static void NwEvReadyToGo()
		{
			EventManager.TriggerEvent(eDIA.Events.Config.EvReadyToGo);
		}


		// ControlPanel

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvEnableButton)]
		static void NwEvEnableButton(string[] args)
		{
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(args));
		}

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvUpdateStepProgress)]
		static void NwEvUpdateStepProgress(string[] args)
		{
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateStepProgress, new eParam(ArrayTools.ConvertStringsIntoInts(args)));
		}

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvUpdateTrialProgress)]
		static void NwEvUpdateTrialProgress(string[] args)
		{
			Debug.Log(args[0] + " " + args[1]);
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateTrialProgress, new eParam(ArrayTools.ConvertStringsIntoInts(args)));
		}

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvUpdateBlockProgress)]
		static void NwEvUpdateBlockProgress(string[] args)
		{
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateBlockProgress, new eParam(ArrayTools.ConvertStringsIntoInts(args)));
		}

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvUpdateSessionSummary)]
		static void NwEvUpdateSessionSummary(string[] args)
		{
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateSessionSummary, new eParam(args));
		}

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvUpdateProgressInfo)]
		static void NwEvUpdateProgressInfo(string arg)
		{
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvUpdateProgressInfo, new eParam(arg));
		}

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvEnableEyeCalibrationTrigger)]
		static void NwEvEnableEyeCalibrationTrigger(string arg)
		{
			EventManager.TriggerEvent(eDIA.Events.Eye.EvEnableEyeCalibrationTrigger, new eParam(bool.Parse(arg)));
		}

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvStartTimer)]
		static void NwEvStartTimer(string arg)
		{
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvStartTimer, new eParam(float.Parse(arg)));
		}

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvStopTimer)]
		static void NwEvStopTimer()
		{
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvStopTimer);
		}



		#endregion // -------------------------------------------------------------------------------------------------------------------------------

	}
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



/*

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
