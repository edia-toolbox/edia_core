using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using TMPro;
using eDIA;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace SFEM {
 
	// USER TASK CONTROL
	public class TaskManagerSFEM : TaskManagerBase {

		// Avatars
		[System.Serializable]
		public class avatarEmos {
			public List<GameObject> emos = new List<GameObject>();
		}

		[Header("Stimuli")]
		public Transform stimuliHolder;
		public Transform RTT;
		public List<avatarEmos> avatars = new List<avatarEmos>();
		public GameObject 	fixationObject;
		[Tooltip("The Y offset of the stimuli to the XR camera")]
		public float stimuliYoffset =  1.505f;
		
		GameObject activeAvatar;
		bool is3D = true;
		
		[Header("User Canvas")]
		public GameObject 	taskCanvas;
		public List<Button> 	emotionButtons = new List<Button>();

		// Info
		private GameObject 	infoPanel;
		private Button 		infoPanelButton;
		private TextMeshProUGUI infoPanelTextField;
  
		// Emotions
		private GameObject 	emotionsCanvas;
		private Coroutine 	userTimer = null;
		private DateTime 		startTimestamp;

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
			base.Awake(); // Tries to get the references to the XRcamera, Right/LeftController transforms //! Do not remove
		
			// ---------------------------------------------------------------------------------------------
			// Custom task related actions go down here ▼

		}

		public override void Start() {
			base.Start(); //! Do not remove

			TaskSequenceSteps();

			// ---------------------------------------------------------------------------------------------
			// Custom task related actions go down here ▼

			// Instead of referencing everything in the inspector, we find some gameobjects just once at startup.
			GetObjectReferences ();
		}

		public override void ResetTrial() {
			base.ResetTrial(); //! Do not remove

			// Add resetting of values for the start of a new trial here
			// someValue = 0;
		}

		public override void OnSessionEndUXF () {
			base.OnSessionEndUXF(); //! Do not remove

			// Add actions to do when session has ended, like showing message to the user
			ShowInfo("End of Session, Logfiles saved", false);
		}

		public override void OnExperimentInitialised(bool result) {
			base.OnExperimentInitialised(result); //! Do not remove

			// Add actions when Experiment manager has prepared everything to start a UXF session.
		}



#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EDIA XR RIG 

		/// <summary>Setup rig with things we need for this task. </summary>
		public override void OnEvFoundXRrigReferences(eParam e) {
			base.OnEvFoundXRrigReferences(e); //! Do not remove

			// ---------------------------------------------------------------------------------------------
			// Custom task related actions go down here ▼

			// Position the canvas and stimuli at the desired distance
			UpdateStimuliPosition();
		}

		public void XRrigCleanUP () {
			// Remove shizzle from XR rig that is only for this task
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK STRUCTURE METHODS 

		/// <summary>The steps the trial goes through</summary>
		void TaskSequenceSteps() {
			trialSequence.Add(new TrialStep("Fixation", 	TaskStep1));
			trialSequence.Add(new TrialStep("Stimuli", 	TaskStep2));
			trialSequence.Add(new TrialStep("UserInput", 	TaskStep3));
			trialSequence.Add(new TrialStep("Interval", 	TaskStep4));
			//trialSequence.Add( new TrialStep("last step", TaskStepX) );
			// etc
		}

		/* Task steps are:
			1. instruction > until keypress
			2. fixation cross > 0.5s
			3. face, either 2d or 3d > 1s
			4. selection > user choice or 5s
			5. inner-trial interval > 1s
		*/

		void TaskStep1() {
			// Show fixation cross
			EventManager.StopListening("EvEyeCalibrationRequested", OnEvEyeCalibrationRequested);			
			
			// Enable injecting break
			ExperimentManager.Instance.EnableExperimentPause(true);

			UpdateStimuliPosition ();

			fixationObject.SetActive(true);
			Invoke("NextStepFromUserOrSceneOrButtonOrTimerOrWhatever", taskSettings.GetFloat("timerFixation"));
		}

		void TaskStep2() {
			// Show face, either 2d or 3d > 1s
			UpdateStimuliPosition ();

			fixationObject.SetActive(false);
			ShowAvatar();
			Invoke("NextStepFromUserOrSceneOrButtonOrTimerOrWhatever", taskSettings.GetFloat("timerStimuli"));
		}

		void TaskStep3() {
			// selection > user choice or 5s
			fixationObject.SetActive(false);
			ShowEmotionCanvas();

			// Start listening to user input
			EventManager.StartListening("EvEmotionSelected", OnEvEmotionSelected);

			// start timer for max time for user
			userTimer = StartCoroutine("WaitOnInput", taskSettings.GetFloat("timerResponse"));

			// Store this timestamp
			startTimestamp = DateTime.Now;
		}

		void TaskStep4() {
			// inner-trial interval > 1s
			AddToLog("Step:" + (currentStep + 1) +" > " + trialSequence[currentStep].title);
			fixationObject.SetActive(true);

			// Invoke method after interval
			Invoke("NextStepFromUserOrSceneOrButtonOrTimerOrWhatever", taskSettings.GetFloat("timerInterTrialInterval"));
		}

		void CleanUp () {
			HideInfo();
			fixationObject.SetActive(false);
			emotionsCanvas.SetActive(false);
			HideAvatar();

			XRrigUtilities.EnableXRInteractorLine(false);

			// foreach(Button b in emotionButtons)
			// 	b.Select();
			//TODO: deselect last chosen button, but how? Otherwise it won't turn green the 2nd time it's been chosen. Does this matter for the real VR version anyway?

			if (userTimer != null) 
				StopCoroutine(userTimer);

			userTimer = null;
		}

		/// <summary>Call this from you code to proceed to the next step</summary>
		public void NextStepFromUserOrSceneOrButtonOrTimerOrWhatever() {
			CleanUp();
			NextStep();
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EVENT HANDLERS

		void OnEvEmotionSelected (eParam e ) {
			StopCoroutine(userTimer);
			EventManager.StopListening("EvEmotionSelected", OnEvEmotionSelected);


			string selectedEmotion = "-1";
			int isCorrect = 0;

			if (e.GetInt() != -1) {

				emotionButtons[e.GetInt()-1].Select();

				// Disable all buttons for input except chosen one (so it shows hightlight)
				foreach(Button b in emotionButtons) {
					if (b!=emotionButtons[e.GetInt()-1]) 
						b.interactable = false;
				}

				int pressedButtonID = e.GetInt();
				string emotionID = Session.instance.CurrentTrial.settings.GetString("order").Substring(e.GetInt()-1,1);
				string emotionName = taskSettings.GetStringList("emotionNames")[int.Parse(emotionID)-1];
				AddToLog("btn:" + pressedButtonID + " = emo:" + emotionID + " =" + emotionName);

				selectedEmotion = Session.instance.CurrentTrial.settings.GetString("order").Substring(e.GetInt()-1,1);
				isCorrect = emotionID == Session.instance.CurrentTrial.settings.GetString("emo") ? 1 : 0;
			}
			
			Session.instance.CurrentTrial.result["selected_emotion"] = selectedEmotion;
			Session.instance.CurrentTrial.result["correct_response"] = isCorrect;
			Session.instance.CurrentTrial.result["response_time"] = (DateTime.Now - startTimestamp).TotalSeconds;
			
			Invoke("NextStepFromUserOrSceneOrButtonOrTimerOrWhatever", taskSettings.GetFloat("timerResponseFeedback"));
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region PUBLIC METHODS

		public void EmotionBtnPressed (int buttonID) {
			XRrigUtilities.EnableXRInteractorLine(false);
			OnEvEmotionSelected(new eParam(buttonID));
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region TASK HELPERS

		void Show3D () {
			// Debug.Log("Show3D");
			RTT.gameObject.SetActive(false);
			SetLayerRecursively(activeAvatar.gameObject, LayerMask.NameToLayer("Default"));
			is3D = true;
		}

		void Show2D () {
			// Debug.Log("Show2D");
			SetLayerRecursively(activeAvatar.gameObject, LayerMask.NameToLayer("Task_RTT"));
			RTT.gameObject.SetActive(true);
			is3D = false;
		}

		static void SetLayerRecursively(GameObject go, int layerNumber) {
			if (go == null) return;
			foreach (Transform trans in go.GetComponentsInChildren<Transform>(true)) {
				trans.gameObject.layer = layerNumber;
			}
		}
		
		void ShowAvatar () {
			// Convert stringvalue to array index
			int avatarIndex = Session.instance.CurrentTrial.settings.GetInt("avatar") - 1;
			int emotionIndex = Session.instance.CurrentTrial.settings.GetInt("emo") - 1;

			activeAvatar = avatars[avatarIndex].emos[emotionIndex];
			ExperimentManager.Instance.SendMarker("ShowAvatar: " + Session.instance.CurrentTrial.settings.GetString("marker"));

			// [un]hide avatar
			for (int a=0;a<avatars.Count;a++) {
				for (int e=0;e<avatars[a].emos.Count;e++) {
					avatars[a].emos[e].SetActive(avatars[a].emos[e] == activeAvatar ? true : false);
				}
			}

			if (Session.instance.CurrentTrial.settings.GetString("stereo") == "1") 
				Show3D();
			else Show2D();
		}
		
		void HideAvatar() {
			if (activeAvatar == null)
				return;

			RTT.gameObject.SetActive(false);
			activeAvatar.gameObject.SetActive(false);
			activeAvatar = null;
		}

		void ShowEmotionCanvas() {
			// Show options
			for (int i=0;i<emotionButtons.Count;i++) {
				string emotionNumber = Session.instance.CurrentTrial.settings.GetString("order").Substring(i,1);
				string emotionName = taskSettings.GetStringList("emotionNames")[int.Parse(emotionNumber)-1];
				emotionButtons[i].name = emotionNumber + "." + emotionName;
				emotionButtons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = emotionName.ToUpper();
				emotionButtons[i].interactable = true;
			}

			emotionsCanvas.SetActive(true);

			// Show controller rays input
			XRrigUtilities.EnableXRInteractorLine(true);
		}

		IEnumerator WaitOnInput (int _seconds) {
			yield return new WaitForSeconds(_seconds);
			AddToLog("Timer ended");

			OnEvEmotionSelected(new eParam(-1)); //! Means NO INPUT RECIEVED
		}

		/// <summary>
		/// Shows the given information string.
		/// </summary>
		/// <param name="msg">Message to show</param>
		/// <param name="showBtn">Show proceed button</param>
		void ShowInfo (string msg, bool showBtn) {
			infoPanelTextField.text = msg;
			infoPanel.SetActive(true);
			infoPanelButton.gameObject.SetActive(showBtn);

			// EventManager.TriggerEvent("EvButtonChangeState", new eParam( new string[] { "eProceed", "true" }));

			if (showBtn)
				XRrigUtilities.EnableXRInteractorLine(true);
		}

		void HideInfo () {
			infoPanel.SetActive(false);
			infoPanelButton.gameObject.SetActive(false);
			XRrigUtilities.EnableXRInteractorLine(false);
		}

		void UpdateStimuliPosition () {
			stimuliHolder.transform.localPosition = new Vector3(0, (-1 * stimuliYoffset), taskSettings.GetFloat("distanceStimuli"));
		}

		void GetObjectReferences () {
			if (taskCanvas == null) {
				Debug.LogError("No infocanvas referenced");
				return;
			}

			infoPanel 			= taskCanvas.transform.GetChild(0).gameObject;
			infoPanelTextField 	= infoPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
			infoPanelButton		= infoPanel.transform.GetChild(2).gameObject.GetComponent<Button>();
			emotionsCanvas		= taskCanvas.transform.GetChild(1).gameObject;
		}

		/// <summary>
		/// Set system open for calibration call from event or button
		/// </summary>
		/// <param name="onOff"></param>
		void EnableCalibrationRequest (bool onOff) {
			EventManager.TriggerEvent("EvButtonChangeState", new eParam( new string[] { ((int)ExperimenterCanvasButtons.EYE_CALIBRATION).ToString(), onOff.ToString() }));

			if (onOff)
				EventManager.StartListening("EvEyeCalibrationRequested", OnEvEyeCalibrationRequested);
			else 
				EventManager.StopListening("EvEyeCalibrationRequested", OnEvEyeCalibrationRequested);
		}

		/// <summary>
		/// Call SRanipal steamVR eye calibration tool
		/// </summary>
		void OnEvEyeCalibrationRequested (eParam e) {
			EnableCalibrationRequest(false);

			// ViveSR.anipal.Eye.SRanipal_Eye.LaunchEyeCalibration();
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region SESSION START / END
		
		// Every Experiment has a START and an END event!!
		// Use these to show an introduction at the start and a thank-you or score overview in the end. 

		/// <summary>Called from Experiment manager</summary>
		public override void OnSessionStart() {
			// Show Intro
			// System awaits 'EvProceed' event automaticcaly to proceed to the first trial. 
			ShowInfo("Welcome to the StereoFEM experiment.", false);
			ExperimentManager.Instance.EnableExperimentProceed(true);

			// eye calibration option enabled
			EnableCalibrationRequest(true);

			// Add which tasksettings should be added to the session settings, which get logged to disk
			Session.instance.settings.SetValue("emotionNames", taskSettings.GetStringList("emotionNames"));
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BREAK

		// If there is a BREAK in the experiment, these methods get called

		public override void OnSessionBreak() {
			ShowInfo("Please take a short break. ", false);
			ExperimentManager.Instance.EnableExperimentProceed(true);

			// Disable 'inject break' button
			ExperimentManager.Instance.EnableExperimentPause(false);

			// eye calibration option enabled
			EnableCalibrationRequest(true);
		}

		public override void OnSessionResume () {
			EnableCalibrationRequest(false);
			HideInfo();
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region BLOCK INTRODUCTION
		
		// If there is a INTRODUCTION at the start at some block, these methods get called
		// Use ExperimentManager.Instance.experimentConfig.GetBlockIntroduction(Session.instance.currentBlockNum) to get the text.
		
		/// <summary>Hook up to Experiment OnSessionBreak event</summary>
		public override void OnBlockIntroduction() {

			ShowInfo(ExperimentManager.Instance.experimentConfig.GetBlockIntroduction(Session.instance.currentBlockNum), false);
			ExperimentManager.Instance.EnableExperimentProceed(true);

			// Disable 'inject break' button
			ExperimentManager.Instance.EnableExperimentPause(false);

			// eye calibration option enabled
			EnableCalibrationRequest(true);
			
		}

		/// <summary>Hook up to Experiment OnSessionResume event</summary>
		public override void OnBlockResume () {

			EnableCalibrationRequest(false);
			HideInfo();
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}
}