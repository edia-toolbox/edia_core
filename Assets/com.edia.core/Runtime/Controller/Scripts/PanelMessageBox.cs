using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Edia.Controller
{
	public class PanelMessageBox : ExperimenterPanel
	{
		[Header("Refs")]
		public TextMeshProUGUI messageField = null;
		public Button panelButton = null;

		[Header("Settings")]
		public float autoHideTimer = 2f;

		public override void Awake()
		{
			base.Awake();

			EventManager.StartListening(Edia.Events.ControlPanel.EvShowMessageBox, OnEvShowMessageBox);
			panelButton.onClick.AddListener(buttonClicked);
			children.RemoveAt(1);
		}

		void Start()
		{
			HidePanel();
		}

		void OnDestroy()
		{
			EventManager.StopListening(Edia.Events.ControlPanel.EvShowMessageBox, OnEvShowMessageBox);
			EventManager.StopListening(Edia.Events.StateMachine.EvProceed, OnEvProceed);
		}

#region MESSAGE PANEL

		public void ShowMessage(string msg, bool autoHide)
		{
			messageField.text = msg;
			EventManager.StartListening(Edia.Events.StateMachine.EvProceed, OnEvProceed);

			if (autoHide is true) {
				StartCoroutine(AutoHide());
			} else panelButton.gameObject.SetActive(true);

			Invoke("ShowPanel", 0.01f); //! Intentionally delayed as on startup the panellayoutmanager is too quick
		}

		/// <summary> Shows the message box. Expects string[], param[0] = message, param[1] = autohide true/false </summary>
		private void OnEvShowMessageBox(eParam obj)
		{
			ShowMessage(obj.GetStringBool_String(), obj.GetStringBool_Bool());
		}

		private void OnEvProceed(eParam obj) {
			// Hide panel when proceed is called
			EventManager.StopListening(Edia.Events.StateMachine.EvProceed, OnEvProceed);
			HidePanel();
			panelButton.gameObject.SetActive(false);
		}
#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HELPERS

		IEnumerator AutoHide()
		{
			panelButton.gameObject.SetActive(false);
			yield return new WaitForSecondsRealtime(autoHideTimer);
			HidePanel();
		}

		void buttonClicked()
		{
			HidePanel();
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------

	}
}