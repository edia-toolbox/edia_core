using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace eDIA {

	public class PanelHeader : MonoBehaviour {

		public Image logo = null;
		public TextMeshProUGUI titleField = null;

		void Awake () {
			//! Set the control panel gameobject on correct layer
			transform.root.gameObject.layer = LayerMask.NameToLayer("ControlUI");
		}

		void OnEnable() {
			EventManager.StartListening(eDIA.Events.Config.EvLocalConfigSubmitted, OnEvLocalConfigSubmitted );
		}

		public void LogoClicked() {
			EventManager.TriggerEvent(eDIA.Events.Settings.EvRequestSystemSettings, null);
		}

		// Received a file name to load, so we know now what task this is as it's part of the param
		void OnEvLocalConfigSubmitted(eParam obj)
		{
			SetTitle(obj.GetStrings()[0]);
		}

		void SetLogo (Sprite logoImage) {
			logo.sprite = logoImage;
		}

		void SetTitle (string title) {
			titleField.text = title;
		}

	}
}