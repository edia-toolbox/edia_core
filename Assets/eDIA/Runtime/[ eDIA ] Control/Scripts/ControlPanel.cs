using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RCAS;

namespace eDIA.Manager
{

	public class ControlPanel : Singleton<ControlPanel>
	{
		// [HideInInspector]
		public Transform NonActivePanelHolder = null;
		// [HideInInspector]
		public Transform MenuPanelHolder = null;
		public bool showEventLog = true;

		[Space(20)]
		public ControlSettings Settings;

		// Local
		private PanelMessageBox _pMessageBox = null;
		private PanelConfigSelection _pConfigSelection = null;
		private PanelHeader _pHeader = null;
		private PanelApplicationSettings _pApplicationSettings = null;
		private PanelExperimentControl _pExperimentControl = null;
		private PanelConsole _pConsole = null;

		// Remote
		private PanelConfigMaker _pConfigMaker = null;

		// private GameObject _eventSystem = null;
		private List<Transform> _currentPanelOrder = new List<Transform>();



		private void Awake()
		{

			GetPanelReferences();

			Init();
		}

		private void Init()
		{
			EventManager.showLog = showEventLog;

			// Panel control
			foreach (Transform tr in NonActivePanelHolder)
			{
				tr.name = tr.GetSiblingIndex().ToString() + "_" + tr.name;
			}

			// General
			if (Settings.LookForLocalConfigs)
				_pConfigSelection.Init();

			_pConsole.ShowConsole(Settings.ShowConsole);

			// Remote
			// _eventSystem.SetActive(Settings.ControlMode is ControlMode.Remote);

			Add2Console("Init done");
		}

		private void Start()
		{
			if (Settings.ControlMode is ControlMode.Remote)
				RCAS_Peer.Instance.OnConnectionEstablished += Connected;

		}

		private void OnDestroy()
		{
		}


		void Connected(System.Net.EndPoint EP)
		{
			RCAS_Peer.Instance.OnConnectionEstablished -= Connected;
			_pConfigMaker.Init();
		}


		void GetPanelReferences()
		{
			// General
			_pMessageBox 		= GetComponentInChildren<PanelMessageBox>();
			_pConfigSelection 	= GetComponentInChildren<PanelConfigSelection>();
			_pHeader 			= GetComponentInChildren<PanelHeader>();
			_pApplicationSettings 	= GetComponentInChildren<PanelApplicationSettings>();
			_pExperimentControl 	= GetComponentInChildren<PanelExperimentControl>();
			_pConsole 			= GetComponentInChildren<PanelConsole>();

			if (Settings.ControlMode is ControlMode.Remote)
				//TODO Create eventsystem in the scene, as we need to click on the buttons

			// Remote
			_pConfigMaker = GetComponentInChildren<PanelConfigMaker>();

		}

		public void ShowPanel(Transform panel, bool onOff)
		{	
			panel.SetParent(onOff ? MenuPanelHolder : NonActivePanelHolder, true);
			UpdatePanelOrder();
		}


		public void UpdatePanelOrder()
		{

			_currentPanelOrder.Clear();
			_currentPanelOrder = MenuPanelHolder.Cast<Transform>().ToList();
			_currentPanelOrder.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });

			for (int i = 0; i < _currentPanelOrder.Count; ++i)
			{
				_currentPanelOrder[i].SetSiblingIndex(i);
			}
		}


		public void ShowMessage(string msg, bool autoHide)
		{
			_pMessageBox.ShowMessage(msg, autoHide);
		}


		public void Add2Console(string msg)
		{
			_pConsole.Add2Console(msg);
		}


		public void Add2ConsoleIn(string msg)
		{
			Add2Console ("< " + msg);
		}


		public void Add2ConsoleOut(string msg)
		{
			Add2Console ("> " + msg);
		}



	}
}