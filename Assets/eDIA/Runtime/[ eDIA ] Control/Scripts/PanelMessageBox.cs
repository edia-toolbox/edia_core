using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace eDIA.Manager
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

			EventManager.StartListening(eDIA.Events.ControlPanel.EvShowMessageBox, OnEvShowMessageBox);
			panelButton.onClick.AddListener(buttonClicked);
		}

		void Start()
		{
			HidePanel();
		}

		void OnDestroy()
		{
			EventManager.StopListening(eDIA.Events.ControlPanel.EvShowMessageBox, OnEvShowMessageBox);
		}

#region MESSAGE PANEL

		public void ShowMessage(string msg, bool autoHide)
		{
			messageField.text = msg;
			panelButton.gameObject.SetActive(!autoHide);
			if (autoHide is true) StartCoroutine(AutoHide());
			Invoke("ShowPanel",0.01f); //! Intentionally delayed as on startup the panellayoutmanager is too quick
		}

		/// <summary> Shows the message box. Expects string[], param[0] = message, param[1] = autohide true/false </summary>
		private void OnEvShowMessageBox(eParam obj)
		{
			ShowMessage(obj.GetStringBool_String(), obj.GetStringBool_Bool());
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HELPERS

		IEnumerator AutoHide()
		{
			yield return new WaitForSeconds(autoHideTimer);
			HidePanel();
		}

		void buttonClicked()
		{
			HidePanel();
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------

	}
}