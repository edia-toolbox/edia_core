using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

namespace eDIA.Manager {

	/// <summary>GUI element that enables the user to choose from a dropdown of found experiment config files</summary>
	public class PanelConfigSelection : ExperimenterPanel {

		[Header ("Refs")]
		public TMP_Dropdown configFilesOptions;
		public Button btnSubmit = null;
		public TextMeshProUGUI infoTextField;


		public void Init() {

			Reset();
			EventManager.StartListening(eDIA.Events.Core.EvFoundLocalConfigFiles, OnEvFoundLocalConfigFiles);
			GenerateParticipantConfigList();

		}

		void OnEvFoundLocalConfigFiles (eParam e) {
			ShowPanel();
		}

		/// <summary>Clear everything to startstate</summary>
		void Reset () {
			btnSubmit.interactable = false;
			infoTextField.text = "eDIA";
			configFilesOptions.ClearOptions();

		}

		/// <summary>Update the participants list of selected task.</summary>
		public void GenerateParticipantConfigList() {

			infoTextField.text = "Looking for configs";

			string[] filelist = FileManager.GetAllFilenamesWithExtensionFrom(	eDIA.Constants.localConfigDirectoryName + "/Participants","json" );

			if (filelist == null || filelist.Length == 0) {
				ControlPanel.Instance.Add2Console("No files found");
				infoTextField.text = "No files found!";
				return;
			}

			// got filenames, fill the dropdown
			List<TMP_Dropdown.OptionData> fileOptions = new List<TMP_Dropdown.OptionData>();
			
			for (int s=0;s<filelist.Length;s++) {
				if (isFileValid(filelist[s]))
					fileOptions.Add(new TMP_Dropdown.OptionData(filelist[s].Split('.')[0].Split('_')[1])); // Fileformat: EXPERIMENTNAME_PARTICIPANTID.json
			}

			if (fileOptions.Count is 0) {
				infoTextField.text = "No valid configs found!";
				ControlPanel.Instance.Add2Console("No valid configs found");
				return;
			}

			infoTextField.text = "Choose config file";

			configFilesOptions.AddOptions(fileOptions);
			EventManager.TriggerEvent(eDIA.Events.Core.EvFoundLocalConfigFiles, new eParam(configFilesOptions.options.Count));

			btnSubmit.interactable = true;
		}


		bool isFileValid (string fileNameToCheck) {

			bool isValid = fileNameToCheck.ToUpper().Contains(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToUpper()) & fileNameToCheck.Contains('_');
			// Debug.LogWarning("[SKIPPED] " + fileNameToCheck);

			return isValid;
		}

		public void BtnSubmitPressed () {
			// Set up param as: TASK / PARTICIPANT
			string[] param = new string[] { UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, configFilesOptions.options[configFilesOptions.value].text };
			
			EventManager.TriggerEvent(eDIA.Events.Core.EvLocalConfigSubmitted, new eParam(param)); 
			
			HidePanel();
		}
	}
}