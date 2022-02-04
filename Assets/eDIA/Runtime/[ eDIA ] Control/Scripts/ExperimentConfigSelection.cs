using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

namespace eDIA {

	/// <summary>GUI element that enables the user to choose from a dropdown of found experiment config files</summary>
	public class ExperimentConfigSelection : MonoBehaviour {

		[Header ("Refs")]
		public TMP_Dropdown taskOptions;
		public TMP_Dropdown configFilesOptions;
		public Button btnSubmit = null;
		public TextMeshProUGUI infoTextField;

		public enum ConfigTypes { TASK, PARTICIPANT };
		private string selectedTask = "empty";

		public string filter = "DEMO";
		
		void Start() {
			OnEvResetExperimentConfigSelection(null);
			EventManager.StartListening("EvResetExperimentConfigSelection", OnEvResetExperimentConfigSelection);
		}

		/// <summary>Repopulate the dropdowns with values</summary>
		private void OnEvResetExperimentConfigSelection(eParam obj)
		{
			Reset();
			GetLocalConfigs(ConfigTypes.TASK);
			UpdateParticipantConfigList();
		}

		/// <summary>Clear everything to startstate</summary>
		void Reset () {
			btnSubmit.interactable = false;
			infoTextField.text = "eDIA";
			configFilesOptions.ClearOptions();
			taskOptions.ClearOptions();

			transform.GetChild(0).gameObject.SetActive(true);
		}

		/// <summary>Update the participants list of selected task.</summary>
		public void UpdateParticipantConfigList() {

			configFilesOptions.ClearOptions();
			selectedTask = taskOptions.options.Count > 0 ? taskOptions.options[taskOptions.value].text : "NONE";

			if (selectedTask != "NONE")
				GetLocalConfigs(ConfigTypes.PARTICIPANT);
			else Debug.LogWarning("No valid task selected");
		}


		/// <summary>Generate an array with configfiles filenames from the given configtype</summary>
		void GetLocalConfigs (ConfigTypes configType) {
			infoTextField.text = "Looking for configs";

			string[] filelist = FileManager.GetAllFilenamesWithExtensionFrom(
				eDIA.Constants.localConfigDirectoryName +  (configType == ConfigTypes.TASK ? "/Tasks" : "/Participants"),"json"
				); // catch result in an array first to check if anything came back

			if (filelist == null) {
				Debug.Log("Local config files not found");
				infoTextField.text = "Nothing found!";
				return;
			}

			infoTextField.text = "Choose config file";

			// got filenames, fill the dropdown
			List<TMP_Dropdown.OptionData> fileOptions = new List<TMP_Dropdown.OptionData>();
			
			for (int s=0;s<filelist.Length;s++) {

				if (configType == ConfigTypes.TASK) {
					fileOptions.Add(new TMP_Dropdown.OptionData(filelist[s].Split('.')[0]));
					continue;
				}

				if (filelist[s].Contains('_') && filelist[s].Contains(selectedTask)) {
					fileOptions.Add(new TMP_Dropdown.OptionData(filelist[s].Split('.')[0].Split('_')[1])); // Fileformat: EXPERIMENTNAME_PARTICIPANTID.json
				} else
				 	Debug.LogWarning("[SKIPPED] " + filelist[s]);
			}

			if (configType == ConfigTypes.TASK) {
				taskOptions.AddOptions(fileOptions);
			} else {
				configFilesOptions.AddOptions(fileOptions);
				EventManager.TriggerEvent("EvFoundLocalConfigFiles", new eParam(configFilesOptions.options.Count));
			} 

			btnSubmit.interactable = true;
		}

		public void BtnSubmitPressed () {
			EventManager.TriggerEvent("EvLocalConfigSubmitted", new eParam(new string[] { taskOptions.options[taskOptions.value].text, configFilesOptions.options[configFilesOptions.value].text } )); // TASK / PARTICIPANT
			transform.GetChild(0).gameObject.SetActive(false);
		}



	}
}