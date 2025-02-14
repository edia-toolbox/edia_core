using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

		private void OnDestroy() {
			EventManager.StopListening(Edia.Events.ControlPanel.EvUpdateSessionSummary, OnEvUpdateSessionSummary );
		}

		public void LogoClicked() {
			            
			if (Edia.Controller.ControlPanel.Instance.ControlMode == Constants.ControlModes.Remote)
				if (!Edia.Controller.ControlPanel.Instance.IsConnected)
					EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam("Unable to retrieve settings, not connected", false));
			
			EventManager.TriggerEvent(Edia.Events.Settings.EvOpenSystemSettings);
		}

		void OnEvUpdateSessionSummary(eParam obj)
		{
			SetTitle(obj.GetStrings()[0]);
		}

		public void SetLogo (Sprite logoImage) {
			logo.sprite = logoImage;
		}

		void SetTitle (string title) {
			titleField.text = title;
		}

	}
}