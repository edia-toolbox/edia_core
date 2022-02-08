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
		public TMP_Dropdown configFilesOptions;
		public Button btnSubmit = null;
		public TextMeshProUGUI infoTextField;

		public enum ConfigTypes { TASK, PARTICIPANT };
		private string selectedTask = "empty";

		void Start() {
			OnEvResetExperimentConfigSelection(null);
			EventManager.StartListening("EvResetExperimentConfigSelection", OnEvResetExperimentConfigSelection);
		}

		/// <summary>Repopulate the dropdowns with values</summary>
		private void OnEvResetExperimentConfigSelection(eParam obj)
		{
			Reset();
			// GetLocalConfigs(ConfigTypes.TASK);
			UpdateParticipantConfigList();
		}

		/// <summary>Clear everything to startstate</summary>
		void Reset () {
			btnSubmit.interactable = false;
			infoTextField.text = "eDIA";
			configFilesOptions.ClearOptions();

			ShowPanel();
		}

		/// <summary>Update the participants list of selected task.</summary>
		public void UpdateParticipantConfigList() {

			infoTextField.text = "Looking for configs";

			string[] filelist = FileManager.GetAllFilenamesWithExtensionFrom(	eDIA.Constants.localConfigDirectoryName + "/Participants","json" );

			if (filelist == null || filelist.Length == 0) {
				Debug.Log("Local config files not found");
				infoTextField.text = "Nothing found!";
				return;
			}

			infoTextField.text = "Choose config file";

			// got filenames, fill the dropdown
			List<TMP_Dropdown.OptionData> fileOptions = new List<TMP_Dropdown.OptionData>();
			
			for (int s=0;s<filelist.Length;s++) {
				if (isFileValid(filelist[s]))
					fileOptions.Add(new TMP_Dropdown.OptionData(filelist[s].Split('.')[0].Split('_')[1])); // Fileformat: EXPERIMENTNAME_PARTICIPANTID.json
			}

			configFilesOptions.AddOptions(fileOptions);
			EventManager.TriggerEvent("EvFoundLocalConfigFiles", new eParam(configFilesOptions.options.Count));

			btnSubmit.interactable = true;
		}


		bool isFileValid (string fileNameToCheck) {

			bool isValid = fileNameToCheck.ToUpper().Contains(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToUpper()) & fileNameToCheck.Contains('_');
			// Debug.LogWarning("[SKIPPED] " + fileNameToCheck);

			return isValid;
		}

		public void BtnSubmitPressed () {
			EventManager.TriggerEvent("EvLocalConfigSubmitted", new eParam(new string[] { UnityEngine.SceneManagement.SceneManager.GetActiveScene().name , configFilesOptions.options[configFilesOptions.value].text } )); // TASK / PARTICIPANT
			HidePanel();
		}

		void ShowPanel () {
			transform.GetChild(0).gameObject.SetActive(true);
			GetComponent<LayoutElement>().ignoreLayout = false;
		}

		void HidePanel () {
			transform.GetChild(0).gameObject.SetActive(false);
			GetComponent<LayoutElement>().ignoreLayout = true;

		}

	}
}