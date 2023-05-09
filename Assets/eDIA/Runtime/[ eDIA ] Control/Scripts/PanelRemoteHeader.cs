using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
			if (obj.GetInt() is -1)
				return;

			logo.sprite = XRManager.Instance.GetXRDeviceIcon(obj.GetInt());
			titleField.text = XRManager.Instance.GetXRDeviceName(obj.GetInt());
		}

	}
}