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

		// [System.Serializable]
		// public class TaskBlock {

		// 	[Header("Short description of this block")]
		// 	[Tooltip("Stick with using the 'name' from the config file for this block")]
		// 	public string BlockDescription;

		// 	[SerializeField]
		// 	[Header("Link the methods the trial goes through sequentially for this block.")]
		// 	public List<TrialSequenceStep> trialSequence = new List<TrialSequenceStep>();

		// }

		[Space(20)]
		public List<TaskBlock> taskBlocks = new List<TaskBlock>();

		[Space(20)]
		[Header("Event hooks\nOptional event hooks to use in your task block")]
		public UnityEvent OnSessionStart = null;
		public UnityEvent OnSessionBreak = null;
		public UnityEvent OnSessionResume = null;
		public UnityEvent OnBlockStart = null;
		public UnityEvent OnBlockEnd = null;
		public UnityEvent OnBlockIntroduction = null;
		public UnityEvent OnBlockResumeAfterIntro = null;
		public UnityEvent OnStartNewTrial = null;
		public UnityEvent OnBetweenSteps = null;
		
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
		}

		/// <summary>Called from UXF session. </summary>
		public void SessionEndUXF() {
			AddToLog("OnSessionEndUXF");
			inSession = false;
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
			taskBlocks[Session.instance.currentBlockNum-1].OnBlockStart();
		}
		
		/// <summary>Called from experiment manager</summary>
		public void BlockEnd () {
			AddToLog("Block End");
			OnBlockEnd?.Invoke();
		}

		/// <summary>Called from experiment manager</summary>
		public void BlockIntroduction () {
			AddToLog("Block introduction");

			OnBlockIntroduction?.Invoke();
		}

		/// <summary>Called from experiment manager</summary>
		public void BlockResumeAfterIntro () {
			AddToLog("OnEvBlockResumeAfterIntro");

			OnBlockResumeAfterIntro?.Invoke();

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
			OnStartNewTrial?.Invoke();
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
				OnBetweenSteps.Invoke();
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