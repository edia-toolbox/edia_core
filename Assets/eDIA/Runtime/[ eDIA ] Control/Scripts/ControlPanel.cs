using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace eDIA.Manager
{

	public class ControlPanel : Singleton<ControlPanel>
	{
		public Transform NonActivePanelHolder = null;
		public Transform MenuPanelHolder = null;
		public Transform consolePanel = null;
		public bool showEventLog = true;
		public bool ShowConsole = false;

		[Space(20)]
		public ControlSettings Settings;

		// Local
		private PanelMessageBox _pMessageBox = null;
		private PanelConfigSelection _pConfigSelection = null;
		private PanelHeader _pHeader = null;
		private PanelApplicationSettings _pApplicationSettings = null;
		private PanelExperimentControl _pExperimentControl = null;

		// Remote
		private PanelConfigMaker _pConfigMaker = null;

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

			_pConfigSelection.Init();

			consolePanel.gameObject.SetActive(ShowConsole);

			Debug.Log("Init done");
		}

		void GetPanelReferences()
		{
			// General
			_pMessageBox 		= GetComponentInChildren<PanelMessageBox>();
			_pConfigSelection 	= GetComponentInChildren<PanelConfigSelection>();
			_pHeader 			= GetComponentInChildren<PanelHeader>();
			_pApplicationSettings 	= GetComponentInChildren<PanelApplicationSettings>();
			_pExperimentControl 	= GetComponentInChildren<PanelExperimentControl>();

			// if (Settings.ControlMode is ControlMode.Remote)
				//TODO Create eventsystem in the scene, as we need to click on the buttons

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

	}
}