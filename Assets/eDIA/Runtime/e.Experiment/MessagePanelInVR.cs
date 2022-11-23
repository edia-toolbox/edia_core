using System.Collections;
using System.Collections.Generic;
using eDIA;
using TMPro;
using UnityEngine;

namespace TASK {

	/// <summary>Sample script to show the user a message in VR canvas</summary>
	public class MessagePanelInVR : MonoBehaviour {
		
		public TextMeshProUGUI msgField = null;
		Canvas myCanvas = null;

		private void Awake() {
			myCanvas = GetComponent<Canvas>();
			myCanvas.enabled = false;

			if (myCanvas.worldCamera == null )
				Debug.LogError("Event Camera reference to XRCam is missing!");
		}

		void Start () {
			EventManager.StartListening ("EvShowMessage", OnEvShowMessage);
		}

		void OnDestroy () {
			EventManager.StopListening ("EvShowMessage", OnEvShowMessage);
			EventManager.StopListening ("EvHideMessage", OnEvHideMessage);
		}


		void OnEvShowMessage (eParam e) {
			ShowMessage(e.GetString ());
		}

		void OnEvHideMessage (eParam e) {
			EventManager.StopListening(eDIA.Events.Core.EvProceed, OnEvHideMessage);
			HidePanel();
		}

		public void ShowMessage (string msg, float duration) {
			ShowMessage(msg);
			Invoke("HidePanel", duration);
		}

		public void ShowMessage (string msg, bool hideOnButtonClick) {
			ShowMessage(msg);
			EventManager.StartListening(eDIA.Events.Core.EvProceed, OnEvHideMessage);
		}


		public void ShowMessage (string msg) {
			if (msgField == null)
				return;

			msgField.text = msg;

			EventManager.StartListening ("EvHideMessage", OnEvHideMessage);
			
			ShowPanel(true);
		}

		void ShowPanel (bool onOff) {
			GetComponent<Canvas>().enabled = onOff;
		}

		public void HidePanel () {
			ShowPanel(false);
			EventManager.StartListening ("EvShowMessage", OnEvShowMessage);
		}

	}
}