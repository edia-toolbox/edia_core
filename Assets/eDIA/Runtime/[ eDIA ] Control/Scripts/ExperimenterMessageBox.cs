using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace eDIA {

	public class ExperimenterMessageBox : MonoBehaviour {

		[Header("Refs")]
		public TextMeshProUGUI messageField = null;
		public Button panelButton = null;

		void Awake() {
			EventManager.StartListening(eDIA.gui.Events.EvShowMessageBox, OnEvShowMessageBox);
		}

		void OnDestroy() {
			EventManager.StopListening(eDIA.gui.Events.EvShowMessageBox, OnEvShowMessageBox);
		}

		private void OnEvShowMessageBox(eParam obj)
		{
			messageField.text = obj.GetString();
		}
	}

}