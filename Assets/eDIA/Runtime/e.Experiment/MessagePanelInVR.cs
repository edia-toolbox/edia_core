using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace eDIA {

	/// <summary>Sample script to show the user a message in VR canvas</summary>
	public class MessagePanelInVR : Singleton<MessagePanelInVR> {
		
		[Header("Refs")]
		public TextMeshProUGUI msgField = null;
		public Button button = null;
		public GameObject menuHolder = null;
		private Image bg = null;

		[Header("Settings")]
		public bool stickToHMD = true;
		public float distanceFromHMD = 2f;
		public bool hasSolidBackground = true;

		Canvas myCanvas = null;
		Coroutine MessageTimer = null;
		Coroutine MessageFader = null;

		private void Awake() {
			myCanvas = GetComponent<Canvas>();
			myCanvas.enabled = false;
			menuHolder.SetActive(false);
			bg = transform.GetChild(0).GetComponent<Image>();

			if (myCanvas.worldCamera == null )
				myCanvas.worldCamera = XRManager.Instance.camOverlay.GetComponent<Camera>();

			if (stickToHMD) {
				transform.SetParent(XRManager.Instance.XRCam, true);
				transform.localPosition = new Vector3(0,0,distanceFromHMD);
			}
			
		}

		void Start () {
			EventManager.StartListening (eDIA.Events.Core.EvShowMessageToUser, 	OnEvShowMessage);
			EventManager.StartListening (eDIA.Events.StateMachine.EvProceed, 		OnEvHideMessage); //! assumption: continuing is always hide panel

		}

		void OnDestroy () {
			EventManager.StopListening (eDIA.Events.Core.EvShowMessageToUser, 	OnEvShowMessage);
			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, 		OnEvHideMessage);
		}

		private void OnDrawGizmos() {
			Gizmos.DrawIcon(new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z), "namename", true);
		}


#region SHOW

		/// <summary>Event catcher</summary>
		void OnEvShowMessage (eParam e) {
			ShowMessage(e.GetString ());
		}

		/// <summary>Shows the actual panel</summary>
		void ShowPanel (bool onOff) {
			GetComponent<Canvas>().enabled = onOff;

			// myCanvas.worldCamera.enabled = (disableOverlayCamOnHide && !onOff);
			myCanvas.worldCamera.enabled = onOff;
		}

		/// <summary>Shows the message in VR on a canvas.</summary>
		/// <param name="msg">Message to show</param>
		public void ShowMessage (string msg) {

			if (MessageTimer != null) StopCoroutine (MessageTimer);
			if (MessageFader != null) StopCoroutine (MessageFader);

			msgField.text = msg;
			MessageFader = StartCoroutine(Fader());
			
			ShowPanel(true);
		}

		/// <summary>Shows the message in VR on a canvas for a certain duration.</summary>
		/// <param name="msg">Message to show</param>
		/// <param name="duration">Duration</param>
		public void ShowMessage (string msg, float duration) {
			ShowMessage(msg);

			MessageTimer = StartCoroutine("timer", duration);
		}

		/// <summary>Shows the message in VR on a canvas with button to proceed.</summary>
		/// <param name="msg">Message to show</param>
		/// <param name="duration">Duration</param>
		public void ShowMessage (string msg, bool showButton) {
			ShowMessage(msg);
			if (showButton)
				ShowMenu();
		}


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HIDE

		/// <summary>Event catcher</summary>
		void OnEvHideMessage (eParam e) {
			HidePanel();
		}

		/// <summary>Doublecheck running routines and hides the panel</summary>
		public void HidePanel () {
			if (MessageTimer != null) StopCoroutine (MessageTimer);
			if (MessageFader != null) StopCoroutine (MessageFader);
			ShowPanel(false);
			HideMenu();
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region MENU

		void ShowMenu()
		{
			menuHolder.SetActive(true);
			button.onClick.AddListener ( BtnPressed );

			// Also trigger proceed button on control panel
			Experiment.Instance.WaitOnProceed();
		}

		void HideMenu()
		{
			menuHolder.SetActive(false);
			button.onClick.RemoveListener ( BtnPressed );
		}
	
		public void BtnPressed () {
			EventManager.TriggerEvent(eDIA.Events.StateMachine.EvProceed, null);
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TIMERS

		IEnumerator timer (float duration) {
			yield return new WaitForSeconds(duration);
			HidePanel();
		}

		IEnumerator Fader()
		{
			float duration = 0.5f; 
			float currentTime = 0f;
			while (currentTime < duration)
			{
				// float alpha = Mathf.Lerp(0f, hasSolidBackground ? 1f : 0.5f, currentTime / duration);
				float alpha = Mathf.Lerp(0f, 1f, currentTime / duration);
				msgField.color = new Color(msgField.color.r, msgField.color.g, msgField.color.b, alpha);
				float alphaBg = Mathf.Lerp(0f, hasSolidBackground ? 1f : 0.5f, currentTime / duration);
				bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, alphaBg);
				currentTime += Time.deltaTime;
				yield return null;
			}
			yield break;
		}



#endregion // -------------------------------------------------------------------------------------------------------------------------------

	}
}