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
			// Listen to even that XR rig is set up
			EventManager.StartListening("EvFoundXRrigReferences", OnEvFoundXRrigReferences);
			
			// Set up sequence listeners
			EventManager.StartListening("EvSessionBreak", 		OnEvSessionBreak);
			EventManager.StartListening("EvSessionResume", 		OnEvSessionResume);
			EventManager.StartListening("EvBlockIntroduction", 	OnEvBlockIntroduction);
			EventManager.StartListening("EvTrialBegin", 		OnEvTrialBegin);

			GetXRrigReferences();
		}

		public virtual void OnEnable() {

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

		// /// <summary>Called from UXF session. </summary>
		// void OnTrialEndUXF(Trial endedTrial) {
		// 	AddToLog("OnTrialEndUXF");
		// 	OnTrialEnd(endedTrial); // call TASK related method to be able to overwrite for presenting result or something
		// }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region eDIA EXPERIMENT EVENT HANDLERS

		/// <summary>Hook up to OnSessionStart event</summary>
		public virtual void OnSessionStart() {
			// Intentially empty
			//! System awaits 'EvProceed' event automaticcaly to proceed to first trial. 
		}

		/// <summary>Hook up to Experiment OnExperimentEnd event</summary>
		public virtual void OnSessionEnding() {
			// Intentially empty
			//! System awaits 'EvProceed' event automaticcaly to proceed to finalising session.
		}

		/// <summary>OnEvSessionBreak event listener</summary>
		void OnEvSessionBreak(eParam e) {
			AddToLog("Break START");
			OnSessionBreak();
		}

		/// <summary> Overridable OnEvSessionBreak</summary>
		public virtual void OnSessionBreak() {
			// Intentially empty
		}

		/// <summary>OnEvSessionResume event listener</summary>
		void OnEvSessionResume (eParam e) {
			AddToLog("Session Resume");
			OnSessionResume();
		}

		/// <summary> Overridable OnSessionResume method</summary>
		public virtual void OnSessionResume() {
			// Intentially empty
		}

		/// <summary>OnEvBlockIntroduction event listener</summary>
		void OnEvBlockIntroduction (eParam e) {
			AddToLog("Block introduction START");
			OnBlockIntroduction();
		}

		/// <summary>Overridable OnBlockIntroduction</summary>
		public virtual void OnBlockIntroduction() {
			// Intentially empty
		}

		void OnEvTrialBegin (eParam e) {
			AddToLog("Trial Begin:" + Session.instance.currentTrialNum);
			StartTrial();
		}

		/// <summary>Overridable OnBlockContinue</summary>
		public virtual void OnBlockResume () {
			// Intentially empty
		}

		public virtual void OnEvProceed (eParam e) {
			EventManager.StopListening("EvProceed", OnEvProceed);
			Debug.Log("OnEvProceed");
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
		// [HideInInspector]
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

		/// <summary>Called from this UXF method. </summary>
		// public virtual void OnTrialEnd(Trial endedTrial) {
		// 	AddToLog("OnTrialEnd");
		// }

		public virtual void ResetTrial() {
			currentStep = -1;
		}

		public void NextStep() {
			if (!inSession)
				return;

			currentStep++;

			if (currentStep < trialSequence.Count) {
				Debug.Log("currentstep:" + currentStep);
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