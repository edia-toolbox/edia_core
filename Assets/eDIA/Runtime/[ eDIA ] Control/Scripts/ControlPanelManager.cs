using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA {

	public class ControlPanelManager : MonoBehaviour
	{
		public enum ControlPanelModes { Hidden, OnScreen, InWorld };
		public ControlPanelModes ControlPanelMode = ControlPanelModes.OnScreen;

		private void Awake() {
			EventManager.StartListening(eDIA.Events.GUI.EvSetControlPanelMode, OnEvSetControlPanelMode);
		}

		private void OnDestroy() {
			EventManager.StopListening(eDIA.Events.GUI.EvSetControlPanelMode, OnEvSetControlPanelMode);
		}

		private void OnEvSetControlPanelMode(eParam obj)
		{
			Debug.Log("OnEvSetControlPanelMode");
			SetControlPanelMode(obj.GetInt());
		}

		public void SetControlPanelMode (int mode) {
			Debug.Log("SetControlPanelMode");
		}
	}
}