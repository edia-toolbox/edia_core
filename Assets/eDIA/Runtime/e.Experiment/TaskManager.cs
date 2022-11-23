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
		public class TaskBlock {

			[Header("Short description of this block")]
			public string BlockDescription;

			[SerializeField]
			[Header("Link the methods the trial goes through sequentially for this block.")]
			public List<TrialSequenceStep> trialSequence = new List<TrialSequenceStep>();

			[Header("Event hooks")]
			public UnityEvent OnSessionStart = null;
			public UnityEvent OnSessionBreak = null;
			public UnityEvent OnSessionResume = null;
			public UnityEvent OnBlockStart = null;
			public UnityEvent OnBlockIntroduction = null;
			public UnityEvent OnBlockResumeAfterIntro = null;
			public UnityEvent OnResetTrial = null;
			public UnityEvent OnStart = null;
		}

		public List<TaskBlock> taskBlocks = new List<TaskBlock>();

		[HideInInspector]
		public int currentStep = -1;

		private bool inSession = false;

		// XR RIG
		[HideInInspector] public Transform XRrig_MainCamera 		= null;
		[HideInInspector] public Transform XRrig_RightController	= null;
		[HideInInspector] public Transform XRrig_LeftController 	= null;

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region Mono methods

		public virtual void Awake() {
			// Listen to event that XR rig is ready
			EventManager.StartListening(eDIA.Events.Interaction.EvFoundXRrigReferences, 	OnEvFoundXRrigReferences);
			EventManager.StartListening(eDIA.Events.Core.EvLocalConfigSubmitted, 		OnEvLocalConfigSubmitted);

			GetXRrigReferences();
		}


		void OnDestroy() {
			EventManager.StopListening("EvFoundXRrigReferences", OnEvFoundXRrigReferences);
			
			// Set up sequence listeners
			EventManager.StopListening(eDIA.Events.Core.EvSessionBreak, 		OnEvSessionBreak);
			EventManager.StopListening(eDIA.Events.Core.EvSessionResume, 		OnEvSessionResume);
			EventManager.StopListening(eDIA.Events.Core.EvBlockIntroduction, 		OnEvBlockIntroduction);
			EventManager.StopListening(eDIA.Events.Core.OnEvBlockResumeAfterIntro, 	OnEvBlockResumeAfterIntro);
			EventManager.StopListening(eDIA.Events.Core.EvTrialBegin, 			OnEvTrialBegin);
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

		public void AddToTrialResults (string key, string value) {
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
			taskBlocks[Session.instance.currentBlockNum].OnSessionStart?.Invoke();
		}


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
		}
		
		void OnEvTrialBegin (eParam e) {
			StartTrial();
		}

//! IS this the start of calling a block? maybe for an INIT method or someething
		void OnEvBlockStart (eParam obj) {
			// New block, got from experimentmanager
			taskBlocks[Session.instance.currentBlockNum].OnBlockStart?.Invoke();
		}

		public virtual void OnEvProceed (eParam e) {
			EventManager.StopListening(eDIA.Events.Core.EvProceed, OnEvProceed);
			NextStep();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BREAK

		/// <summary>OnEvSessionBreak event listener</summary>
		void OnEvSessionBreak(eParam e) {
			AddToLog("Break START");

			EventManager.StartListening(eDIA.Events.Core.EvSessionResume, OnEvSessionResume);
			taskBlocks[Session.instance.currentBlockNum].OnSessionBreak?.Invoke();
		}

		/// <summary>OnEvSessionResume event listener</summary>
		void OnEvSessionResume (eParam e) {
			AddToLog("Session Resume");

			EventManager.StopListening(eDIA.Events.Core.EvSessionResume, OnEvSessionResume);
			taskBlocks[Session.instance.currentBlockNum].OnSessionResume?.Invoke();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BLOCK INTRODUCTION

		/// <summary>OnEvBlockIntroduction event listener</summary>
		void OnEvBlockIntroduction (eParam e) {
			AddToLog("Block introduction");

			EventManager.StartListening(eDIA.Events.Core.OnEvBlockResumeAfterIntro, OnEvBlockResumeAfterIntro);
			taskBlocks[Session.instance.currentBlockNum].OnBlockIntroduction?.Invoke();
		}
		

		/// <summary>OnEvBlockResume event listener</summary>
		public void OnEvBlockResumeAfterIntro (eParam e) {
			AddToLog("Block Resume");

			EventManager.StopListening(eDIA.Events.Core.OnEvBlockResumeAfterIntro, OnEvBlockResumeAfterIntro);
			taskBlocks[Session.instance.currentBlockNum].OnBlockResumeAfterIntro?.Invoke();

			StartTrial();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK STATEMACHINE
		//? Methods controlling the current trial

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

		public void ResetTrial() {

			currentStep = -1;
			taskBlocks[Session.instance.currentBlockNum].OnResetTrial?.Invoke();
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

			if (currentStep < taskBlocks[0].trialSequence.Count) {
				taskBlocks[Session.instance.currentBlockNum].trialSequence[currentStep].methodToCall.Invoke();
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