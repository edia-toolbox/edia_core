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
		public float timeToShow = 2f;

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

		private void OnEvShowMessageBox(eParam obj)
		{
			Debug.Log("OnEvShowMessageBox");
			messageField.text = obj.GetStringBoolString();
			
			ShowPanel();

			children[1].gameObject.SetActive(!obj.GetStringBoolBool());

			if (obj.GetStringBoolBool()) 
				StartCoroutine(AutoHide());
		}

		IEnumerator AutoHide () {
			yield return new WaitForSeconds(timeToShow);

			HidePanel();
		}

		void buttonClicked() {
			HidePanel();
		}

	}
}