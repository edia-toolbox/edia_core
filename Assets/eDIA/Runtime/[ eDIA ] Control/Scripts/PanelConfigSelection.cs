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

		[Header("Refs")]
		public Button btnSubmit = null;

		public string[] _subFolders;
		public string[] _sessFolders;
		public List<string> _taskDefinitionJsonStrings = new();
		public List<string> _eBlockDefinitionJsonStrings = new();
		string _eBSequenceJsonString;
		string _sessionJsonString;

		public TMP_Dropdown SubjectSelectionDropdown;
		public TMP_Dropdown SessionSelectionDropdown;

		public TextMeshProUGUI SubjectField;
		public TextMeshProUGUI SessionField;

		string _subject = "sub_001";
		string _session = "sess_001";

		string _pathToParticipantFiles		= "Configs/participants/";
		string _pathToTaskFiles				= "Configs/task-definitions/";
		string _eBlockDefinitionsFolderName		= "eblock-definitions";
		string _eBlockSequenceFileName		= "eblock_sequence.json";
		string _sessionInfoFilenName			= "session_info.json";


		public void Init() {
			Reset();

			EventManager.StartListening(eDIA.Events.Config.EvFoundLocalConfigFiles, OnEvFoundLocalConfigFiles);

			_subFolders = FileManager.GetAllSubFolders(_pathToParticipantFiles);

			GenerateDropdown(_subFolders, SubjectSelectionDropdown);
			SessionSelectionDropdown.interactable = true;
			OnSubjectValueChanged(0);
			OnSessValueChanged(0);

			btnSubmit.interactable = true;

			EventManager.TriggerEvent(eDIA.Events.Config.EvFoundLocalConfigFiles);
		}

		void OnEvFoundLocalConfigFiles(eParam e) {
			Invoke("ShowPanel", 0.1f); // Small delay to be sure the Awake method collected all child transforms to toggle
		}

		/// <summary>Clear everything to startstate</summary>
		void Reset() {
			SubjectSelectionDropdown.ClearOptions();
			SessionSelectionDropdown.ClearOptions();
			SessionSelectionDropdown.interactable = false;
			btnSubmit.interactable = false;
		}

		public void OnSubmitBtnPressed() {
			LoadJsons();

			EventManager.TriggerEvent(eDIA.Events.Config.EvSetSessionInfo, new eParam(new string[] { _sessionJsonString, _session.Split('-')[1], _subject.Split('-')[1] }));
			EventManager.TriggerEvent(eDIA.Events.Config.EvSetEBlockSequence, new eParam(_eBSequenceJsonString));
			EventManager.TriggerEvent(eDIA.Events.Config.EvSetTaskDefinitions, new eParam(_taskDefinitionJsonStrings.ToArray()));
			EventManager.TriggerEvent(eDIA.Events.Config.EvSetEBlockDefinitions, new eParam(_eBlockDefinitionJsonStrings.ToArray()));

			HidePanel();
		}

		public void OnSubjectValueChanged(int value) {
			_subject = SubjectSelectionDropdown.options[value].text;
			SubjectField.text = _subject.Split('-')[1];

			_sessFolders = FileManager.GetAllSubFolders(_pathToParticipantFiles + SubjectSelectionDropdown.options[value].text);
			GenerateDropdown(_sessFolders, SessionSelectionDropdown);
			OnSessValueChanged(0);
		}

		public void OnSessValueChanged(int value) {
			_session = SessionSelectionDropdown.options[value].text;
			SessionField.text = _session.Split('-')[1]; 
		}

		void GenerateDropdown(string[] folderlist, TMP_Dropdown dropDown) {

			List<TMP_Dropdown.OptionData> tmpOptions = new List<TMP_Dropdown.OptionData>();

			for (int s = 0; s < folderlist.Length; s++) {
				tmpOptions.Add(new TMP_Dropdown.OptionData(folderlist[s]));
			}

			dropDown.ClearOptions();
			dropDown.AddOptions(tmpOptions);
		}

		void LoadJsons() {

			// Task definitions
			string[] filelist = FileManager.GetAllFilenamesWithExtensionFrom(_pathToTaskFiles, "json");

			foreach (string s in filelist) {
				_taskDefinitionJsonStrings.Add(FileManager.ReadStringFromApplicationPath(_pathToTaskFiles + s));
			}

			// Session info
			string currentPath = _pathToParticipantFiles + _subject + "/" + _session + "/";
			_sessionJsonString = FileManager.ReadStringFromApplicationPath(currentPath + _sessionInfoFilenName);

			// Block sequence
			string eBSequenceFilePath = currentPath + _eBlockSequenceFileName;
			_eBSequenceJsonString = FileManager.ReadStringFromApplicationPath(eBSequenceFilePath);

			// Block Task Definitions
			filelist = FileManager.GetAllFilenamesWithExtensionFrom(currentPath + _eBlockDefinitionsFolderName, "json");

			foreach (string s in filelist) {
				_eBlockDefinitionJsonStrings.Add(FileManager.ReadStringFromApplicationPath(currentPath + "/" + _eBlockDefinitionsFolderName + "/" + s));
			}
		}
	}
}