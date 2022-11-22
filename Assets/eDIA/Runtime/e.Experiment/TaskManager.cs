using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using System; // needed for <action> 
using System.Linq;
using UnityEngine.Events;


namespace eDIA {

	public class TaskManager : Singleton<TaskManager> {

#region DECLARATIONS

		public bool showLog = false;
		public Color taskColor = Color.yellow;

		[System.Serializable]
		public class TaskSettingsContainer { 
			public List<ExperimentManager.SettingsTuple> taskSettings = new List<ExperimentManager.SettingsTuple>();
		}
		
		TaskSettingsContainer taskSettingsContainer;
		public UXF.Settings taskSettings = new Settings();


		// Events forwarding
		[Header("Event hooks")]
		public UnityEvent OnOnEnable = null;
		public UnityEvent OnStart = null;



		// XR RIG
		[HideInInspector] public Transform XRrig_MainCamera 		= null;
		[HideInInspector] public Transform XRrig_RightController	= null;
		[HideInInspector] public Transform XRrig_LeftController 	= null;

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region Mono methods

		public virtual void Awake() {
			// Listen to event that XR rig is ready
			EventManager.StartListening(eDIA.Events.Interaction.EvFoundXRrigReferences, 	OnEvFoundXRrigReferences);
			EventManager.StartListening(eDIA.Events.Core.EvExperimentInitialised, 		OnEvExperimentInitialised);
			EventManager.StartListening(eDIA.Events.Core.EvLocalConfigSubmitted, 		OnEvLocalConfigSubmitted);
			EventManager.StartListening(eDIA.Events.Core.EvSetTaskConfig, 			OnEvSetTaskConfig);

			GetXRrigReferences();
		}


		public virtual void OnEnable() {
		}

		public virtual void Start() {
		}


		void OnDestroy() {
			EventManager.StopListening("EvFoundXRrigReferences", OnEvFoundXRrigReferences);
			
			// Set up sequence listeners
			EventManager.StopListening(eDIA.Events.Core.EvSessionBreak, 		OnEvSessionBreak);
			EventManager.StopListening(eDIA.Events.Core.EvSessionResume, 		OnEvSessionResume);
			EventManager.StopListening(eDIA.Events.Core.EvBlockIntroduction, 		OnEvBlockIntroduction);
			EventManager.StopListening(eDIA.Events.Core.EvBlockResume, 			OnEvBlockResume);
			EventManager.StopListening(eDIA.Events.Core.EvTrialBegin, 			OnEvTrialBegin);
			EventManager.StopListening(eDIA.Events.Core.EvExperimentInitialised, 	OnEvExperimentInitialised);
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EDIA XR RIG 

		void GetXRrigReferences () {
			eDIA.XRrigUtilities.GetXRrigReferencesAsync();
		}

		public virtual void OnEvFoundXRrigReferences (eParam e)  {
			EventManager.StopListening(eDIA.Events.Interaction.EvFoundXRrigReferences, OnEvFoundXRrigReferences);
			AddToLog("XRrig references FOUND");

			XRrig_MainCamera 		= XRrigManager.instance.XRrig_MainCamera;
			XRrig_RightController	= eDIA.XRrigUtilities.GetXRcontrollerRight();
			XRrig_LeftController	= eDIA.XRrigUtilities.GetXRcontrollerLeft();

			SetXRrigTracking ();
		}

		void SetXRrigTracking () {
			Session.instance.trackedObjects.Add(XRrig_MainCamera.GetComponent<Tracker>());
			Session.instance.trackedObjects.Add(XRrig_RightController.GetComponent<Tracker>());
			Session.instance.trackedObjects.Add(XRrig_LeftController.GetComponent<Tracker>());

			AddToLog("XRrig added to UXF tracking");
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK UXF HELPERS

		private void LoadTaskConfigFromFile(string filename)
		{
			// Load taskConfigFile
			string taskConfigJSON = FileManager.ReadStringFromApplicationPathSubfolder(Constants.localConfigDirectoryName + "/Tasks", filename);

			if (taskConfigJSON == "ERROR")
			{
				Debug.LogError("Task JSON not correctly loaded!");
				return;
			}

			SetTaskConfigFromJSON(taskConfigJSON);
		}

		private void SetTaskConfigFromJSON (string taskConfigJSON)
		{
			// Parse JSON into a container
			taskSettingsContainer = UnityEngine.JsonUtility.FromJson<TaskSettingsContainer>(taskConfigJSON);

			// Workaround to parse into a UXF Dictionary 
			foreach (ExperimentManager.SettingsTuple tuple in taskSettingsContainer.taskSettings)
			{

				if (tuple.value.Contains(','))
				{ // it's a list!
					List<string> stringlist = tuple.value.Split(',').ToList();
					taskSettings.SetValue(tuple.key, stringlist);
				}
				else taskSettings.SetValue(tuple.key, tuple.value);   // normal string
			}
		}

		/// <summary>Event catcher for taskconfig</summary>
		void OnEvSetTaskConfig(eParam obj)
		{
			SetTaskConfigFromJSON(obj.GetString());
		}

		public void AddToTrialResults (string key, string value) {
			// TODO: Add option to add result easily to results dict and therefor on disk
			Session.instance.CurrentTrial.result[key] = value;
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region UXF EVENT HANDLERS

		void OnSessionBeginUXF() {
			AddToLog("OnSessionBeginUXF");
			EventManager.StartListening(eDIA.Events.Core.EvTrialBegin, 		OnEvTrialBegin);
			EventManager.StartListening(eDIA.Events.Core.EvSessionBreak, 	OnEvSessionBreak);
			EventManager.StartListening(eDIA.Events.Core.EvBlockIntroduction, OnEvBlockIntroduction);
			EventManager.StartListening(eDIA.Events.Core.EvBlockStart, 		OnEvBlockStart);

			inSession = true;
			OnSessionStart();
		}

		// /// <summary>Called from UXF session. </summary>
		// void OnPreSessionEndUXF() {
		// 	AddToLog("OnPreSessionEndUXF");
		// 	OnPreSessionEnd(); // call our own session ending
		// }

		/// <summary>Called from UXF session. </summary>
		public virtual void OnSessionEndUXF() {
			AddToLog("OnSessionEndUXF");
			inSession = false;
			EventManager.StopListening(eDIA.Events.Core.EvBlockStart, 		OnEvBlockStart);
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region eDIA EXPERIMENT EVENT HANDLERS

		/// <summary>Look up given index in the localConfigFiles list and give content of that file to system </summary>
		/// <param name="e">String = filename of the configfile</param>
		void OnEvLocalConfigSubmitted (eParam e) {
			EventManager.StopListening(eDIA.Events.Core.EvLocalConfigSubmitted, OnEvLocalConfigSubmitted);

			string filename = e.GetStrings()[0] + ".json"; // combine task string and participant string
			
			// Debug.Log(e.GetString());
			LoadTaskConfigFromFile (filename);
		}
		
		void OnEvExperimentInitialised (eParam e) {
			OnExperimentInitialised(e.GetBool());
		}

		/// <summary>At this point we have acces to all taskSettings</summary>
		public virtual void OnExperimentInitialised(bool result) {
			// Intentially empty
		}

		void OnEvTrialBegin (eParam e) {
			StartTrial();
		}

		void OnEvBlockStart (eParam obj) {
			// New block, got from experimentmanager
			// Call overridable local Method
			OnBlockStart();
		}

		public virtual void OnBlockStart () {
			// Intentially empty

		}

		public virtual void OnEvProceed (eParam e) {
			EventManager.StopListening(eDIA.Events.Core.EvProceed, OnEvProceed);
			NextStep();
		}



#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region SESSION START / END

		/// <summary>Hook up to OnSessionStart event</summary>
		public virtual void OnSessionStart() {
			// Intentially empty
			//! System awaits 'EvProceed' event automaticaly to proceed to first trial. 
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BREAK

		/// <summary>OnEvSessionBreak event listener</summary>
		void OnEvSessionBreak(eParam e) {
			AddToLog("Break START");
			EventManager.StartListening(eDIA.Events.Core.EvSessionResume, 		OnEvSessionResume);
			
			OnSessionBreak();
		}

		/// <summary> Overridable OnEvSessionBreak</summary>
		public virtual void OnSessionBreak() {
			// Intentially empty
		}

		/// <summary>OnEvSessionResume event listener</summary>
		void OnEvSessionResume (eParam e) {
			AddToLog("Session Resume");
			EventManager.StopListening(eDIA.Events.Core.EvSessionResume, 		OnEvSessionResume);
			OnSessionResume();
		}

		/// <summary> Overridable OnSessionResume method</summary>
		public virtual void OnSessionResume() {
			// Intentially empty
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BLOCK INTRODUCTION

		/// <summary>OnEvBlockIntroduction event listener</summary>
		void OnEvBlockIntroduction (eParam e) {
			AddToLog("Block introduction");
			EventManager.StartListening(eDIA.Events.Core.EvBlockResume, 		OnEvBlockResume);
			OnBlockIntroduction();
		}
		
		/// <summary>Overridable OnBlockIntroduction</summary>
		public virtual void OnBlockIntroduction() {
			// Intentially empty
		}

		/// <summary>OnEvBlockResume event listener</summary>
		public void OnEvBlockResume (eParam e) {
			AddToLog("Block Resume");
			EventManager.StopListening(eDIA.Events.Core.EvBlockResume, 		OnEvBlockResume);
			OnBlockResume();
			StartTrial();
		}

		/// <summary>Overridable OnBlockContinue</summary>
		public virtual void OnBlockResume () {
			// Intentially empty
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK STATEMACHINE
		//? Methods controlling the current trial

		[SerializeField]//[HideInInspector]
		public List<TrialStep> trialSequence = new List<TrialStep>();
		[HideInInspector]
		public int currentStep = -1;

		private bool inSession = false;


		void StartTrial() {

			AddToLog("StartTrial");
			ResetTrial();

			// Fire up the task state machine to run the steps of the trial.
			NextStep();
		}

		/// <summary>Called after the task sequence is done </summary>
		void EndTrial() {
			AddToLog("Trial Steps DONE");
			Session.instance.EndCurrentTrial(); // tells UXF to end this trial and fire the event that follows
		}

		public virtual void ResetTrial() {
			currentStep = -1;
		}

		/// <summary>Call next step in the trial with delay.</summary>
		/// <param name="delay">Time to wait before proceeding. Expects float</param>
		public void NextStep (float delay) {
			Invoke("NextStep", delay);
		}

		public void NextStep() {
			if (!inSession)
				return;

			currentStep++;

			if (currentStep < trialSequence.Count) {
				trialSequence[currentStep].methodToCall.Invoke();
			}
			else EndTrial();
		}

#endregion	// -------------------------------------------------------------------------------------------------------------------------------
#region MISC	

		public void AddToLog(string _msg) {
			if (showLog)
				eDIA.LogUtilities.AddToLog(_msg, "TASK", taskColor);
		}
		

#endregion	// -------------------------------------------------------------------------------------------------------------------------------
		
	}
}