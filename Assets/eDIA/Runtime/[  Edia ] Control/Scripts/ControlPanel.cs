using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Edia.Controller
{
	public class ControlPanel : Singleton<ControlPanel>
	{
		public Transform NonActivePanelHolder = null;
		public Transform PanelHolder = null;
		public Transform RemotePanel = null;
		public Transform RemotePanelHolder = null;
		public Transform ConsolePanel = null;
		public bool ShowEventLog = true;
		public bool ShowConsole = false;

		[Space(20)]
		public ControlSettings Settings;

		// Local
		PanelMessageBox _pMessageBox = null;
		PanelConfigSelection _pConfigSelection = null;
		PanelHeader _pHeader = null;
		PanelApplicationSettings _pApplicationSettings = null;
		PanelExperimentControl _pExperimentControl = null;

		// Remote
		PanelConfigMaker _pConfigMaker = null;
		List<Transform> _currentPanelOrder = new List<Transform>();

		void Awake()
		{
			DontDestroyOnLoad(this);

			GetPanelReferences();

			Init();
		}

		void OnDestroy()
		{
			EventManager.StopListening (Edia.Events.ControlPanel.EvConnectionEstablished, OnEvConnectionEstablished);
		}

		void Init()
		{
			EventManager.showLog = ShowEventLog; // Eventmanager to show debug in console

			// Move all panels from task first to non visuable holder
			foreach (Transform t in PanelHolder) { 
				t.SetParent(NonActivePanelHolder, true);
			}

			// Panels renaming
			foreach (Transform tr in NonActivePanelHolder)
			{
				tr.name = tr.GetSiblingIndex().ToString() + "_" + tr.name;
			}

			ConsolePanel.gameObject.SetActive(ShowConsole);
			//RemotePanel.gameObject.SetActive(Settings.ControlMode is ControlMode.Remote);

			if (Settings.ControlMode is ControlMode.Remote)
			{
				EventManager.StartListening(Edia.Events.ControlPanel.EvConnectionEstablished, OnEvConnectionEstablished);
			} else 
				InitConfigFileSearch();
		}



		void OnEvConnectionEstablished(eParam obj)
		{
			EventManager.StopListening(Edia.Events.ControlPanel.EvConnectionEstablished, OnEvConnectionEstablished);
			InitConfigFileSearch();
		}

		void InitConfigFileSearch()
		{
			_pConfigSelection.Init();
		}

		void GetPanelReferences()
		{
			// General
			_pMessageBox 			= GetComponentInChildren<PanelMessageBox>();
			_pConfigSelection 		= GetComponentInChildren<PanelConfigSelection>();
			_pExperimentControl 	= GetComponentInChildren<PanelExperimentControl>();
			_pHeader 				= GetComponentInChildren<PanelHeader>();
			_pApplicationSettings 	= GetComponentInChildren<PanelApplicationSettings>();
		}

		public void ShowPanel(Transform panel, bool onOff)
		{	
			panel.SetParent(onOff ? PanelHolder : NonActivePanelHolder, true);  
			UpdatePanelOrder();

			// WIP approach for giving a panel a different panel (needed when control panel has multiple columns):
			//panel.SetParent(onOff ? panel.GetComponent<ExperimenterPanel>().myParent : NonActivePanelHolder, true); // => disabled as awake is too late to store value of parent
		}


		public void UpdatePanelOrder()
		{
			_currentPanelOrder.Clear();
			_currentPanelOrder = PanelHolder.Cast<Transform>().ToList();
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

	}
}