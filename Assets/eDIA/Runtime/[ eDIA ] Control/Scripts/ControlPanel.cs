using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA.Manager {

	public class ControlPanel : Singleton<ControlPanel>
	{
		[HideInInspector]
		public Transform 				NonActivePanelHolder = null;
		public Transform 				MenuPanelHolder = null;
		
		[Space(20)]
		public ControlSettings Settings;

		private PanelMessageBox 		_pMessageBox = null;
		private PanelConfigSelection 		_pConfigSelction = null;
		private PanelHeader 			_pHeader = null;
		private PanelApplicationSettings 	_pApplicationSettings = null;
		private PanelExperimentControl 	_pExperimentControl = null;
		private PanelConsole			_pConsole = null;
		
		private List<Transform> 		_currentPanelOrder = new List<Transform>();



		private void Awake() {

			GetPanelReferences();

			if (Settings.AutoInitialise)
				Init ();
	
		}

		private void Init()
		{
			// Panel control
			foreach (Transform tr in NonActivePanelHolder) {
				tr.name = tr.GetSiblingIndex().ToString() + "_" + tr.name;
			}

			// Settings
			if (Settings.LookForLocalConfigs)
				_pConfigSelction.Init();

			_pConsole.ShowConsole(Settings.ShowConsole);
			
		}

		private void OnDestroy() {
		}


		void GetPanelReferences () {

			_pMessageBox 		= GetComponentInChildren<PanelMessageBox>();
			_pConfigSelction 		= GetComponentInChildren<PanelConfigSelection>();
			_pHeader 			= GetComponentInChildren<PanelHeader>();
			_pApplicationSettings 	= GetComponentInChildren<PanelApplicationSettings>();
			_pExperimentControl 	= GetComponentInChildren<PanelExperimentControl>();
			_pConsole			= GetComponentInChildren<PanelConsole>();
			
		}

		public void ShowPanel (Transform panel, bool onOff) {
			
			panel.SetParent(onOff? MenuPanelHolder : NonActivePanelHolder, true);
			UpdatePanelOrder();
		}
		

		public void UpdatePanelOrder () {

			_currentPanelOrder.Clear();
			_currentPanelOrder = MenuPanelHolder.Cast<Transform>().ToList();
			_currentPanelOrder.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });

			for (int i=0; i<_currentPanelOrder.Count; ++i) {
				_currentPanelOrder[i].SetSiblingIndex(i);
			}
		}


		public void ShowMessage (string msg, bool autoHide) {
			_pMessageBox.ShowMessage(msg, autoHide);
		}

		public void Add2Console (string msg) {
			_pConsole.Add2Console(msg);
		}


	}
}