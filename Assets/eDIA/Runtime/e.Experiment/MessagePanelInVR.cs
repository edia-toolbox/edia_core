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
		Coroutine MessageTimer = null;

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
			HidePanel();
		}

		/// <summary>Shows the message in VR on a canvas for a certain duration.</summary>
		/// <param name="msg">Message to show</param>
		/// <param name="duration">Duration</param>
		public void ShowMessage (string msg, float duration) {
			ShowMessage(msg);

			MessageTimer = StartCoroutine("timer", duration);
		}
		
		IEnumerator timer (float duration) {
			yield return new WaitForSeconds(duration);
			HidePanel();
		}

		/// <summary>Shows the message in VR on a canvas until EvProceed event is fired</summary>
		/// <param name="msg">Message to show</param>
		/// <param name="hideOnEvProceed">Hide when EvProceed is fired</param>
		public void ShowMessage (string msg, bool hideOnEvProceed) {
			ShowMessage(msg);
			EventManager.StartListening(eDIA.Events.Core.EvProceed, OnEvHideMessage);
		}

		/// <summary>Shows the message in VR on a canvas.</summary>
		/// <param name="msg">Message to show</param>
		public void ShowMessage (string msg) {
			Debug.Log("Received: " + msg);
			if (MessageTimer != null) StopCoroutine ("MessageTimer");

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
			EventManager.StopListening(eDIA.Events.Core.EvProceed, OnEvHideMessage);
			EventManager.StartListening ("EvShowMessage", OnEvShowMessage);
		}

	}
}