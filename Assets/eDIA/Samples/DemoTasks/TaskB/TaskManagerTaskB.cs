using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using eDIA;
using UnityEngine.UI;
using TMPro;

namespace TASK {

		/// <summary>
		/// This is a DEMO script for a very simple task.
		/// All main functionalities to run the Experiment, UXF and Trial statemachine are in TaskManagerBase so no need to worry about that.<br/><br/>
		/// <para>To start your own TaskManager for your experiment task: 
		/// 1. Copy/Paste the complete 'DemoTask' folder from /eDIA/Samples to the /Assets/ folder in your project<br/>
		/// 2. Rename the script file and class to your Taskname<br/>
		/// 3. Switch the inspector to 'debug' mode (3 dots right topcorner)<br/>
		/// 4. Find [ SYSTEM ] > [ EXP ] gameobject and replace 'TaskManagerDemo' script entry in the inspector with your TaskManager script and switch inspector mode back again</para>
		/// </summary>
		public class TaskManagerTaskB : TaskManagerBase {

			[Header(("Task related refs"))]
			public Image stimuliHolder;
			public Transform buttonPanel = null;
			public List<Sprite> stimulis = new List<Sprite>();

#region MONO METHODS 

			void OnDisable() {
			}

			void OnDestroy() {
			}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region OVERRIDE INHERITED METHODS
			public override void OnEnable() {
				base.OnEnable(); //! Do not remove

				// ---------------------------------------------------------------------------------------------
				// Custom task related actions go down here ▼

				// ..
			}
			
			public override void Awake() {
				base.Awake(); // Tries to get the references to the MainCamera, Right/LeftController transforms //! Do not remove
			
				// ---------------------------------------------------------------------------------------------
				// Custom task related actions go down here ▼

				// ..
			}

			public override void Start() {
				base.Start(); //! Do not remove

				TaskSequenceSteps();

				// ---------------------------------------------------------------------------------------------
				// Custom task related actions go down here ▼

				// ..
			}

			public override void ResetTrial() {
				base.ResetTrial(); //! Do not remove

				// Add resetting of values for the start of a new trial here
				// someValue = 0;
			}

			public override void OnSessionEndUXF () {
				base.OnSessionEndUXF(); //! Do not remove

				// Add actions to do when session has ended, like showing message to the user
				EventManager.TriggerEvent("EvShowMessage", new eParam("Session ended, logfiles saved"));

			}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EDIA XR RIG 

			/// <summary>Setup rig with things we need for this task. </summary>
			public override void OnEvFoundXRrigReferences(eParam e) {
				base.OnEvFoundXRrigReferences(e); // Do system related hidden actions first

				// ---------------------------------------------------------------------------------------------
				// Custom task related actions go down here ▼

			}

			/// <summary>At this point we have acces to all taskSettings</summary>
			public override void OnExperimentInitialised(bool result) {
				base.OnExperimentInitialised(result); //! Do not remove
				
				// ---------------------------------------------------------------------------------------------
				// Custom task related actions go down here ▼
				
			}
			
			public void XRrigCleanUP () {
				// Remove shizzle from XR rig that is only for this task
			}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK STRUCTURE METHODS 

			/// <summary>The steps the trial goes through</summary>
			void TaskSequenceSteps() {
				trialSequence.Add(new TrialStep("Present Stimuli", TaskStep1));
				trialSequence.Add(new TrialStep("Show buttons, wait on user", TaskStep2));
				trialSequence.Add(new TrialStep("Wait ", TaskStep3));
				//trialSequence.Add( new TrialStep("last step", TaskStepX) );
				// etc
			}

			/// <summary>Present Stimuli</summary>
			void TaskStep1() {
				AddToLog("Step:" + (currentStep + 1) +" > " + trialSequence[currentStep].title);

				ExperimentManager.Instance.EnableExperimentPause(true);

				buttonPanel.gameObject.SetActive(false);
				EventManager.TriggerEvent("EvShowMessage", new eParam("Take a good look at this symbol"));

				// Show stimuli
				stimuliHolder.sprite = stimulis[Random.Range(0,stimulis.Count)];
				stimuliHolder.gameObject.SetActive(true);

				Invoke("NextStepFromUserOrSceneOrButtonOrTimerOrWhatever", taskSettings.GetFloat("timerWait")); 
			}
			
			/// <summary>Move cube, wait on user input</summary>
			void TaskStep2() {
				AddToLog("Step:" + (currentStep + 1) +" > " + trialSequence[currentStep].title);

				// Show buttons
				for (int b=0;b<buttonPanel.transform.childCount;b++) {
					buttonPanel.GetChild(b).gameObject.SetActive( b < Session.instance.CurrentTrial.settings.GetInt("button_range") ? true : false  );
				}
				buttonPanel.gameObject.SetActive(true);

				EventManager.TriggerEvent("EvShowMessage", new eParam("Rate it from 1 - 5 by pressing the corresponding button"));
			}

			/// <summary>Wait</summary>
			void TaskStep3() {
				AddToLog("Step:" + (currentStep + 1) +" > " + trialSequence[currentStep].title);

				stimuliHolder.gameObject.SetActive(false);

				EventManager.TriggerEvent("EvShowMessage", new eParam("Thank you"));
				Invoke("NextStepFromUserOrSceneOrButtonOrTimerOrWhatever", taskSettings.GetFloat("timerWait")); 
			}

			/// <summary>Call this from your code to proceed to the next step</summary>
			public void NextStepFromUserOrSceneOrButtonOrTimerOrWhatever() {
				NextStep();
			}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BUTTON PRESSES

			public void BtnPressed (int btnID) {

				Session.instance.CurrentTrial.result["selected_button"] = btnID;
				buttonPanel.gameObject.SetActive(false);

				NextStepFromUserOrSceneOrButtonOrTimerOrWhatever();
			}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region SESSION START / END
			
			// Every Experiment has a START and an END event!!
			// Use these to show an introduction at the start and a thank-you or score overview in the end. 
			
			/// <summary>Called from Experiment manager</summary>
			public override void OnSessionStart() {
				//! System awaits 'EvProceed' event automaticcaly to proceed to first trial. 
				EventManager.TriggerEvent("EvShowMessage", new eParam("Welcome to the experiment, please click button to continue"));

				// Add additional task specific settings to the UXF logging system
				// Session.instance.settings.SetValue("timerShowCube", taskSettings.GetFloat("timerShowCube"));
			}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BREAK

			// If there is a BREAK in the experiment, these methods get called
			public override void OnSessionBreak() {
				//! System waits on 'EvProceed' event automaticaly to proceed
				Debug.Log("<color=#ffffff> >>> Take a short break </color>");
				EventManager.TriggerEvent("EvShowMessage", new eParam("Take a short break, \nClick button to continue"));

			}

			public override void OnSessionResume () {
				Debug.Log("<color=#ffffff> >>> Resuming experiment </color>");
				EventManager.TriggerEvent("EvShowMessage", new eParam("Resuming experiment"));

			}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BLOCK INTRODUCTION
			
			// If there is a INTRODUCTION at the start at some block, these methods get called
			// Use ExperimentManager.Instance.experimentConfig.GetBlockIntroduction(Session.instance.currentBlockNum) to get the text.
			
			/// <summary>Called when the block introduction starts</summary>
			public override void OnBlockIntroduction() {
				//! System waits on 'EvProceed' event automaticaly to proceed

				EventManager.TriggerEvent("EvShowMessage", new eParam(  ExperimentManager.Instance.experimentConfig.GetBlockIntroduction(Session.instance.currentBlockNum) ));
				Debug.Log("<color=#ffffff> >>> "+ ExperimentManager.Instance.experimentConfig.GetBlockIntroduction(Session.instance.currentBlockNum) + "</color>");

			}

			/// <summary>Called when block resumes</summary>
			public override void OnBlockResume () {


			}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}

}