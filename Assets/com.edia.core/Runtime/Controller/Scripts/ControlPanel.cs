using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace Edia.Controller
{
	public class ControlPanel : Singleton<ControlPanel>
	{
		[Space(20)]
		public ControlMode controlMode = ControlMode.Local;
		public bool ShowEventLog = true;
		public bool ShowConsole = false;

		[Header("Refs")]
		public Transform NonActivePanelHolder = null;
		public Transform PanelHolder = null;
		public Transform RemotePanel = null;
		//public Transform RemotePanelHolder = null;
		public Transform ConsolePanel = null;
		// Local
		public PanelMessageBox _pMessageBox = null;
		public PanelConfigSelection _pConfigSelection = null;
		public PanelHeader _pHeader = null;
		public PanelApplicationSettings _pApplicationSettings = null;
		public PanelExperimentControl _pExperimentControl = null;

		// Remote
		//PanelConfigMaker _pConfigMaker = null;
		List<Transform> _currentPanelOrder = new List<Transform>();

		void Awake()
		{
			DontDestroyOnLoad(this);

			PreparePanels();
			
			Init();
			

            EventManager.StartListening(Edia.Events.Core.EvQuitApplication, OnEvQuitApplication);
		}

		void OnDestroy()
		{
			EventManager.StopListening (Edia.Events.ControlPanel.EvConnectionEstablished, OnEvConnectionEstablished);
		}

		void Init() {
			EventManager.showLog = ShowEventLog; // Eventmanager to show debug in console

			ConsolePanel.gameObject.SetActive(ShowConsole);

			if (controlMode is ControlMode.Remote) {
				EventManager.StartListening(Edia.Events.ControlPanel.EvConnectionEstablished, OnEvConnectionEstablished);
			}
			else
				InitConfigFileSearch();
		}

		void PreparePanels() {
			
			// Move all panels from task first to non visuable holder
			foreach (Transform t in PanelHolder) {
				t.SetParent(NonActivePanelHolder, true);
			}

			// Panels renaming
			foreach (Transform tr in NonActivePanelHolder) {
				tr.name = tr.GetSiblingIndex().ToString() + "_" + tr.name;
			}
		}

		void OnEvConnectionEstablished(eParam obj)
		{
			EventManager.StopListening(Edia.Events.ControlPanel.EvConnectionEstablished, OnEvConnectionEstablished);

			InitConfigFileSearch();
		}

        private void OnEvStartExperiment(eParam param)
        {
        }

        void InitConfigFileSearch()
		{
			_pConfigSelection.Init();
		}

		public void ShowPanel(Transform panel, bool onOff)
		{	
			panel.SetParent(onOff ? PanelHolder : NonActivePanelHolder, true);  
			UpdatePanelOrder();
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

        void OnEvQuitApplication(eParam obj)
        {
            this.ConsolePanel.Add2Console("Quiting..");
			Debug.Log($"{name}:Quiting..");
			Invoke("DoQuit", 1f);
        }

		void DoQuit () {
			Debug.Log($"{name}:Bye..");
			Application.Quit();
		}
    }
}