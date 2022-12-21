using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RCAS;
using eDIA.Utilities;

// [System.Serializable]
// public class StringEvent 	: UnityEvent<string>{}

// [System.Serializable]
// public class BoolEvent 		: UnityEvent<bool>{}

// [System.Serializable]
// public class BoolStringEvent 	: UnityEvent<bool,string>{}

// [System.Serializable]
// public class ComObjectEvent 	: UnityEvent<object>{}

// [System.Serializable]
// public class DefaultEvent 	: UnityEvent{}

// Communication manager interface. Translates internal commands into network packages and viseversa
// ==============================================================================================================================================
namespace eDIA {

	/// <summary>
	/// Communication manager interface. Translates internal commands into network packages and viseversa
	/// </summary>
	public class ExperimentConnector : MonoBehaviour
	{


		private void Awake() {
			StartForwarder();
		}

// ==============================================================================================================================================

		// * FROM MANAGER <<

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvSetTaskConfig)]
		static void NwEvSetTaskConfig(string tConfig) {
			AddToLog("NwEvSetTaskConfig:" + tConfig);
			EventManager.TriggerEvent(eDIA.Events.Config.EvSetTaskConfig, new eParam(tConfig));
		}

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvSetExpConfig)]
		static void NwEvSetExpConfig(string eConfig) {
			AddToLog("NwEvSetExpConfig" + eConfig);
			EventManager.TriggerEvent(eDIA.Events.Config.EvSetExperimentConfig, new eParam(eConfig));
		}

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvStartExperiment)]
		static void NwEvStartExperiment() {
			AddToLog("NwEvStartExperiment");
			EventManager.TriggerEvent(eDIA.Events.StateMachine.EvStartExperiment, null);
		}

		[RCAS_RemoteEvent(eDIA.Events.Network.NwEvProceed)]
		static void NwEvProceed() {
			AddToLog("NwEvProceed");
			EventManager.TriggerEvent(eDIA.Events.StateMachine.EvProceed, null);
		}





		[RCAS_RemoteEvent("poke")]
		static void Poke() {
			Debug.Log("You got poked!");
		}

		[RCAS_RemoteEvent("SetConfig")]
		static void SetConfig (string message) {
			Debug.Log("string length: " + message.Length);
			Debug.Log("Someone whispers us a message: "+message);

		// Controller.SetConfig(message);
		}

		[RCAS_RemoteEvent("set_params")]
		static void SetParams(string [] args) {
		Debug.Log($"Parameters received: {args[0]}, {args[1]}, {args[2]}");
		}


		private static void AddToLog(string _msg) {
				eDIA.LogUtilities.AddToLog(_msg, "EXP", Color.cyan);
		}








// ==============================================================================================================================================

		// * TO MANAGER >>

		private void StartForwarder () {

			// Configs
			EventManager.StartListening(eDIA.Events.Config.EvTaskConfigSet, 			NwEvTaskConfigSet);
			EventManager.StartListening(eDIA.Events.Config.EvExperimentConfigSet, 		NwEvExperimentConfigSet);
			EventManager.StartListening(eDIA.Events.Config.EvReadyToGo,				NwEvReadyToGo);

			// Control panel
			EventManager.StartListening(eDIA.Events.ControlPanel.EvEnableButton,		NwEvEnableButton);
			EventManager.StartListening(eDIA.Events.ControlPanel.EvUpdateStepProgress,	NwEvUpdateStepProgress);
			EventManager.StartListening(eDIA.Events.ControlPanel.EvUpdateTrialProgress,	NwEvUpdateTrialProgress);
			EventManager.StartListening(eDIA.Events.ControlPanel.EvUpdateBlockProgress,	NwEvUpdateBlockProgress);
			EventManager.StartListening(eDIA.Events.ControlPanel.EvUpdateSessionSummary,	NwEvUpdateSessionSummary);
			EventManager.StartListening(eDIA.Events.ControlPanel.EvUpdateProgressInfo,	NwEvUpdateProgressInfo);
			EventManager.StartListening(eDIA.Events.ControlPanel.EvStartTimer,		NwEvStartTimer);
			EventManager.StartListening(eDIA.Events.ControlPanel.EvStopTimer,			NwEvStopTimer);

			// Eye
			EventManager.StartListening(eDIA.Events.Eye.EvEnableEyeCalibrationTrigger,	NwEvEnableEyeCalibrationTrigger);

		}


		// Configs

		private void NwEvTaskConfigSet(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvTaskConfigSet);
		}

		private void NwEvExperimentConfigSet(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvExperimentConfigSet);
		}

		private void NwEvReadyToGo(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvReadyToGo);
		}


		// Control panel

		private void NwEvEnableButton(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvEnableButton, obj.GetStrings());
		}

		private void NwEvUpdateStepProgress(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvUpdateStepProgress, ArrayTools.ConvertIntsToStrings(obj.GetInts()));
		}

		private void NwEvUpdateTrialProgress(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvUpdateTrialProgress, ArrayTools.ConvertIntsToStrings(obj.GetInts()));
		}

		private void NwEvUpdateBlockProgress(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvUpdateBlockProgress, ArrayTools.ConvertIntsToStrings(obj.GetInts()));
		}

		private void NwEvUpdateSessionSummary(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvUpdateSessionSummary, obj.GetStrings());
		}

		private void NwEvUpdateProgressInfo(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvUpdateProgressInfo, obj.GetString());
		}

		private void NwEvStartTimer(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvStartTimer, obj.GetFloat().ToString());
		}

		private void NwEvStopTimer(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvStopTimer);
		}


		// Eye

		private void NwEvEnableEyeCalibrationTrigger(eParam obj)
		{
			RCAS_Peer.Instance.TriggerRemoteEvent(eDIA.Events.Network.NwEvEnableEyeCalibrationTrigger, obj.GetBool().ToString());
		}






		/*




		// public TextMeshProUGUI connectionStatus;



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




		#######################################################################################################################
		####
		#### 		Commented out as we don't' have decided on the proper networking asset to use for communications
		####
		#######################################################################################################################

		[SerializeField] 
		[Header("Fired when connection is made or lost to the manager")]
		public BoolStringEvent onConnected;

		[SerializeField] 
		[Header("Fired when session data is received from the manager")]
		public StringEvent onBeginSession;

		[SerializeField] 
		[Header("Fired when session START is received from the manager")]
		public DefaultEvent onStartSession;

		[SerializeField] 
		[Header("Fired when session END is received from the manager")]
		public DefaultEvent onEndSession;
		
		[SerializeField] 
		[Header("Fired when session PAUSE is received from the manager")]
		public BoolEvent onPauseSession;

		[SerializeField]
		[Header("test to send a unknown object type")]
		public ComObjectEvent onObjectTest;

		private Coroutine networkCheckerRoutine = null;

		public GameViewEncoder gameViewEncoder;

		[Header("Settings")]
		public bool isConnected = false;
		public bool isDebug = false;
		private bool isRemote = false; // main isRemote setting is on the experiment side.

		// ==============================================================================================================================================

		#region Starters 

		//# Singleton {
		public static NetworkComManager instance = null;

		//# SYSTEM STEP 01 !
		private void Awake()
		{
			// Make a singleton for this so it's reachable 
			if (instance == null) instance = this;
			else if (instance != this) Destroy(this);
		}

		public void StartAsClient () 
		{
			isRemote = true;
			StartCoroutine(Init());
		}

		IEnumerator Init()
		{
			// Wait buffer
			yield return new WaitForSeconds(1);

			// Start client
			FMNetworkManager.instance.Action_InitAsClient();

			// Wait till networkdiscovery has found the server connection
			while (!FMNetworkManager.instance.Client.FoundServer)
				yield return new WaitForSeconds(0.5f);

			// Wait till networkdiscovery has found the server connection
			while (!FMNetworkManager.instance.Client.IsConnected)
				yield return new WaitForSeconds(0.5f);

			// networkCheckerRoutine = StartCoroutine(NetworkStatusChecker());
		}

		void OnDestroy()
		{
			// EventManager.StopListening("EvUpdateSeqSnapShot", 	OnEvUpdateSeqSnapShot);
			EventManager.StopListening("EvUpdateSeqStatus", 	OnEvUpdateSeqStatus);
		}

		#endregion

		// ==============================================================================================================================================

		
		public void ConnectionMade()
		{
			SendHello();
		}

		// -----------------------------------------------------------------------------------------------------------------------------------------------

		#region <<< Network Recieving

		public void RecieveMsg(string _msg)
		{
			if (isDebug)
				Debug.Log("CLIENTRAW:" + _msg);

			string[] keywords = _msg.Split('|');

			switch (keywords[0])
			{
				case "INFO":
					break;

				case "COMMAND":
					switch (keywords[1])
					{
						case "SETTABLEHEIGHT":
							EventManager.TriggerEvent("EvSetTableheight", null);

							AddToConsole("Set Tableheight");
							
							break;

						case "SESSION":
							switch (keywords[2])
							{
								case "BEGIN":
									AddToConsole("Set Session Data");
									onBeginSession.Invoke(keywords[3]);
									Debug.Log("BEGIN " + keywords[3]);
								break;
								case "START":
									AddToConsole("Session Start");
									// EventManager.StartListening("EvUpdateSeqSnapShot", 	OnEvUpdateSeqSnapShot);
									EventManager.StartListening("EvUpdateSeqStatus", 	OnEvUpdateSeqStatus);
									onStartSession.Invoke();
									SendResultMsg("OK", "SESSIONSTART");
									break;
								case "STOP":
									AddToConsole("Task stopped");
									EventManager.TriggerEvent("EvRequestStopTask", null);
									
									break;
								case "END":
									AddToConsole("Session stopped from manager");
									// EventManager.TriggerEvent("EvRequestStopTask", null);
									onEndSession.Invoke();
									
									break;
								case "NEXT":
									// IncreasePhase();
									AddToConsole("Task Next Trial");
									EventManager.TriggerEvent("EvNextTrial", null);
									break;
								case "PAUSE":
									AddToConsole("Task Pause");
									onPauseSession.Invoke(true);
									EventManager.TriggerEvent("EvSessionPause", new eParam(true));
									break;
								case "UNPAUSE":
									AddToConsole("Task UnPause");
									onPauseSession.Invoke(false);
									EventManager.TriggerEvent("EvSessionPause", new eParam(false));
									break;
								case "PROCEED":
									AddToConsole("Task proceed");
									EventManager.TriggerEvent("EvUserClicked", null);
									break;
								case "OVERRULE":
									AddToConsole("Task proceed overrule");
									// LogManager.AddToSessionLog(LogManager.LogSessionAction.EXECUTED, "UserProceed overrule by experimenter");
									EventManager.TriggerEvent("EvUserClicked", null);
									break;
								case "NXTBLK":
									AddToConsole("Task nxt block");
									EventManager.TriggerEvent("EvDebugNextBlock", null);
									break;
								case "NXTCLTR":
									AddToConsole("Task clstr block");
									EventManager.TriggerEvent("EvDebugNextCluster", null);
									break;
							}
							break;

						// case "CAM":
						// 	switch (keywords[2])
						// 	{
						// 		case "USER":
						// 			// ChangeCamView(camUser);
						// 			break;
						// 		case "SPECTATOR":
						// 			// ChangeCamView(camSpec);
						// 			break;
						// 	}
						// 	break;

						case "RECONNECT":
						{
							Invoke("ReconnectToManager",3);
						}
						break;
					}
					break;
			}
		}

		#endregion

		// -----------------------------------------------------------------------------------------------------------------------------------------------

		#region >>> Network Sending

		void SendMsg(string _msg)
		{
			if (isRemote)
			{
				if (FMNetworkManager.instance.Client.IsConnected)
				{
					FMNetworkManager.instance.SendToServer(_msg);
					// AddToConsole("SEND >" + _msg);
				} else AddToConsole("UNABLE TO SEND > NOT CONNECTED");
			}
		}

		/// <summary>
		/// Send reply to the server on last recieved command
		/// </summary>
		/// <param name="_resultstatus">OK / ERROR</param>
		/// <param name="_msg">Additional info</param>
		public void SendResultMsg(string _resultstatus, string _msg)
		{
			SendMsg("INFO|RESULT|" + _resultstatus + "|" + _msg);
		}

		void SendHello()
		{
			Debug.Log("Send helloooooo");
			string networkMsg = string.Empty;
			networkMsg = "INFO|";
			networkMsg += "CLIENT|";
			networkMsg += "HELLO|";

			// Get serial number of the Quest used
			string deviceID = "N.A.";

	#if (UNITY_EDITOR)
		deviceID = "UnityEditor";
	#endif
	#if (UNITY_STANDALONE_WIN)
		deviceID = "WindowsBuild";
	#endif

	#if (UNITY_ANDROID)
		#if (!UNITY_EDITOR)
				AndroidJavaObject jo = new AndroidJavaObject("android.os.Build");
				deviceID = jo.GetStatic<string>("SERIAL");
		#endif
	#endif

			// // Get software version of MimVMT
			// networkMsg += deviceID + ", v" + SystemSettings.mimVersion; // GetDeviceName
			networkMsg += deviceID + ", v1"; // GetDeviceName

			// SystemLogManager.AddToLog("SEND " + networkMsg);
			SendMsg(networkMsg);
		}

		// public void SendBatteryUpdate()
		// {
		// 	string networkMsg = string.Empty;
		// 	networkMsg = "INFO|";
		// 	networkMsg += "HW|";
		// 	networkMsg += "PWR|";
		// 	networkMsg += (OVRManager.batteryLevel * 100) + "," + OVRInput.GetControllerBatteryPercentRemaining(OVRInput.Controller.LTouch) + "," + OVRInput.GetControllerBatteryPercentRemaining(OVRInput.Controller.RTouch);

		// 	SendMsg(networkMsg);
		// }

		// IEnumerator SendBatteryUpdateTimer()
		// {
		// 	while (true)
		// 	{
		// 		yield return new WaitForSeconds(SystemSettings.timeOfIntervalPwrUpdate);
		// 		//     AddToConsole(DateTime.Now + " Status timer done, sending update");
		// 		SendBatteryUpdate();
		// 	}
		// }

		public void ReconnectToManager() 
		{
			SendHello();
		}

		// public void OnEvUpdateSeqSnapShot (eParam e)
		// {
		// 	string networkMsg = "INFO|SES|SEQ|" + e.GetString();
		// 	SendMsg(networkMsg);
		// }

		/// <summary>Sends the current Block and Trial number to the manager</summary>
		/// <param name="eParam">int[] <blocknumber><trialnumber></param>
		public void SendSessionProgressUpdate (eParam e) {
			string networkMsg = "INFO|SES|SEQUENCEUPDATE|" + e.GetIntAt(0) + "," + e.GetIntAt(1) + "," + e.GetIntAt(2) + "," + e.GetIntAt(3);
			SendMsg(networkMsg);
		}

		/// <summary>Sends the string describing the STATUS to the manager</summary>
		void OnEvUpdateSeqStatus (eParam e) {
			string networkMsg = "INFO|SES|STATUS|" + e.GetString();
			SendMsg(networkMsg);
		}

		public void SendAwaitsTimer(float _duration)
		{
			// Show that we are waiting on a timer and how long
			SendMsg("INFO|SES|AWAITS|TIMER|" + _duration.ToString());
		}

		public void SendAwaitsSignalRemote()
		{
			// Client sequence requires a PROCEED call from server
			SendMsg("INFO|SES|AWAITS|EXPERIMENTER");
		}

		public void SendAwaitsSignalLocal()
		{
			// Client sequence requires a PROCEED call from server
			SendMsg("INFO|SES|AWAITS|USER");
		}

		public void SendCurrentOnscreenInfo(string _text)
		{
			string networkMsg = "INFO|SES|ONSCREENINFO|";
			networkMsg += _text;

			SendMsg(networkMsg);
		}

		public void SendEndSession () 
		{
			SendMsg("INFO|SES|END");
		}

		public void SendSessionMDE (string _MDEvalue) 
		{
			Debug.Log("MDE:" + _MDEvalue);
			SendMsg("INFO|SES|MDE|" + _MDEvalue);
		}

		public void SendHMDmounted (bool _onOff)
		{
			SendMsg("INFO|HW|MOUNTED|" + _onOff);
		}

		#endregion

		#endregion

		// ==============================================================================================================================================


		void AddToConsole(string _msg)
		{
			// SystemLogManager.AddToLog(_msg);
			if (isDebug)
				Debug.Log(_msg);
			EventManager.TriggerEvent("EvAddToDebugConsole", new eParam (_msg));
		}

		// TODO Should not be in a networkcom manager
		// public void SendScreenShot ()
		// {
		// 	camUser.enabled = true;
		// 	gameViewEncoder.Action_UpdateTexture();
		// 	Debug.Log("SendScreenShot");
		// 	camUser.enabled = false;
		// }

		#region LogFiles

	/// <summary>
	/// Sends the data to the server to be saved to disk
	/// </summary>
	/// <param name="List of strings"></param>
		public void SendSessionInfoFileEntries (List<string> _entries) 
		{
			if (!isConnected)
				return;

			string networkMsg = "LOG|INFO|";

			foreach(string s in _entries) 
			{
				networkMsg += s + "+";
			}

			SendMsg(networkMsg);
		}

		string ConvertToMultiString (List<string> _entries) 
		{
			string t = "";
			foreach (string s in _entries) { t+= s + "+"; }

			return t.Substring(0, t.Length-1); // remove the last + char
		}

		public void SendSessionLogRecords(List<string> _entries)
		{
			string networkMsg = "LOG|SESSION|";
			networkMsg += ConvertToMultiString(_entries);
			SendMsg(networkMsg);
		}

		public void SendUserLogRecords(List<string> _entries)
		{
			string networkMsg = "LOG|USER|";
			networkMsg += ConvertToMultiString(_entries);
			SendMsg(networkMsg);
		}

		public void SendObjectLogRecords(List<string> _entries)
		{
			string networkMsg = "LOG|OBJECT|";
			networkMsg += ConvertToMultiString(_entries);
			SendMsg(networkMsg);
		}

		#endregion

		*/
	}
}