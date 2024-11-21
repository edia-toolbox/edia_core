using System;
using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Edia;

namespace Edia.Controller {

	public class PanelApplicationSettings : ExperimenterPanel {

		// Default buttons that are always needed for running a experiment
		[Header ("Buttons")]
		public Button btnApply = null;
		public Button btnClose = null;
		public Button btnBrowse = null;
		public Button btnQuit = null;

		[Header ("Settings")]
		public Slider volumeSlider = null;
		public TMP_Dropdown resolutionDropdown = null;
		public TMP_Dropdown interactiveInteractorDropdown = null;
		public TMP_Dropdown visibleInteractorDropdown = null;
		public TMP_Dropdown languageDropdown = null;
		public TextMeshProUGUI pathToLogfilesField = null;

		public SettingsDeclaration localSystemSettingsContainer = null;

		public override void Awake () {
			base.Awake ();

			HidePanel ();
			SetupPanels ();

			EventManager.StartListening (Edia.Events.Settings.EvOpenSystemSettings, OnEvOpenSystemSettings);
		}

		void OnDestroy () {
			EventManager.StopListening (Edia.Events.Settings.EvOpenSystemSettings, OnEvOpenSystemSettings);
		}

#region EVENT LISTENERS

		private void OnEvOpenSystemSettings (eParam obj) {

			// Where to get the settings from?
			if (ControlPanel.Instance.Settings.ControlMode == ControlMode.Local) {
				// ask systemsettings singleton via event
				EventManager.StartListening(Edia.Events.Settings.EvProvideSystemSettings, OnProcessSystemSettings);
				EventManager.TriggerEvent(Edia.Events.Settings.EvRequestSystemSettings);
			} else {
				// TODO: In case of remote, does the controlpanel get settings locally from file?
			}

		}

		/// <summary>
		/// Processes given JSON string with systemsettings, shows panel with updated info
		/// </summary>
		/// <param name="obj">Systemsettings package as JSON string</param>
		private void OnProcessSystemSettings (eParam obj) {
			EventManager.StopListening(Edia.Events.Settings.EvProvideSystemSettings, OnProcessSystemSettings);

			// Get the current stored settings
			localSystemSettingsContainer = UnityEngine.JsonUtility.FromJson<SettingsDeclaration> (obj.GetString ());

			// populate the GUI elements with correct values
			//volumeSlider.value 			= localSystemSettingsContainer.volume;
			interactiveInteractorDropdown.value = (int) localSystemSettingsContainer.InteractiveInteractor;
			visibleInteractorDropdown.value 	= (int) localSystemSettingsContainer.VisableInteractor;
			//languageDropdown.value 			= (int) localSystemSettingsContainer.language;
			pathToLogfilesField.text 		= localSystemSettingsContainer.pathToLogfiles;
			//resolutionDropdown.value 		= localSystemSettingsContainer.screenResolution;

			ShowPanel ();

			btnApply.interactable = false;
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
		#region BUTTONPRESSES

		public void ValueChanged () {
			btnApply.interactable = true;
		}

		void BtnApplyPressed () {
			// Something has changed
			UpdateLocalSettings ();

			EventManager.TriggerEvent (Edia.Events.Settings.EvUpdateSystemSettings, new eParam (UnityEngine.JsonUtility.ToJson (localSystemSettingsContainer, false)));
		}

		void BtnQuitPressed() {
			Debug.Log($"{name}:Quit request sent");
			EventManager.TriggerEvent(Edia.Events.Core.EvQuitApplication);
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

			//localSystemSettingsContainer.volume = volumeSlider.value;
			localSystemSettingsContainer.InteractiveInteractor = (Edia.Constants.Interactor) interactiveInteractorDropdown.value;
			localSystemSettingsContainer.VisableInteractor = (Edia.Constants.Interactor) visibleInteractorDropdown.value;
			//localSystemSettingsContainer.language = (Edia.Constants.Languages) languageDropdown.value;
			//localSystemSettingsContainer.screenResolution = resolutionDropdown.value;
		}

		void SetupPanels () {
			btnApply.onClick.AddListener (() => BtnApplyPressed ());
			btnClose.onClick.AddListener (() => HidePanel ());
			btnBrowse.onClick.AddListener (() => OpenFileBrowser ());
			btnQuit.onClick.AddListener(() => BtnQuitPressed());

			foreach (Vector2 s in Edia.Constants.screenResolutions) {
				TMP_Dropdown.OptionData n = new TMP_Dropdown.OptionData(String.Format("{0}x{1}", s.x, s.y));
				resolutionDropdown.options.Add(n);
			}
		}


	}
}