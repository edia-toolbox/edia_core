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
		Coroutine MessageFader = null;

		private void Awake() {
			myCanvas = GetComponent<Canvas>();
			myCanvas.enabled = false;

			if (myCanvas.worldCamera == null )
				myCanvas.worldCamera = XRrigManager.instance.XRrig_Cam.GetComponent<Camera>();
		}

		void Start () {
			EventManager.StartListening (eDIA.Events.Core.EvShowMessageToUser, 	OnEvShowMessage);
			EventManager.StartListening (eDIA.Events.Core.EvProceed, 			OnEvHideMessage); //! assumption: continuing is always hide panel
		}

		void OnDestroy () {
			EventManager.StopListening (eDIA.Events.Core.EvShowMessageToUser, 	OnEvShowMessage);
			EventManager.StopListening(eDIA.Events.Core.EvProceed, 			OnEvHideMessage);
		}


#region SHOW

		/// <summary>Event catcher</summary>
		void OnEvShowMessage (eParam e) {
			ShowMessage(e.GetString ());
		}

		/// <summary>Shows the actual panel</summary>
		void ShowPanel (bool onOff) {
			GetComponent<Canvas>().enabled = onOff;
		}

		/// <summary>Shows the message in VR on a canvas.</summary>
		/// <param name="msg">Message to show</param>
		public void ShowMessage (string msg) {
			if (MessageTimer != null) StopCoroutine ("MessageTimer");
			if (MessageFader != null) StopCoroutine ("MessageFader");

			msgField.text = msg;
			MessageFader = StartCoroutine("Fader");
			
			ShowPanel(true);
		}

		/// <summary>Shows the message in VR on a canvas for a certain duration.</summary>
		/// <param name="msg">Message to show</param>
		/// <param name="duration">Duration</param>
		public void ShowMessage (string msg, float duration) {
			ShowMessage(msg);

			MessageTimer = StartCoroutine("timer", duration);
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HIDE

		/// <summary>Event catcher</summary>
		void OnEvHideMessage (eParam e) {
			HidePanel();
		}

		/// <summary>Doublecheck running routines and hides the panel</summary>
		public void HidePanel () {
			if (MessageTimer != null) StopCoroutine ("MessageTimer");
			if (MessageFader != null) StopCoroutine ("MessageFader");
			ShowPanel(false);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HANDS

		IEnumerator timer (float duration) {
			yield return new WaitForSeconds(duration);
			HidePanel();
		}

		IEnumerator Fader()
		{
			float duration = 1f; //Fade out over 2 seconds.
			float currentTime = 0f;
			while (currentTime < duration)
			{
				float alpha = Mathf.Lerp(0f, 1f, currentTime / duration);
				msgField.color = new Color(msgField.color.r, msgField.color.g, msgField.color.b, alpha);
				currentTime += Time.deltaTime;
				yield return null;
			}
			yield break;
		}



#endregion // -------------------------------------------------------------------------------------------------------------------------------

	}
}