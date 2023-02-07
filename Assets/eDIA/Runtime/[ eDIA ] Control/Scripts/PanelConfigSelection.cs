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
		public string configFileTaskName = "TASK";

		public void Init() {
			Reset();

			EventManager.StartListening(eDIA.Events.Config.EvFoundLocalConfigFiles, OnEvFoundLocalConfigFiles);
			
			GenerateParticipantConfigList();

		}

		void OnEvFoundLocalConfigFiles (eParam e) {
			Invoke("ShowPanel", 0.1f); // Small delay to be sure the Awake method collected all child transforms to toggle
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

			Debug.Log("Configs files found: " + filelist.Length);

			if (filelist == null || filelist.Length == 0) {
				Debug.Log("No files found");
				infoTextField.text = "No files found!";
				return;
			}

			// got filenames, fill the dropdown
			List<TMP_Dropdown.OptionData> fileOptions = new List<TMP_Dropdown.OptionData>();

			for (int s=0;s<filelist.Length;s++) {
				if (isFileValid(filelist[s]))
					fileOptions.Add(new TMP_Dropdown.OptionData(filelist[s].Split('.')[0].Split('_')[1])); // Fileformat: EXPERIMENTNAME_PARTICIPANTID.json
			}
			
			if (fileOptions.Count == 0) {
				infoTextField.text = "No valid configs found!";
				Debug.Log("No valid configs found");
				return;
			}

			infoTextField.text = "Choose config file";

			configFilesOptions.AddOptions(fileOptions);
			EventManager.TriggerEvent(eDIA.Events.Config.EvFoundLocalConfigFiles, new eParam(configFilesOptions.options.Count));

			btnSubmit.interactable = true;

		}




		bool isFileValid (string fileNameToCheck) {

			bool isValid = fileNameToCheck.ToUpper().Contains(configFileTaskName.ToUpper()) & fileNameToCheck.Contains('_');
			// if (!isValid) Debug.LogWarning("[SKIPPED] " + fileNameToCheck);

			return isValid;
		}

		public void BtnSubmitPressed () {
			
			string filenameExperiment = configFileTaskName + "_" + configFilesOptions.options[configFilesOptions.value].text + ".json"; // combine task string and participant string
			
			EventManager.TriggerEvent( eDIA.Events.Config.EvSetExperimentConfig, 
				new eParam( 
					@" {""experiment"":""TaskA"",""experimenter"":""eDIA"",""session_number"":0,""participant_details"":[{""key"":""ID"",""value"":""SOMEID""},{""key"":""Age"",""value"":""23""},{""key"":""Name"",""value"":""Heinrich""}]} "
					// FileManager.ReadStringFromApplicationPathSubfolder(eDIA.Constants.localConfigDirectoryName + "/Participants", filenameExperiment)
				)
			);

			string filenameTask = configFileTaskName + ".json"; // task string
			
			EventManager.TriggerEvent(
				eDIA.Events.Config.EvSetTaskConfig, 
				new eParam (
					FileManager.ReadStringFromApplicationPathSubfolder(eDIA.Constants.localConfigDirectoryName + "/Tasks", filenameTask)
				)
			);
			
			HidePanel();
		}
	}
}