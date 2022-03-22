using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace eDIA {

	public class PanelApplicationSettings : ExperimenterPanel {

		// Default buttons that are always needed for running a experiment
		[Header("Buttons")]
		public Button btnApply = null;
		public Button btnClose = null;

		[Header("Settings")]
		public Slider volumeSlider = null;
		public TMP_Dropdown resolutionDropdown = null;
		public TMP_Dropdown primaryHandDropdown = null;
		public TMP_Dropdown activeInteractorDropdown = null;
		public TMP_Dropdown languageDropdown = null;

		private SettingsDeclaration localSystemSettingsContainer = null;
		private bool hasChanged = false;

		public override void Awake() {

			base.Awake();

			HidePanel ();
			SetupButtons ();

			EventManager.StartListening(eDIA.Events.Core.EvOpenSystemSettings, OnEvOpenSystemSettings);
		}

		void OnDestroy() {
			EventManager.StopListening(eDIA.Events.Core.EvOpenSystemSettings, OnEvOpenSystemSettings);
		}

#region EVENT LISTENERS

		private void OnEvOpenSystemSettings(eParam obj)
		{
			// Get the current stored settings
			localSystemSettingsContainer = UnityEngine.JsonUtility.FromJson<SettingsDeclaration>(obj.GetString());

			// populate the GUI elements with correct values
			volumeSlider.value = localSystemSettingsContainer.volume;
			primaryHandDropdown.value = (int)localSystemSettingsContainer.primaryInteractor;
			activeInteractorDropdown.value = (int)localSystemSettingsContainer.interactor;
			languageDropdown.value = (int)localSystemSettingsContainer.language;
			// resolutionDropdown.value = localSystemSettingsContainer.screenResolution;

			// Show
			ShowPanel();

			btnApply.interactable = false ;

			Debug.Log("OnEvOpenSystemSettings: " + obj.GetString());
		}
		

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BUTTONPRESSES

		public void ValueChanged () {
			hasChanged = true;
			btnApply.interactable = true;
		}

		void BtnApplyPressed () {
			// Something has changed
			UpdateLocalSettings();

			EventManager.TriggerEvent(eDIA.Events.Core.EvUpdateSystemSettings, new eParam ( UnityEngine.JsonUtility.ToJson(localSystemSettingsContainer)));
		}



#endregion // -------------------------------------------------------------------------------------------------------------------------------

		void UpdateLocalSettings () {

			localSystemSettingsContainer.volume = volumeSlider.value;
			localSystemSettingsContainer.primaryInteractor = (Constants.Interactor)primaryHandDropdown.value;
			localSystemSettingsContainer.interactor = (Constants.Interactor)activeInteractorDropdown.value;
			localSystemSettingsContainer.language = (Constants.Languages)languageDropdown.value;

			// resolutionDropdown.value = localSystemSettingsContainer.screenResolution;
		}

		void SetupButtons () {
			btnApply.onClick.AddListener(	()=> BtnApplyPressed());
			btnClose.onClick.AddListener(	()=> HidePanel ());
		}
	}
}