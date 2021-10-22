using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using System; // needed for <action> 
// using eDIA;

namespace eDIA {

	public class TaskManagerBase : MonoBehaviour {

#region DECLARATIONS

		public bool showLog = false;
		public Color taskColor = Color.yellow;

		// XR RIG
		[HideInInspector] public Transform XRrig_MainCamera 		= null;
		[HideInInspector] public Transform XRrig_RightController	= null;
		[HideInInspector] public Transform XRrig_LeftController 	= null;

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region Mono methods

		public virtual void Awake() {
			EventManager.StartListening("EvFoundXRrigReferences", OnEvFoundXRrigReferences);

			GetXRrigReferences();
		}

		public virtual void OnEnable() {
			// Listen to even that XR rig is set up
		}

		public virtual void Start() {
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EDIA XR RIG 

		void GetXRrigReferences () {

			eDIA.XRrigUtilities.GetXRrigReferencesAsync();
		}

		public virtual void OnEvFoundXRrigReferences (eParam e)  {
			AddToLog("XRrig references FOUND");

			XRrig_MainCamera 		= eDIA.XRrigUtilities.GetXRcam();
			XRrig_RightController	= eDIA.XRrigUtilities.GetXRcontrollerRight();
			XRrig_LeftController	= eDIA.XRrigUtilities.GetXRcontrollerLeft();

			SetXRrigTracking ();
			
			// remove listener to xr rig setup
			EventManager.StopListening("EvFoundXRrigReferences", OnEvFoundXRrigReferences);
		}

		void SetXRrigTracking () {
			Session.instance.trackedObjects.Add(XRrig_MainCamera.GetComponent<Tracker>());
			Session.instance.trackedObjects.Add(XRrig_RightController.GetComponent<Tracker>());
			Session.instance.trackedObjects.Add(XRrig_LeftController.GetComponent<Tracker>());

			AddToLog("XRrig added to UXF tracking");
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK UXF HELPERS

		public void AddToTrialResults (string key, string value) {
			// TODO: Add option to add result easily to results dict and therefor on disk
			Session.instance.CurrentTrial.result[key] = value;
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region UXF EVENT HANDLERS

		void OnSessionBeginUXF() {
			AddToLog("OnSessionBeginUXF");
			inSession = true;
			OnSessionStart();
		}

		/// <summary>Called from UXF session. </summary>
		void OnPreSessionEndUXF() {
			AddToLog("OnPreSessionEndUXF");
			OnSessionEnding(); // call our own session ending
		}

		/// <summary>Called from UXF session. </summary>
		public virtual void OnSessionEndUXF() {
			AddToLog("OnSessionEndUXF");
			inSession = false;
		}

		/// <summary>Called from UXF session. Begin setting things up for the trial that is about to start </summary>
		public void OnTrialBegin(Trial trial) {
			AddToLog("OnTrialBegin");
			StartTrial(trial);
		}

		/// <summary>Called from UXF session. </summary>
		void OnTrialEndUXF(Trial endedTrial) {
			AddToLog("OnTrialEndUXF");
			OnTrialEnd(endedTrial); // call TASK related method to be able to overwrite for presenting result or something
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region eDIA EXPERIMENT EVENT HANDLERS

		/// <summary>Hook up to OnSessionStart event</summary>
		public virtual void OnSessionStart() {
			AddToLog("SessionStart: Show Intro");
			//! System awaits 'EvProceed' event automaticcaly to proceed to first trial. 
		}

		/// <summary>Hook up to Experiment OnExperimentEnd event</summary>
		public virtual void OnSessionEnding() {
			AddToLog("SessionEnd: Show Outro");
			//! System awaits 'EvProceed' event automaticcaly to proceed to finalising session.
		}

		/// <summary>Hook up to Experiment OnSessionBreak event</summary>
		public virtual void OnSessionBreak() {
			AddToLog("Break START");
		}

		/// <summary>Hook up to Experiment OnSessionResume event</summary>
		public virtual void OnSessionResume () {
			AddToLog("Break END");
		}

		/// <summary>Hook up to Experiment OnSessionBreak event</summary>
		public virtual void OnBlockIntroduction() {
			AddToLog("Block introduction START");
		}

		/// <summary>Hook up to Experiment OnSessionResume event</summary>
		public virtual void OnBlockContinue (Trial trial) {
			AddToLog("Block Continue");
			OnTrialBegin(trial);
		}

		public virtual void OnEvProceed (eParam e) {
			EventManager.StopListening("EvProceed", OnEvProceed);
			NextStep();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK STATEMACHINE CALLS
		//? Methods controlling the current trial

		[System.Serializable]
		public class TrialStep {
			public string title;
			public Action methodToCall;

			public TrialStep(string title, Action methodToCall) {
				this.title = title;
				this.methodToCall = methodToCall;
			}
		}

		[SerializeField][HideInInspector]
		public List<TrialStep> trialSequence = new List<TrialStep>();
		[HideInInspector]
		public int currentStep = -1;

		private bool inSession = false;

		/// <summary>Called from incoming UXF event</summary>
		void StartTrial(Trial trial) {
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

		/// <summary>04 > Presenting results of ended trial. Called from this UXF method. </summary>
		public virtual void OnTrialEnd(Trial endedTrial) {
			AddToLog("OnTrialEnd");
		}

		public virtual void ResetTrial() {
			currentStep = -1;
		}

		public void NextStep() {
			if (!inSession)
				return;

			currentStep++;

			if (currentStep < trialSequence.Count)
				trialSequence[currentStep].methodToCall.Invoke();
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