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

	  public string[] subFolders;
	  public string[] sessFolders;

	  public TMP_Dropdown SubjectSelectionDropdown;
	  public TMP_Dropdown SessionSelectionDropdown;

	  public TextMeshProUGUI SubjectField;
	  public TextMeshProUGUI SessionField;

	  string _subject = "na";
	  string _session = "na";

	  string EBSequenceJsonString;
	  public List<string> TaskDefinitionJsonStrings = new();
	  public List<string> EBlockDefinitionJsonStrings = new();


	  public void Init()
	  {
		Reset();

		EventManager.StartListening(eDIA.Events.Config.EvFoundLocalConfigFiles, OnEvFoundLocalConfigFiles);

		subFolders = FileManager.GetAllSubFolders("Configs/Participants");

		GenerateDropdown(subFolders, SubjectSelectionDropdown);
		SessionSelectionDropdown.interactable = true;
		OnSubjectValueChanged(0);
		OnSessValueChanged(0);

		EventManager.TriggerEvent(eDIA.Events.Config.EvFoundLocalConfigFiles);
	  }

	  void OnEvFoundLocalConfigFiles(eParam e)
	  {
		Invoke("ShowPanel", 0.1f); // Small delay to be sure the Awake method collected all child transforms to toggle
	  }

	  /// <summary>Clear everything to startstate</summary>
	  void Reset()
	  {
		SubjectSelectionDropdown.ClearOptions();
		SessionSelectionDropdown.ClearOptions();
		SessionSelectionDropdown.interactable = false;

		btnSubmit.interactable = false;
	  }

	  public void OnSubmitBtnPressed()
	  {
		LoadJsons();

		// Send over EBSequence json
		// Send over TaskDefinitionJson list
		// Send over EBlockDefinitionJson list

		EventManager.TriggerEvent(eDIA.Events.Config.EvSetEBlockSequence, new eParam(EBSequenceJsonString));
		EventManager.TriggerEvent(eDIA.Events.Config.EvSetTaskDefinitions, new eParam(TaskDefinitionJsonStrings));
		EventManager.TriggerEvent(eDIA.Events.Config.EvSetEBlockDefinitions, new eParam(EBlockDefinitionJsonStrings));

		HidePanel();
	  }


	  public void OnSubjectValueChanged(int value)
	  {
		_subject = SubjectSelectionDropdown.options[value].text;
		SubjectField.text = _subject;

		sessFolders = FileManager.GetAllSubFolders("Configs/Participants/" + SubjectSelectionDropdown.options[value].text);
		GenerateDropdown(sessFolders, SessionSelectionDropdown);
		OnSessValueChanged(0);
	  }

	  public void OnSessValueChanged(int value)
	  {
		_session = SessionSelectionDropdown.options[value].text;
		SessionField.text = _session;
	  }

	  void GenerateDropdown(string[] folderlist, TMP_Dropdown dropDown)
	  {

		List<TMP_Dropdown.OptionData> tmpOptions = new List<TMP_Dropdown.OptionData>();

		for (int s = 0; s < folderlist.Length; s++)
		{
		    tmpOptions.Add(new TMP_Dropdown.OptionData(folderlist[s]));
		}

		dropDown.ClearOptions();
		dropDown.AddOptions(tmpOptions);
	  }


	  public void OnLoadButtonPressed()
	  {
		// start session
		Debug.Log("Load Jsons");
		LoadJsons();
	  }

	  public void OnStartButtonPressed()
	  {
		// start session
		Debug.Log("Start Session");
		//experimentGenerator.StartExperiment();
	  }

	  private void LoadJsons()
	  {

		string[] filelist = FileManager.GetAllFilenamesWithExtensionFrom("Configs/TaskDefinitions/", "json");

		foreach (string s in filelist)
		{
		    TaskDefinitionJsonStrings.Add(FileManager.ReadStringFromApplicationPath("Configs/TaskDefinitions/" + s));
		    Debug.Log("Load: " + TaskDefinitionJsonStrings[TaskDefinitionJsonStrings.Count - 1]);
		}

		string currentPath = "Configs/Participants/" + _subject + "/" + _session + "/";

		string EBsequenceFilePath = currentPath + "EBlockSequence.json";
		EBSequenceJsonString = FileManager.ReadStringFromApplicationPath(EBsequenceFilePath);
		Debug.Log("Load: " + EBSequenceJsonString);

		filelist = FileManager.GetAllFilenamesWithExtensionFrom(currentPath + "/EBlockDefinitions", "json");

		foreach (string s in filelist)
		{
		    EBlockDefinitionJsonStrings.Add(FileManager.ReadStringFromApplicationPath(currentPath + "/EBlockDefinitions/" + s));
		    Debug.Log("Load: " + EBlockDefinitionJsonStrings[EBlockDefinitionJsonStrings.Count - 1]);
		}

	  }
    }
}