using eDIA;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfigLoader : MonoBehaviour {
	public string[] subFolders;
	public string[] sessFolders;

	public TMP_Dropdown SubjectSelectionDropdown;
	public TMP_Dropdown SessionSelectionDropdown;

	public TextMeshProUGUI SubjectField;
	public TextMeshProUGUI SessionField;

	string _subject = "na";
	string _session = "na";

	public string EBSequenceJsonString;
	public List<string> TaskJsonStrings = new ();
	public List<string> EBlockJsonStrings = new ();

	public SessionGenerator experimentGenerator;

	private void Start() {
		subFolders = FileManager.GetAllSubFolders("Configs/Participants");
		GenerateDropdown(subFolders, SubjectSelectionDropdown);
		SessionSelectionDropdown.interactable = true;
		OnSubjectValueChanged(0);
		OnSessValueChanged(0);
	}
	public void OnSubjectValueChanged (int value)  {
		_subject = SubjectSelectionDropdown.options[value].text;
		SubjectField.text = _subject;

		sessFolders = FileManager.GetAllSubFolders("Configs/Participants/" + SubjectSelectionDropdown.options[value].text);
		GenerateDropdown(sessFolders, SessionSelectionDropdown);
		OnSessValueChanged(0);
	}

	public void OnSessValueChanged(int value) {
		_session = SessionSelectionDropdown.options[value].text;
		SessionField.text = _session;
	}

	void GenerateDropdown(string[] folderlist, TMP_Dropdown dropDown) {

		List<TMP_Dropdown.OptionData> tmpOptions = new List<TMP_Dropdown.OptionData>();

		for (int s = 0; s < folderlist.Length; s++) {
			tmpOptions.Add(new TMP_Dropdown.OptionData(folderlist[s]));
		}

		dropDown.ClearOptions();
		dropDown.AddOptions(tmpOptions);
	}

	private void Reset() {
		SubjectSelectionDropdown.ClearOptions();
		SessionSelectionDropdown.ClearOptions();
		SessionSelectionDropdown.interactable = false;
	}

	public void OnLoadButtonPressed () {
		// start session
		Debug.Log("Load Jsons");
		LoadJsons();
	}

	public void OnStartButtonPressed() {
		// start session
		Debug.Log("Start Session");
		//experimentGenerator.StartExperiment();
	}

	private void LoadJsons () {

		string[] filelist = FileManager.GetAllFilenamesWithExtensionFrom("Configs/TaskDefinitions/", "json");

		foreach (string s in filelist) {
			TaskJsonStrings.Add(FileManager.ReadStringFromApplicationPath("Configs/TaskDefinitions/" + s));
			Debug.Log("Load: " + TaskJsonStrings[TaskJsonStrings.Count - 1]);
		}

		string currentPath = "Configs/Participants/" + _subject + "/" + _session + "/";
		
		string EBsequenceFilePath = currentPath + "EBlockSequence.json";
		EBSequenceJsonString = FileManager.ReadStringFromApplicationPath(EBsequenceFilePath);
		Debug.Log("Load: " + EBSequenceJsonString);
		
		filelist = FileManager.GetAllFilenamesWithExtensionFrom(currentPath + "/EBlockDefinitions", "json");

		foreach (string s in filelist) {
			EBlockJsonStrings.Add(FileManager.ReadStringFromApplicationPath(currentPath + "/EBlockDefinitions/" + s));
			Debug.Log("Load: " + EBlockJsonStrings[EBlockJsonStrings.Count - 1]);
		}

		SendJsonStrings();

	}

	private void SendJsonStrings() {
		//experimentGenerator.SetEBSequence(EBSequenceJsonString);
		//experimentGenerator.SetTaskDefinitions(TaskJsonStrings);
		//experimentGenerator.SetEBlockDefinitions(EBlockJsonStrings);
	}
}

