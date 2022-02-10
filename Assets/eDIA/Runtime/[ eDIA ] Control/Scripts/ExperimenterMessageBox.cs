using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace eDIA {

	public class ExperimenterMessageBox : ExperimenterPanel {

		[Header("Refs")]
		public TextMeshProUGUI messageField = null;
		public Button panelButton = null;

		void Awake() {
			EventManager.StartListening(eDIA.Events.GUI.EvShowMessageBox, OnEvShowMessageBox);
			panelButton.onClick.AddListener(buttonClicked);
			
			HidePanel ();
		}

		void OnDestroy() {
			EventManager.StopListening(eDIA.Events.GUI.EvShowMessageBox, OnEvShowMessageBox);
		}

		private void OnEvShowMessageBox(eParam obj)
		{
			messageField.text = obj.GetString();

			ShowPanel();
		}

		void buttonClicked() {
			Debug.Log("buttonClicked");
			HidePanel();
		}

	}
}