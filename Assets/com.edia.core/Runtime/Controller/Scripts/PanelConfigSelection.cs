using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using Unity.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace Edia.Controller {

	/// <summary>GUI element that enables the user to choose from a dropdown of found experiment config files</summary>
	public class PanelConfigSelection : ExperimenterPanel {

		[Header("Refs")]
		public Button btnSubmit = null;

		List<string> _subFolders = new();
		List<string> _sessFolders = new();
		List<string> _taskDefinitionJsonStrings = new();
		List<string> _xBlockDefinitionJsonStrings = new();
		string _xBlockSequenceJsonString;
		string _sessionInfoJsonString;

		public TMP_Dropdown SubjectSelectionDropdown;
		public TMP_Dropdown SessionSelectionDropdown;

		public TextMeshProUGUI TopField;

		string _subject = "sub_001";
		string _session = "sess_001";

		public List<string> xBlockDefinitionsFileList; // for sanity check
		public XBlockSequence _xBlockSequence;

		public void Init() {
			Reset();

			EventManager.StartListening(Edia.Events.Config.EvFoundLocalConfigFiles, OnEvFoundLocalConfigFiles);

			string[] tempFolders = FileManager.GetAllSubFolders(Constants.PathToParticipantFiles);
			_subFolders.Clear();

			// Remove non valid foldernames
			foreach (string subFolder in tempFolders) {
				if (subFolder.StartsWith("sub-"))
					_subFolders.Add(subFolder);
			}

			if (_subFolders.Count == 0)
				return;

			GenerateDropdown(_subFolders, SubjectSelectionDropdown);
			SessionSelectionDropdown.interactable = true;
			OnSubjectValueChanged(0);
			OnSessValueChanged(0);

			TopField.text = "Select participant and session";

			btnSubmit.interactable = true;

			EventManager.TriggerEvent(Edia.Events.Config.EvFoundLocalConfigFiles);
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

			if (!ValidateConfigs())
				return;

			Debug.Log("Passed config file validation");

			EventManager.TriggerEvent(Edia.Events.Config.EvSetSessionInfo, new eParam(new string[] { _sessionInfoJsonString, _session.Split('-')[1], _subject.Split('-')[1] }));
			EventManager.TriggerEvent(Edia.Events.Config.EvSetXBlockSequence, new eParam(_xBlockSequenceJsonString));
			EventManager.TriggerEvent(Edia.Events.Config.EvSetTaskDefinitions, new eParam(_taskDefinitionJsonStrings.ToArray()));
			EventManager.TriggerEvent(Edia.Events.Config.EvSetXBlockDefinitions, new eParam(_xBlockDefinitionJsonStrings.ToArray()));

			HidePanel();
		}

		public void OnSubjectValueChanged(int value) {
			_subject = SubjectSelectionDropdown.options[value].text;

			List<string> subfolders = FileManager.GetAllSubFolders(Constants.PathToParticipantFiles + SubjectSelectionDropdown.options[value].text).ToList<string>();
			_sessFolders.Clear();

			foreach (string subfolder in subfolders) {
				if (subfolder.StartsWith("ses-"))
					_sessFolders.Add(subfolder);
			}

			if (_sessFolders.Count == 0) {
				SessionSelectionDropdown.ClearOptions();
				return;
			}

			GenerateDropdown(_sessFolders, SessionSelectionDropdown);
			OnSessValueChanged(0);
		}

		public void OnSessValueChanged(int value) {
			_session = SessionSelectionDropdown.options[value].text;
		}

		void GenerateDropdown(List<string> folderlist, TMP_Dropdown dropDown) {

			List<TMP_Dropdown.OptionData> tmpOptions = new List<TMP_Dropdown.OptionData>();

			for (int s = 0; s < folderlist.Count; s++) {
				tmpOptions.Add(new TMP_Dropdown.OptionData(folderlist[s]));
			}

			dropDown.ClearOptions();
			dropDown.AddOptions(tmpOptions);
		}

		void LoadJsons() {

			// Session info
			string currentPath = Constants.PathToParticipantFiles + _subject + "/" + _session + "/";
			_sessionInfoJsonString = FileManager.ReadStringFromApplicationPath(currentPath + Constants.FileNameSessionInfo);

			// Block sequence
			string eBSequenceFilePath = currentPath + Constants.FileNameSessionSequence;
			_xBlockSequenceJsonString = FileManager.ReadStringFromApplicationPath(eBSequenceFilePath);

			// Task definitions
			string[] filelist = FileManager.GetAllFilenamesWithExtensionFrom(Constants.PathToBaseDefinitions, "json");

			foreach (string s in filelist) {
				_taskDefinitionJsonStrings.Add(FileManager.ReadStringFromApplicationPath(Constants.PathToBaseDefinitions+ s));
			}

			// Block Task Definitions
			xBlockDefinitionsFileList = FileManager.GetAllFilenamesWithExtensionFrom(currentPath + Constants.FolderNameXBlockDefinitions, "json").ToList();

			foreach (string s in xBlockDefinitionsFileList) {
				string currentFileName = currentPath + "/" + Constants.FolderNameXBlockDefinitions + "/" + s;
				_xBlockDefinitionJsonStrings.Add(FileManager.ReadStringFromApplicationPath(currentFileName.ToLower()));
			}
		}

		private bool ValidateConfigs() {
			// Validate all strings on formatting

			if (!ValidateJSON(_sessionInfoJsonString)) {
				Debug.Log("JSON Format error: Session Info");
				EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam("JSON Format error: session_info.json", false ));
				return false;
			}

			if (!ValidateJSON(_xBlockSequenceJsonString)) {
				Debug.Log("JSON Format error: Xblock sequence");
				EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam("JSON Format error: session_sequence.json", false));
				return false;
			}

			foreach (string s in _taskDefinitionJsonStrings) {
				if (!ValidateJSON(s)) {
					Debug.Log("JSON Format error: Task definition");
					EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam("JSON Format error: Task definition", false));
					return false;
				}
			}

			foreach (string s in _xBlockDefinitionJsonStrings) {
				if (!ValidateJSON(s)) {
					Debug.Log("JSON Format error: Xblock definition");
					EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam("JSON Format error: Xblock definition", false));
					return false;
				}
			}

			// Sanity check - for each entry in the sequence file, there should be a file with the same name
			_xBlockSequence = UnityEngine.JsonUtility.FromJson<XBlockSequence>(_xBlockSequenceJsonString);

			foreach (string s in _xBlockSequence.Sequence) {
				bool found = false;
				//Debug.Log(" src: " + s.ToLower() );
				foreach ( string deffile in xBlockDefinitionsFileList )  {
					//Debug.Log(" check: " + deffile.Split('.')[0].ToLower());
					if ( s.ToLower() == deffile.Split('.')[0].ToLower() ) {
						found = true;
						break;
					}
				}
				if (!found) { 
					Debug.LogError("No <b>" + s + ".json</b> config file not found in xblock definitions folder");
					return false; 
				}
			}

			return true;
		}


		// Method to validate a string as JSON
		public bool ValidateJSON(string jsonString) {
			try {
				JsonUtility.FromJson<object>(jsonString);
				return true;
			}
			catch (System.Exception) {
				return false;
			}
		}
	}
}
