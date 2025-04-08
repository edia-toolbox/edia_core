using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using Unity.Properties;
using static Edia.Constants;

namespace Edia.Controller {

	/// <summary>GUI element that enables the user to choose from a dropdown of found experiment config files</summary>
	public class PanelConfigSelection : ExperimenterPanel {

		[Header("Refs")]
		public Button btnSubmit = null;

		List<string> _subFolders                  = new();
		List<string> _sesFolders                  = new();
		List<string> _baseDefinitionJsonStrings   = new();
		List<string> _xBlockDefinitionJsonStrings = new();
		string       _xBlockSequenceJsonString;
		string       _sessionInfoJsonString;
		string       _sessionJsonString;

		public TMP_Dropdown SubjectSelectionDropdown;
		public TMP_Dropdown SessionSelectionDropdown;

		public TextMeshProUGUI TopField;

		string _subject = "";
		string _session = "";

		public List<string> xBlockDefinitionsFileList; // for sanity check
		public XBlockSequence _xBlockSequence;

		/// <summary>
		/// Initializes the panel configuration selection process.
		/// This method resets existing data, sets up event listeners, retrieves and filters the
		/// list of relevant participant folders, and updates associated UI components such as dropdown lists.
		/// Additionally, it makes necessary elements interactable and triggers the initial start-up event for the process.
		/// </summary>
		public void Init() {
			Reset();

			EventManager.StartListening(Edia.Events.Config.EvFoundLocalConfigFiles, OnEvFoundLocalConfigFiles);

			string[] tempFolders = FileManager.GetAllSubFolders(PathToParticipantFiles);
			_subFolders.Clear();

			// Remove non valid folder names
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

			ControlPanel.Instance.AddToConsoleLog("Passed config file validation");

			EventManager.TriggerEvent(Edia.Events.Config.EvSetSessionInfo, new eParam(new string[] { _sessionInfoJsonString, _session.Split('-')[1], _subject.Split('-')[1] }));
			EventManager.TriggerEvent(Edia.Events.Config.EvSetXBlockSequence, new eParam(_xBlockSequenceJsonString));
			EventManager.TriggerEvent(Edia.Events.Config.EvSetBaseDefinitions, new eParam(_baseDefinitionJsonStrings.ToArray()));
			EventManager.TriggerEvent(Edia.Events.Config.EvSetXBlockDefinitions, new eParam(_xBlockDefinitionJsonStrings.ToArray()));

			HidePanel();
		}

		public void OnSubjectValueChanged(int value) {
			_subject = SubjectSelectionDropdown.options[value].text;

			List<string> subfolders = FileManager.GetAllSubFolders(PathToParticipantFiles + SubjectSelectionDropdown.options[value].text).ToList<string>();
			_sesFolders.Clear();

			foreach (string subfolder in subfolders) {
				if (subfolder.StartsWith("ses-"))
					_sesFolders.Add(subfolder);
			}

			if (_sesFolders.Count == 0) {
				SessionSelectionDropdown.ClearOptions();
				return;
			}

			GenerateDropdown(_sesFolders, SessionSelectionDropdown);
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
			string currentPath = PathToParticipantFiles + _subject + "/" + _session + "/";
			_sessionInfoJsonString = FileManager.ReadStringFromApplicationPath(currentPath + FileNameSessionInfo);

			// Block sequence
			string eBSequenceFilePath = currentPath + FileNameSessionSequence;
			_xBlockSequenceJsonString = FileManager.ReadStringFromApplicationPath(eBSequenceFilePath);

			// Task definitions
			string[] filelist = FileManager.GetAllFilenamesWithExtensionFrom(PathToBaseDefinitions, "json");

			foreach (string s in filelist) {
				_baseDefinitionJsonStrings.Add(FileManager.ReadStringFromApplicationPath(PathToBaseDefinitions + s));
			}

			// Block Task Definitions
			xBlockDefinitionsFileList = FileManager.GetAllFilenamesWithExtensionFrom(currentPath + FolderNameXBlockDefinitions, "json").ToList();

			foreach (string s in xBlockDefinitionsFileList) {
				string currentFileName = currentPath + "/" + FolderNameXBlockDefinitions + "/" + s;
				_xBlockDefinitionJsonStrings.Add(FileManager.ReadStringFromApplicationPath(currentFileName.ToLower()));
			}
		}

		private bool ValidateConfigs() {
			// Validate all strings on formatting

			if (!ValidateJSON(_sessionJsonString)) {
				ControlPanel.Instance.AddToConsoleLog("JSON Format error: Session", LogType.Error);
				EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam("JSON Format error: session.json", false ));
				return false;
			}

			
			if (!ValidateJSON(_sessionInfoJsonString)) {
				ControlPanel.Instance.AddToConsoleLog("JSON Format error: Session Info", LogType.Error);
				EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam("JSON Format error: session_info.json", false ));
				return false;
			}

			if (!ValidateJSON(_xBlockSequenceJsonString)) {
				ControlPanel.Instance.AddToConsoleLog("JSON Format error: Xblock sequence", LogType.Error);
				EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam("JSON Format error: session_sequence.json", false));
				return false;
			}

			foreach (string s in _baseDefinitionJsonStrings) {
				if (!ValidateJSON(s)) {
					ControlPanel.Instance.AddToConsoleLog("JSON Format error: Base definition", LogType.Error);
					EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam("JSON Format error: Base definition", false));
					return false;
				}
			}

			foreach (string s in _xBlockDefinitionJsonStrings) {
				if (!ValidateJSON(s)) {
					ControlPanel.Instance.AddToConsoleLog("JSON Format error: Xblock definition", LogType.Error);
					EventManager.TriggerEvent(Edia.Events.ControlPanel.EvShowMessageBox, new eParam("JSON Format error: Xblock definition", false));
					return false;
				}
			}

			// Sanity check - for each entry in the sequence file, there should be a file with the same name
			_xBlockSequence = UnityEngine.JsonUtility.FromJson<XBlockSequence>(_xBlockSequenceJsonString);

			foreach (string s in _xBlockSequence.Sequence) {
				bool found = false;
				//ControlPanel.Instance.AddToConsoleLog(" src: " + s.ToLower() );
				foreach ( string deffile in xBlockDefinitionsFileList )  {
					//ControlPanel.Instance.AddToConsoleLog(" check: " + deffile.Split('.')[0].ToLower());
					if ( s.ToLower() == deffile.Split('.')[0].ToLower() ) {
						found = true;
						break;
					}
				}
				if (!found) { 
					ControlPanel.Instance.AddToConsoleLog("No <b>" + s + ".json</b> config file not found in xblock definitions folder", LogType.Error);
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
