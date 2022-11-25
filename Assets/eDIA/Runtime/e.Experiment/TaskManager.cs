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

		[Space(20)]
		public List<TaskBlock> taskBlocks = new List<TaskBlock>();

		[Space(20)]
		[Header("Event hooks\n\nOptional event hooks to use in your task")]
		public UnityEvent OnSessionStart = null;
		public UnityEvent OnSessionBreak = null;
		public UnityEvent OnSessionEnd = null;

		
		[HideInInspector]
		public int currentStep = -1;

		private bool inSession = false;

		Coroutine timer = null;

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

			// Disable all task blocks at start
			foreach (TaskBlock t in taskBlocks)
				t.gameObject.SetActive(false);
		}


		void OnDestroy() {
			EventManager.StopListening("EvFoundXRrigReferences", OnEvFoundXRrigReferences);
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
#region eDIA EVENT HANDLERS

		/// <summary>Look up given index in the localConfigFiles list and give content of that file to system </summary>
		/// <param name="e">String = filename of the configfile</param>
		void OnEvLocalConfigSubmitted (eParam e) {

			EventManager.StopListening(eDIA.Events.Core.EvLocalConfigSubmitted, OnEvLocalConfigSubmitted);
			string filename = e.GetStrings()[0] + ".json"; // combine task string and participant string
		}
		

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region eDIA EXPERIMENT MANAGER CALLS

		public void SessionBeginUXF() {
			AddToLog("OnSessionBeginUXF");
			inSession = true;
			OnSessionStart?.Invoke();
		}

		/// <summary>Called from UXF session. </summary>
		public void SessionEndUXF() {
			AddToLog("OnSessionEndUXF");
			taskBlocks[Session.instance.currentBlockNum-2].gameObject.SetActive(false); 
			inSession = false;
			OnSessionEnd?.Invoke();
		}
		
		public void TrialBegin () {
			StartTrial();
		}

		public void OnEvProceed (eParam e) {

			Debug.Log("<color=#50eee0>["+ name +  "]]> OnEvProceed called</color>");
			EventManager.StopListening(eDIA.Events.Core.EvProceed, OnEvProceed); // stop listening to avoid doubleclicks
			EventManager.TriggerEvent("EvButtonChangeState", new eParam( new string[] { "PROCEED", "false" })); // disable button, as OnEvProceed might have come from somewhere else than the button itself
			NextStep();
		}

		/// <summary>Called from experiment manager</summary>
		public void SessionBreak() {
			AddToLog("Break START");
			OnSessionBreak?.Invoke();
		}

		/// <summary>Called from experiment manager</summary>
		public void BlockStart () {
			AddToLog("Block Start");

			// Disable old block
			if (Session.instance.currentBlockNum-1 != 0)
				taskBlocks[Session.instance.currentBlockNum-2].gameObject.SetActive(false);

			// enable new block
			taskBlocks[Session.instance.currentBlockNum-1].gameObject.SetActive(true);
			taskBlocks[Session.instance.currentBlockNum-1].OnBlockStart();
		}
		
		/// <summary>Called from experiment manager</summary>
		public void BlockEnd () {
			AddToLog("Block End");
			taskBlocks[Session.instance.currentBlockNum-1].OnBlockEnd();
		}

		/// <summary>Called from experiment manager</summary>
		public void BlockIntroduction () {
			AddToLog("Block introduction");
			taskBlocks[Session.instance.currentBlockNum-1].OnBlockIntroduction();	
		}

		/// <summary>Called from experiment manager</summary>
		public void BlockResumeAfterIntro () {
			AddToLog("OnEvBlockResumeAfterIntro");
			taskBlocks[Session.instance.currentBlockNum-1].OnBlockResumeAfterIntro();	

			StartTrial();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK STATEMACHINE
		//? Methods controlling the current trial

		void StartTrial() {

			AddToLog("StartTrial");
			StartNewTrial();

			// Fire up the task state machine to run the steps of the trial.
			NextStep();
		}

		/// <summary>Called after the task sequence is done </summary>
		void EndTrial() {
			AddToLog("Trial Steps DONE");
			Session.instance.EndCurrentTrial(); // tells UXF to end this trial and fire the event that follows
		}

		public void StartNewTrial() {
			currentStep = -1;
			taskBlocks[Session.instance.currentBlockNum-1].OnStartNewTrial();
		}

		/// <summary>Call next step in the trial with delay.</summary>
		/// <param name="delay">Time to wait before proceeding. Expects float</param>
		public void NextStep (float delay) {
			if (timer != null) StopCoroutine(timer); // Kill timer, if any

			timer = StartCoroutine("NextStepTimer", delay);
		}

		/// <summary>Coroutine as timer as we can kill that to avoid delayed calls in the statemachine</summary>
		IEnumerator NextStepTimer (float delay) {
			
			yield return new WaitForSeconds(delay);
			NextStep();
		}

		/// <summary>Call next step in the trial.</summary>
		public void NextStep() {
			if (timer != null) StopCoroutine(timer); // Kill timer, if any

			if (!inSession)
				return;

			currentStep++;

			if (currentStep < taskBlocks[Session.instance.CurrentBlock.number-1].trialSteps.Count) {
				taskBlocks[Session.instance.currentBlockNum-1].OnBetweenSteps();
				taskBlocks[Session.instance.CurrentBlock.number-1].trialSteps[currentStep].Invoke();
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