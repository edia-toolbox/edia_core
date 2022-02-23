using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace eDIA {

	public class ApplicationSettings : ExperimenterPanel {

		// Default buttons that are always needed for running a experiment
		[Header("Buttons")]
		public Button btnApply = null;
		public Button btnClose = null;

		[Header("Settings")]
		public Slider volumeSlider = null;
		public TMP_Dropdown resolutionDropdown = null;
		public TMP_Dropdown primaryHandDropdown = null;
		public TMP_Dropdown languageDropdown = null;


		public SettingsDeclaration localSystemSettingsContainer = null;

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
			languageDropdown.value = (int)localSystemSettingsContainer.language;
			
			// Show
			ShowPanel();

			Debug.Log("OnEvOpenSystemSettings: " + obj.GetString());
		}
		

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BUTTONPRESSES



#endregion // -------------------------------------------------------------------------------------------------------------------------------

		void SetupButtons () {
			btnApply.onClick.AddListener(	()=> EventManager.TriggerEvent(eDIA.Events.Core.EvUpdateSystemSettings, null));
			btnClose.onClick.AddListener(	()=> HidePanel ());
		}
	}
}