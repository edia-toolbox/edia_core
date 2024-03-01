using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace eDIA {
	/// <summary>Sample script to show the user a message in VR canvas</summary>
	public class MessagePanelInVR : ScreenInVR {
		[Header("Refs")]
		public TextMeshProUGUI MsgField = null;
		public GameObject MenuHolder = null;
		public Button buttonOK = null;
		public Button buttonProceed = null;
		public bool HasSolidBackground = true;
		
		Image _backgroungImg = null;
		bool _hasClicked = false;
		
		Coroutine _messageTimer = null;
		Coroutine _messageFader = null;

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
			EventManager.StartListening(eDIA.Events.Core.EvShowMessageToUser, OnEvShowMessage);
			EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, OnEvHideMessage); //! assumption: continuing is always hide panel
		}

		void OnDestroy() {
			EventManager.StopListening(eDIA.Events.Core.EvShowMessageToUser, OnEvShowMessage);
			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, OnEvHideMessage);
		}


		#region MESSAGE OPTIONS	

		/// <summary>Event catcher</summary>
		void OnEvShowMessage(eParam e) {
			ShowMessage(e.GetString());
		}

		/// <summary>Shows the message in VR on a canvas.</summary>
		/// <param name="msg">Message to show</param>
		public void ShowMessage(string msg) {
			if (_messageTimer != null) StopCoroutine(_messageTimer);
			if (_messageFader != null) StopCoroutine(_messageFader);

			MsgField.text = msg;
			_messageFader = StartCoroutine(Fader());

			ShowPanel(true);
		}

		/// <summary>Shows the message in VR on a canvas for a certain duration.</summary>
		/// <param name="msg">Message to show</param>
		/// <param name="duration">Duration</param>
		public void ShowMessage(string msg, float duration) {
			ShowMessage(msg);

			_messageTimer = StartCoroutine("timer", duration);
		}

		/// <summary>Shows the message in VR on a canvas with button to proceed.</summary>
		/// <param name="msg">Message to show</param>
		/// <param name="duration">Duration</param>
		public void ShowMessage(string msg, bool showProceedButton) {
			ShowMessage(msg);

			if (showProceedButton) {
				ButtonToggling(true, true);

				// Also trigger proceed button on control panel
				EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "true" }));
				Xperiment.Instance.WaitOnProceed();
			}
		}

		/// <summary>
		/// Shows a series of messages, user has to click OK button to go through them
		/// </summary>
		/// <param name="messages"></param>
		public void ShowMessage(List<string> messages) {

			ButtonToggling(messages.Count > 1 ? true : false, false);
			StartCoroutine(MessagesRoutine(messages));
			ShowPanel(true);
		}

		IEnumerator MessagesRoutine(List<string> messages) {

			foreach (string msg in messages) {
				ShowMessage(msg);
				_hasClicked = false;

				while (!_hasClicked) {
					yield return new WaitForEndOfFrame();
				}
			}

			ButtonToggling(false, true);
		}

		void ButtonToggling(bool onOffOk, bool onOffProceed) {
			buttonOK.gameObject.SetActive(onOffOk);
			buttonProceed.gameObject.SetActive(onOffProceed);
			MenuHolder.SetActive(onOffProceed || onOffOk);
		}


		public void OnBtnOKPressed() {
			_hasClicked = true;
		}

		public void OnBtnProceedPressed() {
			EventManager.TriggerEvent(eDIA.Events.StateMachine.EvProceed);
		}


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region HIDE

		/// <summary>Event catcher</summary>
		void OnEvHideMessage(eParam e) {
			HidePanel();
		}

		/// <summary>Doublecheck running routines and hides the panel</summary>
		public void HidePanel() {
			if (_messageTimer != null) StopCoroutine(_messageTimer);
			if (_messageFader != null) StopCoroutine(_messageFader);
			ShowPanel(false);
			HideMenu();
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region MENU

		void HideMenu() {
			ButtonToggling(false, false);
		}


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region TIMERS

		IEnumerator timer(float duration) {
			yield return new WaitForSeconds(duration);
			HidePanel();
		}

		IEnumerator Fader() {
			float duration = 0.5f;
			float currentTime = 0f;
			while (currentTime < duration) {
				// float alpha = Mathf.Lerp(0f, hasSolidBackground ? 1f : 0.5f, currentTime / duration);
				float alpha = Mathf.Lerp(0f, 1f, currentTime / duration);
				MsgField.color = new Color(MsgField.color.r, MsgField.color.g, MsgField.color.b, alpha);
				float alphaBg = Mathf.Lerp(0f, HasSolidBackground ? 1f : 0.5f, currentTime / duration);
				_backgroungImg.color = new Color(_backgroungImg.color.r, _backgroungImg.color.g, _backgroungImg.color.b, alphaBg);
				currentTime += Time.deltaTime;
				yield return null;
			}
			yield break;
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}
}