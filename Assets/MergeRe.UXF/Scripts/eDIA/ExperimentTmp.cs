using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UXF;
using eDIA;
using Utils;

//! debug
using UnityEngine.UI;
using TMPro;

	/// <summary>
	/// Controls the start block gameobject
	/// </summary>
	public class ExperimentTmp : MonoBehaviour
	{
		static Experiment _instance;

		public static Experiment Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = GameObject.FindObjectOfType<Experiment>();
				}

				return _instance;
			}
		}

		public enum EState
		{
			IDLE,
			RUNNING,
			WAITINGONPROCEED,
			PAUSED,
			ENDED
		}

		public EState state = EState.IDLE;

		//! debug
		EState prevState = EState.IDLE;
		public TextMeshProUGUI infofield;

		// 
		public Session session;
		public SessionGenerator experimentGenerator;
		public List<EBlock> Blocks;

		int currentStep = -1;
		int storedBlockNum = 0;
		bool isPauseRequested = false;
		EBlock activeTaskBlock;

		void Awake()
		{
			EventManager.showLog = true;

			infofield.text = state.ToString();

			if (!ValidateBlockAssetList()) {
				Debug.LogError("Block list contains invalid naming");
				return;
			}

			// Hard reference statemachine links between UXF and EXP
			session.onSessionBegin.AddListener(OnSessionBeginUXF);
			session.onSessionEnd.AddListener(OnSessionEndUXF);
			session.onTrialBegin.AddListener(OnTrialBeginUXF);
			session.onTrialEnd.AddListener(OnTrialEndUXF);
		}

		private bool ValidateBlockAssetList() {
			bool _succes = true;
			foreach(EBlock tb in Blocks) {
				string firstPart = tb.name.Split('_')[0];
				if((firstPart != "Break") && (firstPart != "Task")) {
					_succes = false;
				}

				tb.gameObject.SetActive(false);
			}
			return _succes;
		}



		//! Debug

		private void Update()
		{
			if (state != prevState)
			{
				infofield.text = state.ToString();
				prevState = state;
			}
		}

		//! Statemachine

		/// <summary>
		/// Bridge from UXF to Experiment
		/// </summary>
		/// <param name="session"></param>
		void OnSessionBeginUXF(Session session)
		{
			this.Add2Console("OnSessionBeginUXF");

			state = EState.RUNNING;

			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "true" }));
			EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, OnSessionBegin);

			storedBlockNum = 0;

		}

		/// <summary>
		/// Catch UXF after starting session but before starting first trial
		/// </summary>
		/// <param name="param"></param>
		private void OnSessionBegin(eParam param)
		{
			this.Add2Console("OnSessionBegin");

			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, OnSessionBegin);

			Session.instance.BeginNextTrial();
		}


		/// <summary>
		/// Bridge from UXF to Experiment
		/// </summary>
		/// <param name="trial"></param>
		void OnTrialBeginUXF(Trial trial)
		{
			bool isNewBlock = (Session.instance.currentBlockNum != storedBlockNum) && (Session.instance.currentBlockNum <= Session.instance.blocks.Count);

			if (isNewBlock)
			{
				BlockStart();
			}
			else
			{
				StartTrial();
			}
		}


		void BlockStart()
		{
			this.Add2Console("BlockStart");

			storedBlockNum = Session.instance.currentBlockNum;
			
			activeTaskBlock = Blocks[Blocks.FindIndex(x => x.name == Session.instance.CurrentBlock.settings.GetString("_assetId"))];
			activeTaskBlock.gameObject.SetActive(true);

			Debug.Log("HasIntro? : " + Session.instance.CurrentBlock.settings.ContainsKey("_start"));

			bool hasIntro = Session.instance.CurrentBlock.settings.ContainsKey("_start"); // TODO tricky to test on a string

			// Inject introduction step or continue UXF sequence
			if (hasIntro)
			{
				this.Add2Console("Block Intro");

				activeTaskBlock.OnBlockStart();

				//! @Felix now the question is, do we activate the button and wait here, or leave it up to the experimenter. Or make it a 'setting' of the FW (menu/edia/settings => main settings (stores it on disk somehow))
				EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, BlockContinueAfterIntro);
				EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "true" }));
			}
			else
			{
				StartTrial();
			}
		}

		void StartTrial()
		{
			this.Add2Console("StartTrial");

			currentStep = -1;
			NextStep();
		}


		void NextStep()
		{
			this.Add2Console("NextStep");

			currentStep++;

			if (currentStep < activeTaskBlock.trialSteps.Count)
			{
				activeTaskBlock.trialSteps[currentStep].Invoke();
			}
			else EndTrial();
		}

		/// <summary>
		/// Public access for taskmanager
		/// </summary>
		public void WaitOnProceed()
		{
			this.Add2Console("WaitOnProceed");

			state = EState.WAITINGONPROCEED;

			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "TRUE" }));
			EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, OnEvProceed);
		}


		void OnEvProceed(eParam e)
		{
			this.Add2Console("OnEvProceed");

			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, OnEvProceed);
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "false" }));

			Continue();
		}

		/// <summary>
		/// Public access for taskmanager
		/// </summary>
		public void Proceed()
		{
			this.Add2Console("Proceed");

			EventManager.TriggerEvent(eDIA.Events.StateMachine.EvProceed);
		}

		void Continue () {
			state = EState.RUNNING;

			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, OnEvProceed);
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "false" }));

			NextStep();
		}


		//TODO move to top
		Coroutine proceedTimer = null;

		/// <summary>Call next step in the trial with delay.</summary>
		/// <param name="duration">Time to wait before proceeding. Expects float</param>
		public void ProceedWithDelay(float duration) {

			if (proceedTimer != null) StopCoroutine(proceedTimer); // Kill timer, if any
			proceedTimer = StartCoroutine("ProceedTimer", duration);
		}

		/// <summary>Coroutine as timer as we can kill that to avoid delayed calls in the statemachine</summary>
		IEnumerator ProceedTimer(float duration) {
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvStartTimer, new eParam(duration));
			yield return new WaitForSecondsRealtime(duration);

			EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, OnEvProceed);
			Proceed();
		}


		/// <summary>
		/// Bridge from Experiment to UXF
		/// </summary>
		void EndTrial()
		{
			this.Add2Console("EndTrial");
			Session.instance.EndCurrentTrial();
		}

		/// <summary>
		/// Bridge from UXF to Experiment
		/// </summary>
		/// <param name="trial"></param>
		void OnTrialEndUXF(Trial trial)
		{
			this.Add2Console("OnTrialEnd");

			// Are we ending?
			if (Session.instance.isEnding)
				return;

			// Is there a PAUSE requested right now?
			if (isPauseRequested)
			{
				isPauseRequested = false;

				if (trial == Session.instance.LastTrial)
					return;

				SessionBreak();
				return;
			}

			// Reached last trial in a block?
			if (Session.instance.CurrentBlock.lastTrial != trial)
			{ // NO
				Session.instance.BeginNextTrial();
				return;
			}
			else
			{ // YES
				BlockEnd();
				return;
			}
		}

		void BlockEnd()
		{
			this.Add2Console("BlockEnd");

			bool hasOutro = Session.instance.CurrentBlock.settings.ContainsKey("outro"); // TODO this is now always false

			// Inject introduction step or continue UXF sequence
			if (hasOutro)
			{
				this.Add2Console("Block Outro");
				EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, BlockContinueAfterOutro);
				EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "true" }));
			}
			else
			{
				BlockCheckAndContinue();
			}

		}

		/// <summary>
		/// Bridge from UXF to Experiment
		/// </summary>
		/// <param name="session"></param>
		void OnSessionEndUXF(Session session)
		{
			state = EState.ENDED;
			this.Add2Console("onSessionEnd");
		}


		void FinalizeSession()
		{
			this.Add2Console("FinalizeSession");

			Session.instance.End();
		}



		// --------------------------------------------------------------------------------------------------------

		/// <summary>Called from this manager. </summary>
		void BlockContinueAfterIntro(eParam e)
		{
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "false" }));
			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, BlockContinueAfterIntro);

			StartTrial();
		}

		/// <summary>Called from this manager. </summary>
		void BlockContinueAfterOutro(eParam e)
		{
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "false" }));
			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, BlockContinueAfterOutro);

			BlockCheckAndContinue();
		}


		void BlockCheckAndContinue()
		{
			activeTaskBlock.gameObject.SetActive(false);

			// Is this the last trial of the session?
			if (Session.instance.LastTrial == Session.instance.CurrentTrial)
			{
				this.Add2Console("Reached end of trials ");
				FinalizeSession();
				return;
			}

			//if (taskConfig.breakAfter.Contains(Session.instance.currentBlockNum))
			//{
			//	SessionBreak();
			//	return;
			//}

			Session.instance.BeginNextTrialSafe();
		}


		// --------------------------------------------------------------------------------------------------------

		void SessionBreak()
		{
			this.Add2Console("SessionBreak");

			EventManager.StartListening(eDIA.Events.StateMachine.EvProceed, SessionResumeAfterBreak);
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "true" }));

		}

		// <summary>Called from EvProceed event. Stops listener, invokes onSessionResume event and calls UXF BeginNextTrial. </summary>
		void SessionResumeAfterBreak(eParam e)
		{
			this.Add2Console("SessionResume");
			EventManager.StopListening(eDIA.Events.StateMachine.EvProceed, SessionResumeAfterBreak);
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvEnableButton, new eParam(new string[] { "PROCEED", "false" }));

			Session.instance.BeginNextTrialSafe();
		}



	}
