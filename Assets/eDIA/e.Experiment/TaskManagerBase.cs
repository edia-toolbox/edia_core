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

			GetXRrigReferences();
		}

		public virtual void OnEnable() {

		}

		public virtual void Start() {
			
		}

		void OnDestroy() {
			EventManager.StopListening("EvFoundXRrigReferences", OnEvFoundXRrigReferences);
			
			// Set up sequence listeners
			EventManager.StopListening("EvSessionBreak", 		OnEvSessionBreak);
			EventManager.StopListening("EvSessionResume", 		OnEvSessionResume);
			EventManager.StopListening("EvBlockIntroduction", 	OnEvBlockIntroduction);
			EventManager.StopListening("EvBlockResume", 		OnEvBlockResume);
			EventManager.StopListening("EvTrialBegin", 		OnEvTrialBegin);
	
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
			EventManager.StartListening("EvTrialBegin", 		OnEvTrialBegin);
			EventManager.StartListening("EvSessionBreak", 		OnEvSessionBreak);
			EventManager.StartListening("EvBlockIntroduction", 	OnEvBlockIntroduction);

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

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region eDIA EXPERIMENT EVENT HANDLERS

		/// <summary>Hook up to OnSessionStart event</summary>
		public virtual void OnSessionStart() {

			//! System awaits 'EvProceed' event automaticaly to proceed to first trial. 
		}

		/// <summary>Hook up to Experiment OnExperimentEnd event</summary>
		public virtual void OnSessionEnding() {
			// Intentially empty
			//! System awaits 'EvProceed' event automaticaly to proceed to finalising session.
		}

		/// <summary>OnEvSessionBreak event listener</summary>
		void OnEvSessionBreak(eParam e) {
			AddToLog("Break START");
			EventManager.StartListening("EvSessionResume", 		OnEvSessionResume);
			OnSessionBreak();
		}

		/// <summary> Overridable OnEvSessionBreak</summary>
		public virtual void OnSessionBreak() {
			// Intentially empty
		}

		/// <summary>OnEvSessionResume event listener</summary>
		void OnEvSessionResume (eParam e) {
			AddToLog("Session Resume");
			EventManager.StopListening("EvSessionResume", 		OnEvSessionResume);
			OnSessionResume();
		}

		/// <summary> Overridable OnSessionResume method</summary>
		public virtual void OnSessionResume() {
			// Intentially empty
		}

		/// <summary>OnEvBlockIntroduction event listener</summary>
		void OnEvBlockIntroduction (eParam e) {
			AddToLog("Block introduction");
			EventManager.StartListening("EvBlockResume", 		OnEvBlockResume);
			OnBlockIntroduction();
		}
		
		/// <summary>Overridable OnBlockIntroduction</summary>
		public virtual void OnBlockIntroduction() {
			// Intentially empty
		}

		/// <summary>OnEvBlockResume event listener</summary>
		public void OnEvBlockResume (eParam e) {
			AddToLog("Block Resume");
			EventManager.StopListening("EvBlockResume", 		OnEvBlockResume);
			OnBlockResume();
			StartTrial();
		}

		/// <summary>Overridable OnBlockContinue</summary>
		public virtual void OnBlockResume () {
			// Intentially empty
		}
		
		void OnEvTrialBegin (eParam e) {
			StartTrial();
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