using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Edia {

	public class PanelHeader : MonoBehaviour {

		public Image logo = null;
		public TextMeshProUGUI titleField = null;

		void Awake () {
			//! Set the control panel gameobject on correct layer
			transform.root.gameObject.layer = LayerMask.NameToLayer("ControlUI");
		}

		void OnEnable() {
			EventManager.StartListening(Edia.Events.ControlPanel.EvUpdateSessionSummary, OnEvUpdateSessionSummary );
		}

		public void LogoClicked() {
			EventManager.TriggerEvent(Edia.Events.Settings.EvOpenSystemSettings);
		}

		void OnEvUpdateSessionSummary(eParam obj)
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