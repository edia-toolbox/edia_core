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
			[Tooltip("Stick with using the 'name' from the config file for this block")]
			public string BlockDescription;

			[SerializeField]
			[Header("Link the methods the trial goes through sequentially for this block.")]
			public List<TrialSequenceStep> trialSequence = new List<TrialSequenceStep>();

			[Space(20)]
			[Header("Event hooks\nOptional event hooks to use in your task block")]
			public UnityEvent OnSessionStart = null;
			public UnityEvent OnSessionBreak = null;
			public UnityEvent OnSessionResume = null;
			public UnityEvent OnBlockStart = null;
			public UnityEvent OnBlockIntroduction = null;
			public UnityEvent OnBlockResumeAfterIntro = null;
			public UnityEvent OnStartNewTrial = null;
			public UnityEvent OnBetweenSteps = null;
		}

		[Space(20)]
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
			EventManager.StopListening(eDIA.Events.Core.EvBlockResumeAfterIntro, 	OnEvBlockResumeAfterIntro);
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
			taskBlocks[0].OnSessionStart?.Invoke(); // is always 0 as session starts only once
			//! TODO This would probably won't work at the moment we introduce multiple tasks in one session though
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

		void OnEvBlockStart (eParam obj) {
			// New block, got from experimentmanager
			OnBlockStart();
		}

		public void OnEvProceed (eParam e) {
			AddToLog("EvProceed listener OFF");
			EventManager.StopListening(eDIA.Events.Core.EvProceed, OnEvProceed);
			NextStep();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BREAK

		/// <summary>OnEvSessionBreak event listener</summary>
		void OnEvSessionBreak(eParam e) {
			AddToLog("Break START");

			EventManager.StartListening(eDIA.Events.Core.EvSessionResume, OnEvSessionResume);
			taskBlocks[Session.instance.CurrentBlock.number-1].OnSessionBreak?.Invoke();
		}

		/// <summary>OnEvSessionResume event listener</summary>
		void OnEvSessionResume (eParam e) {
			AddToLog("Session Resume");

			EventManager.StopListening(eDIA.Events.Core.EvSessionResume, OnEvSessionResume);
			taskBlocks[Session.instance.CurrentBlock.number-1].OnSessionResume?.Invoke();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BLOCK 

		/// <summary>OnEvBlockIntroduction event listener</summary>
		public void OnBlockStart () {
			AddToLog("Block Start");
			taskBlocks[Session.instance.CurrentBlock.number-1].OnBlockStart?.Invoke();
		}
		

		/// <summary>OnEvBlockIntroduction event listener</summary>
		void OnEvBlockIntroduction (eParam e) {
			AddToLog("Block introduction");

			EventManager.StartListening(eDIA.Events.Core.EvBlockResumeAfterIntro, OnEvBlockResumeAfterIntro);
			taskBlocks[Session.instance.CurrentBlock.number-1].OnBlockIntroduction?.Invoke();
		}
		

		/// <summary>OnEvBlockResume event listener</summary>
		void OnEvBlockResumeAfterIntro (eParam e) {
			AddToLog("Block Resume");

			EventManager.StopListening(eDIA.Events.Core.EvBlockResumeAfterIntro, OnEvBlockResumeAfterIntro);
			taskBlocks[Session.instance.CurrentBlock.number-1].OnBlockResumeAfterIntro?.Invoke();

			StartTrial();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK STATEMACHINE
		//? Methods controlling the current trial

		void StartTrial() {

			AddToLog("StartTrial");
			OnStartNewTrial();

			// Fire up the task state machine to run the steps of the trial.
			NextStep();
		}

		/// <summary>Called after the task sequence is done </summary>
		void EndTrial() {
			AddToLog("Trial Steps DONE");
			Session.instance.EndCurrentTrial(); // tells UXF to end this trial and fire the event that follows
		}

		public void OnStartNewTrial() {
			currentStep = -1;
			taskBlocks[Session.instance.CurrentBlock.number-1].OnStartNewTrial?.Invoke();
		}

		/// <summary>Call next step in the trial with delay.</summary>
		/// <param name="delay">Time to wait before proceeding. Expects float</param>
		public void NextStep (float delay) {
			Invoke("NextStep", delay);
		}

		/// <summary>Call next step in the trial.</summary>
		public void NextStep() {
			if (!inSession)
				return;

			currentStep++;

			if (currentStep < taskBlocks[Session.instance.CurrentBlock.number-1].trialSequence.Count) {
				taskBlocks[Session.instance.CurrentBlock.number-1].OnBetweenSteps.Invoke();
				taskBlocks[Session.instance.CurrentBlock.number-1].trialSequence[currentStep].methodToCall.Invoke();
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