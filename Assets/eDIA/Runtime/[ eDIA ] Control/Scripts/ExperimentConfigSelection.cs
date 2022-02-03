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

		List<string> localConfigFilenames = new List<string>();

		void Start() {
			OnEvResetExperimentConfigSelection(null);
			EventManager.StartListening("EvResetExperimentConfigSelection", OnEvResetExperimentConfigSelection);
		}

		private void OnEvResetExperimentConfigSelection(eParam obj)
		{
			Reset();
			GetLocalExperimentConfigs();
		}

		void Reset () {
			btnSubmit.interactable = false;
			infoTextField.text = "eDIA";
			configFilesOptions.ClearOptions();
			localConfigFilenames.Clear();
			transform.GetChild(0).gameObject.SetActive(true);
		}

		/// <summary>Generate an array with  configfiles filenames from the given subfolder</summary>
		void GetLocalExperimentConfigs () {
			infoTextField.text = "Looking for configs";

			string[] filelist = FileManager.GetAllFilenamesFrom(eDIA.Constants.localConfigDirectoryName,"json"); // catch result in an array first to check if anything came back

			if (filelist == null) {
				// AddToLog("Local config files not found");
				Debug.Log("Local config files not found");
				infoTextField.text = "Nothing found!";
				return;
			}

			infoTextField.text = "Choose config file";
			localConfigFilenames = filelist.ToList<string>();

			// got filenames, fill the dropdown
			List<TMP_Dropdown.OptionData> fileOptions = new List<TMP_Dropdown.OptionData>();
			
			for (int s=0;s<filelist.Length;s++) {
				if (filelist[s].Contains('_')) {
					fileOptions.Add(new TMP_Dropdown.OptionData(filelist[s].Split('.')[0].Split('_')[1])); // Fileformat: EXPERIMENTNAME_PARTICIPANTID.json
				} else
				 	Debug.LogWarning("[SKIPPED] Incorrect filename " + filelist[s] + " <EXPERIMENTNAME_PARTICIPANTID>.json");
			}

			configFilesOptions.AddOptions(fileOptions);

			btnSubmit.interactable = true;
			EventManager.TriggerEvent("EvFoundLocalConfigFiles", new eParam(localConfigFilenames.Count));
		}

		public void BtnSubmitPressed () {
			EventManager.TriggerEvent("EvLocalConfigSubmitted", new eParam(localConfigFilenames[configFilesOptions.value]));
			transform.GetChild(0).gameObject.SetActive(false);
		}



	}
}