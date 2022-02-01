using Microsoft.VisualBasic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

namespace eDIA {

	public class ExperimentConfigSelection : MonoBehaviour {

		[Header ("Session configs")]
		public TMP_Dropdown configFilesOptions;
		public Button btnSubmit = null;
		public TextMeshProUGUI infoTextField;

		public List<string> localConfigFilenames = new List<string>();

		void Start() {
			Reset();

			GetLocalExperimentConfigs();
		}

		void Reset () {
			btnSubmit.interactable = false;
			infoTextField.text = "eDIA";
			configFilesOptions.ClearOptions();
			localConfigFilenames.Clear();
		}

		/// <summary>Generate an array with  configfiles filenames from the given subfolder</summary>
		void GetLocalExperimentConfigs () {
			infoTextField.text = "Looking for configs";

			string[] filelist = FileManager.GetAllFilenamesFrom(edia.Constants.localConfigDirectoryName,"json"); // catch result in an array first to check if anything came back

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
				fileOptions.Add(new TMP_Dropdown.OptionData(filelist[s].Split('.')[0]));
			}

			configFilesOptions.AddOptions(fileOptions);

			btnSubmit.interactable = true;
			EventManager.TriggerEvent("EvFoundLocalConfigFiles", new eParam(localConfigFilenames.Count));
		}

		public void BtnSubmitPressed () {
			EventManager.TriggerEvent("EvLocalConfigSubmitted", new eParam(localConfigFilenames[configFilesOptions.value]));
		}



	}
}