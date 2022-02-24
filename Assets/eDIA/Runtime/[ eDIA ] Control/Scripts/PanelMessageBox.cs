using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace eDIA {

	public class PanelMessageBox : ExperimenterPanel {

		[Header("Refs")]
		public TextMeshProUGUI messageField = null;
		public Button panelButton = null;

		[Header("Settings")]
		public float autoHideTimer = 2f;

		public override void Awake() {

			base.Awake();

			EventManager.StartListening(eDIA.Events.GUI.EvShowMessageBox, OnEvShowMessageBox);
			panelButton.onClick.AddListener(buttonClicked);
		}

		void Start() {
			HidePanel ();
		}

		void OnDestroy() {
			EventManager.StopListening(eDIA.Events.GUI.EvShowMessageBox, OnEvShowMessageBox);
		}

#region EVENT LISTENERS

		/// <summary> Shows the message box. Expects string[], param[0] = message, param[1] = autohide true/false </summary>
		private void OnEvShowMessageBox(eParam obj)
		{
			messageField.text = obj.GetStringBoolString();
			
			ShowPanel();

			children[1].gameObject.SetActive(!obj.GetStringBoolBool());

			if (obj.GetStringBoolBool()) 
				StartCoroutine(AutoHide());
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HELPERS
		
		IEnumerator AutoHide () {
			yield return new WaitForSeconds(autoHideTimer);

			HidePanel();
		}

		void buttonClicked() {
			HidePanel();
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------

	}
}