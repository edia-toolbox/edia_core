using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Edia {
	/// <summary>Sample script to show the user a message in VR canvas</summary>
	public class MessagePanelInVR : ScreenInVR {
		[Header("Refs")]
		public TextMeshProUGUI MsgField = null;
		public GameObject MenuHolder = null;
		[FormerlySerializedAs("buttonOK")] public Button buttonNEXT = null;
		public Button buttonProceed = null;
		public bool HasSolidBackground = true;

		Image _backgroungImg = null;
		bool _hasClicked = false;
		List<string> messageQueue = new ();

		Coroutine _messageTimer = null;
		Coroutine _messagePanelFader = null;
		Coroutine _messageTextFader = null;
		Coroutine _messagesRoutine = null;

		// -- Singleton
		private static MessagePanelInVR instance = null;

		public static MessagePanelInVR Instance {
			get {
				if ((object)instance == null) {
					instance = (MessagePanelInVR)FindObjectOfType(typeof(MessagePanelInVR));

					if (instance == null) {
						GameObject singletonObject = new GameObject(typeof(MessagePanelInVR).ToString());
						instance = singletonObject.AddComponent<MessagePanelInVR>();
					}
				}
				return instance;
			}
		}

		// ---

		void Start() {
			_backgroungImg = transform.GetChild(0).GetComponent<Image>();

			//EventManager.StartListening(eDIA.Events.Core.EvShowMessageToUser, OnEvShowMessage); // TODO: Add for situation that a message has to be shown from remote control panel
			EventManager.StartListening(Edia.Events.StateMachine.EvProceed, OnEvHideMessage); //! assumption: continuing is always hide panel
		}

		void OnDestroy() {
			//EventManager.StopListening(eDIA.Events.Core.EvShowMessageToUser, OnEvShowMessage);
			EventManager.StopListening(Edia.Events.StateMachine.EvProceed, OnEvHideMessage);
		}


		#region MESSAGE OPTIONS	

		///// <summary>Event catcher</summary>
		//void OnEvShowMessage(eParam e) {
		//	ShowMessage(e.GetStrings().ToList());
		//}

		/// <summary>Shows the message in VR on a canvas for a certain duration.</summary>
		/// <param name="msg">Message to show</param>
		/// <param name="duration">Duration</param>
		public void ShowMessage(string msg, float duration) {
			ShowMessage(msg);
			_messageTimer = StartCoroutine(HidePanelAfter(duration));
			HideMenu();
		}

		/// <summary>
		/// Shows one message including proceed button
		/// </summary>
		/// <param name="msg"></param>
		public void ShowMessage (string msg) {
			ShowMessage(new List<string> { msg });
		}

		/// <summary>
		/// Shows a series of messages, user has to click OK button to go through them
		/// </summary>
		/// <param name="messages"></param>
		public void ShowMessage(List<string> messages) {

			messageQueue = messages;
			_messagesRoutine = StartCoroutine(ProcessMessageQueue());

			Show(true);
		}
		
		IEnumerator ProcessMessageQueue () {
			
			EventManager.StartListening(Edia.Events.ControlPanel.EvNextMessagePanelMsg, OnEvNextMessagepanelMsg);
			
			while (messageQueue.Count > 0) {
				ShowMessageOnPanel(messageQueue[0]);
				_hasClicked = false;

				while (!_hasClicked) {
					yield return new WaitForEndOfFrame();
				}

				messageQueue.RemoveAt(0);
			}
		}

		void ButtonToggling(bool onOffNext, bool onOffProceed) {
			buttonNEXT.gameObject.SetActive(onOffNext);
			buttonNEXT.interactable = onOffNext;

			string[] param = new[] { "NEXT", onOffNext.ToString() };
			EventManager.TriggerEvent(Edia.Events.ControlPanel.EvEnableButton, new eParam(param));
			
			buttonProceed.gameObject.SetActive(onOffProceed);
			buttonProceed.interactable = onOffProceed;
			
			MenuHolder.SetActive(onOffNext || onOffProceed);
		}

		private void OnEvNextMessagepanelMsg(eParam e) {
			OnBtnNEXTPressed();
		}

		public void OnBtnNEXTPressed() {
			_hasClicked = true;
		}

		public void OnBtnProceedPressed() {
			XRManager.Instance.EnableXROverlayRayInteraction(false);
			EventManager.TriggerEvent(Edia.Events.StateMachine.EvProceed);
		}

		/// <summary>Shows the message in VR on a canvas.</summary>
		/// <param name="msg">Message to show</param>
		public void ShowMessageOnPanel(string msg) {
			if (_messageTimer != null) StopCoroutine(_messageTimer);
			if (_messagePanelFader != null) StopCoroutine(_messagePanelFader);

			MsgField.text = msg;
			//XRManager.Instance.EnableXROverlayRayInteraction(true);

			ButtonToggling(messageQueue.Count > 1 ? true : false, messageQueue.Count == 1 ? true : false);

			_messageTextFader = _messageTextFader is not null ? null : StartCoroutine(Fader());

			Show(true);
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region HIDE

		/// <summary>Event catcher</summary>
		void OnEvHideMessage(eParam e) {
			HidePanel();
		}

		public override void Show (bool onOff) { 
			base.Show (onOff);
			
			EventManager.TriggerEvent(Edia.Events.XR.EvEnableXROverlay, new eParam(onOff));
			XRManager.Instance.EnableXROverlayRayInteraction(onOff);
			
			_messagePanelFader = _messagePanelFader is not null ? null : StartCoroutine(TextFader());
		}

		/// <summary>Doublecheck running routines and hides the panel</summary>
		public override void HidePanel() {

			if (_messageTimer != null) StopCoroutine(_messageTimer);
			if (_messagePanelFader != null) StopCoroutine(_messagePanelFader);

			messageQueue.Clear();
			HideMenu();
			
			base.HidePanel();
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region MENU

		public void HideMenu() {
			ButtonToggling(false, false);
			XRManager.Instance.EnableXROverlayRayInteraction(false);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TIMERS

		IEnumerator HidePanelAfter(float duration) {
			yield return new WaitForSeconds(duration);
			HidePanel();
		}

		IEnumerator Fader() {
			float duration = 0.5f;
			float currentTime = 0f;

			while (currentTime < duration) {
				float alpha = Mathf.Lerp(0f, 1f, currentTime / duration);
				float alphaBg = Mathf.Lerp(0f, HasSolidBackground ? 1f : 0.5f, currentTime / duration);
				_backgroungImg.color = new Color(_backgroungImg.color.r, _backgroungImg.color.g, _backgroungImg.color.b, alphaBg);
				currentTime += Time.deltaTime;
				yield return null;
			}
			yield break;
		}

		IEnumerator TextFader () {
			float duration = 0.5f;
			float currentTime = 0f;

			while (currentTime < duration) {
				float alpha = Mathf.Lerp(0f, 1f, currentTime / duration);
				MsgField.color = new Color(MsgField.color.r, MsgField.color.g, MsgField.color.b, alpha);
				currentTime += Time.deltaTime;
				yield return null;
			}
			yield break;
		}


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}
}