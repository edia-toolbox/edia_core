using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using eDIA.Manager;

namespace eDIA {

	public class PanelRemoteHeader : MonoBehaviour {

		public Image logo = null;
		public TextMeshProUGUI titleField = null;

		void Awake () {
			//! Set the control panel gameobject on correct layer
			transform.root.gameObject.layer = LayerMask.NameToLayer("ControlUI");
		}

		void OnEnable() {
			EventManager.StartListening(eDIA.Events.ControlPanel.EvConnectionEstablished, OnEvConnectionEstablished);
		}

		private void OnEvConnectionEstablished(eParam obj)
		{
			Debug.Log("OnEvConnectionEstablished:" + obj.GetInt());

			if (obj.GetInt() is -1)
				return;

			logo.sprite = ControlPanel.Instance.GetXRDeviceIcon(obj.GetInt());
			titleField.text = ControlPanel.Instance.GetXRDeviceName(obj.GetInt());
		}

	}
}