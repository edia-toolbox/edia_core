using System;
using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using eDIA;

namespace eDIA.Manager {

	public class PanelApplicationSettings : ExperimenterPanel {

		// Default buttons that are always needed for running a experiment
		[Header ("Buttons")]
		public Button btnApply = null;
		public Button btnClose = null;
		public Button btnBrowse = null;

		[Header ("Settings")]
		public Slider volumeSlider = null;
		public TMP_Dropdown resolutionDropdown = null;
		public TMP_Dropdown interactiveInteractorDropdown = null;
		public TMP_Dropdown visibleInteractorDropdown = null;
		public TMP_Dropdown languageDropdown = null;
		public TextMeshProUGUI pathToLogfilesField = null;

		public SettingsDeclaration localSystemSettingsContainer = null;
		private bool hasChanged = false;

		public override void Awake () {
			base.Awake ();

			HidePanel ();
			SetupPanels ();

			EventManager.StartListening (eDIA.Events.Settings.EvOpenSystemSettings, OnEvOpenSystemSettings);
		}

		void OnDestroy () {
			EventManager.StopListening (eDIA.Events.Settings.EvOpenSystemSettings, OnEvOpenSystemSettings);
		}

#region EVENT LISTENERS

		private void OnEvOpenSystemSettings (eParam obj) {
			// Get the current stored settings
			localSystemSettingsContainer = UnityEngine.JsonUtility.FromJson<SettingsDeclaration> (obj.GetString ());

			// populate the GUI elements with correct values
			volumeSlider.value 			= localSystemSettingsContainer.volume;
			interactiveInteractorDropdown.value = (int) localSystemSettingsContainer.InteractiveInteractor;
			visibleInteractorDropdown.value 	= (int) localSystemSettingsContainer.VisableInteractor;
			languageDropdown.value 			= (int) localSystemSettingsContainer.language;
			pathToLogfilesField.text 		= localSystemSettingsContainer.pathToLogfiles;
			resolutionDropdown.value 		= localSystemSettingsContainer.screenResolution;
			// Show
			ShowPanel ();

			btnApply.interactable = false;
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BUTTONPRESSES

		public void ValueChanged () {
			hasChanged = true;
			btnApply.interactable = true;
		}

		void BtnApplyPressed () {
			// Something has changed
			UpdateLocalSettings ();

			EventManager.TriggerEvent (eDIA.Events.Settings.EvUpdateSystemSettings, new eParam (UnityEngine.JsonUtility.ToJson (localSystemSettingsContainer, false)));
		}

		void OpenFileBrowser () {
			StartCoroutine( ShowLoadDialogCoroutine() );
		}

		IEnumerator ShowLoadDialogCoroutine () {
			yield return FileBrowser.WaitForLoadDialog (FileBrowser.PickMode.Folders, false, null, null, "Select Folder", "Select");

			if (FileBrowser.Success) {

				if (FileBrowser.Result[0] != localSystemSettingsContainer.pathToLogfiles) {
					localSystemSettingsContainer.pathToLogfiles = FileBrowser.Result[0];
					Debug.Log(localSystemSettingsContainer.pathToLogfiles);
					pathToLogfilesField.text = FileBrowser.Result[0];
					ValueChanged ();
				}
			}
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------

		void UpdateLocalSettings () {

			localSystemSettingsContainer.volume = volumeSlider.value;
			localSystemSettingsContainer.InteractiveInteractor = (eDIA.Constants.Interactor) interactiveInteractorDropdown.value;
			localSystemSettingsContainer.VisableInteractor = (eDIA.Constants.Interactor) visibleInteractorDropdown.value;
			localSystemSettingsContainer.language = (eDIA.Constants.Languages) languageDropdown.value;
			localSystemSettingsContainer.screenResolution = resolutionDropdown.value;
		}

		void SetupPanels () {
			btnApply.onClick.AddListener (() => BtnApplyPressed ());
			btnClose.onClick.AddListener (() => HidePanel ());
			btnBrowse.onClick.AddListener (() => OpenFileBrowser ());

			foreach (Vector2 s in eDIA.Constants.screenResolutions) {
				TMP_Dropdown.OptionData n = new TMP_Dropdown.OptionData(String.Format("{0}x{1}", s.x, s.y));
				resolutionDropdown.options.Add(n);
			}
		}
	}
}