using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace eDIA
{
	/// <summary>Sample script to show the user a message in VR canvas</summary>
	public class MessagePanelInVR : Singleton<MessagePanelInVR>
	{
		[Header("Refs")]
		public TextMeshProUGUI MsgField = null;
		public GameObject MenuHolder = null;
		public Button buttonOK = null;
		public Button buttonProceed = null;
		private Image _backgroungImg = null;

		[Header("Settings")]
		public bool StickToHMD = true;
		public float DistanceFromHMD = 2f;
		public bool HasSolidBackground = true;
		//public float DefaultDuration = 3f;

		bool _hasClicked = false;

		Canvas _myCanvas = null;
		GraphicRaycaster _graphicRaycaster = null;
		TrackedDeviceGraphicRaycaster _trackedDeviceGraphicRaycaster = null;

		Coroutine _messageTimer = null;
		Coroutine _messageFader = null;

		private void Awake()
		{
			_myCanvas = GetComponent<Canvas>();
			_myCanvas.enabled = false;
			_graphicRaycaster = GetComponent<GraphicRaycaster>();
			_graphicRaycaster.enabled = false;
			_trackedDeviceGraphicRaycaster = GetComponent<TrackedDeviceGraphicRaycaster>();
			_trackedDeviceGraphicRaycaster.enabled = false;
			_backgroungImg = transform.GetChild(0).GetComponent<Image>();
			
			ButtonToggling(false,false);

			if (_myCanvas.worldCamera == null)
				_myCanvas.worldCamera = XRManager.Instance.camOverlay.GetComponent<Camera>();

			if (StickToHMD)
			{
				transform.SetParent(XRManager.Instance.XRCam, true);
				transform.localPosition = new Vector3(0, 0, DistanceFromHMD);
			}
		}

		void Start()
		{
			EventManager.StartListening(eDIA.Events.Core.EvShowMessageToUser, OnEvShowMessage);
			EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, OnEvHideMessage); //! assumption: continuing is always hide panel

		}

		void OnDestroy()
		{
			EventManager.StopListening(eDIA.Events.Core.EvShowMessageToUser, OnEvShowMessage);
			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, OnEvHideMessage);
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawIcon(new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z), "namename", true);
		}


		#region PANEL

		/// <summary>Shows the actual panel</summary>
		void ShowPanel(bool onOff)
		{
			GetComponent<Canvas>().enabled = onOff;

			// myCanvas.worldCamera.enabled = (disableOverlayCamOnHide && !onOff);
			_myCanvas.worldCamera.enabled = onOff;
			_graphicRaycaster.enabled = onOff;
			_trackedDeviceGraphicRaycaster.enabled = onOff;
		}


		#endregion
		#region MESSAGE OPTIONS	

		/// <summary>Event catcher</summary>
		void OnEvShowMessage(eParam e)
		{
			ShowMessage(e.GetString());
		}

		/// <summary>Shows the message in VR on a canvas.</summary>
		/// <param name="msg">Message to show</param>
		public void ShowMessage(string msg)
		{
			if (_messageTimer != null) StopCoroutine(_messageTimer);
			if (_messageFader != null) StopCoroutine(_messageFader);

			MsgField.text = msg;
			_messageFader = StartCoroutine(Fader());

			ShowPanel(true);
		}

		/// <summary>Shows the message in VR on a canvas for a certain duration.</summary>
		/// <param name="msg">Message to show</param>
		/// <param name="duration">Duration</param>
		public void ShowMessage(string msg, float duration)
		{
			ShowMessage(msg);

			_messageTimer = StartCoroutine("timer", duration);
		}

		/// <summary>Shows the message in VR on a canvas with button to proceed.</summary>
		/// <param name="msg">Message to show</param>
		/// <param name="duration">Duration</param>
		public void ShowMessage(string msg, bool showProceedButton)
		{
			ShowMessage(msg);

			if (showProceedButton) {
				ButtonToggling(true, true);

				// Also trigger proceed button on control panel
				EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "true" }));
				Experiment.Instance.WaitOnProceed();
			}
		}

		/// <summary>
		/// Shows a series of messages, user has to click OK button to go through them
		/// </summary>
		/// <param name="messages"></param>
		public void ShowMessage (List<string> messages) {

			ButtonToggling(messages.Count > 1 ? true : false, false);
			StartCoroutine(MessagesRoutine(messages));
			ShowPanel(true);
		}

		IEnumerator MessagesRoutine (List<string> messages) {

			foreach(string msg in messages) {
				ShowMessage(msg);
				_hasClicked = false;

				while (!_hasClicked) { 
					yield return new WaitForEndOfFrame();
				}
			}

			ButtonToggling (false, true);
		}

		void ButtonToggling(bool onOffOk, bool onOffProceed) {
			buttonOK.gameObject.SetActive(onOffOk);
			buttonProceed.gameObject.SetActive(onOffProceed);
			MenuHolder.SetActive(onOffProceed || onOffOk);
		}


		public void OnBtnOKPressed () {
			_hasClicked = true;
		}

		public void OnBtnProceedPressed() {
			EventManager.TriggerEvent(eDIA.Events.StateMachine.EvProceed);
		}


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region HIDE

		/// <summary>Event catcher</summary>
		void OnEvHideMessage(eParam e)
		{
			HidePanel();
		}

		/// <summary>Doublecheck running routines and hides the panel</summary>
		public void HidePanel()
		{
			if (_messageTimer != null) StopCoroutine(_messageTimer);
			if (_messageFader != null) StopCoroutine(_messageFader);
			ShowPanel(false);
			HideMenu();
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region MENU

		void HideMenu()
		{
			ButtonToggling(false, false);
		}


		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region TIMERS

		IEnumerator timer(float duration)
		{
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