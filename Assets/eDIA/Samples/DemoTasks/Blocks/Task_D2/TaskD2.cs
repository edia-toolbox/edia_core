using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using eDIA;
using UXF;

public class TaskD2 : XBlock {
	
	public List<StimulusD2> stimuli = new();
	List<int> validStimuliIndexes = new();
	public int validStimuliAmount = 8; // TODO this would be a setting
	public ScreenInVR TaskCanvas; 

	void Awake() {
		TaskCanvas.Show(false);

		trialSteps.Add(GenerateGrid);
		trialSteps.Add(RunTask);
		trialSteps.Add(CheckAndLogResults);
		trialSteps.Add(CleanUp);
	}

	void GenerateGrid() {
		// Determine random valid indexes
		validStimuliIndexes = GenerateRandomNumbers(validStimuliAmount, stimuli.Count);

		// Generate sheet
		for (int i = 0; i < stimuli.Count; i++) {
			stimuli[i].SetValid( validStimuliIndexes.Contains(i) ? true : false );
		}

		TaskCanvas.Show(true);

		Xperiment.Instance.WaitOnProceed();
		Xperiment.Instance.Proceed();
	}

	void RunTask() {
		XRManager.Instance.EnableXRRayInteraction(true);

		// Wait on user to finish
		Xperiment.Instance.WaitOnProceed();
	}

	public void TaskDoneBtnPressed() {
		this.Add2Console("TaskButtonPressed calling Proceed");
		Xperiment.Instance.Proceed();
	}

	void CheckAndLogResults () {
		XRManager.Instance.EnableXRRayInteraction(false);

		// Check sheets and log to UXF
		int correctlyTicked = 0;
		int incorrectlyTicked = 0;

		foreach (StimulusD2 s in stimuli) {
			if(s.IsValid && s.isTicked)
				correctlyTicked++;

			if (s.isTicked && !s.IsValid)
				incorrectlyTicked++;
		}

		Debug.Log("correctlyTicked: " + correctlyTicked + " incor: " + incorrectlyTicked);

		Session.instance.CurrentTrial.result["correctlyTicked"] = correctlyTicked;
		Session.instance.CurrentTrial.result["incorrectlyTicked"] = incorrectlyTicked;

		Xperiment.Instance.WaitOnProceed();
		Xperiment.Instance.Proceed();
	}

	private void CleanUp() {
		foreach (StimulusD2 s in stimuli) {
			s.Reset();
		}

		TaskCanvas.Show(false);

		Xperiment.Instance.WaitOnProceed();
		Xperiment.Instance.Proceed();
	}


	static List<int> GenerateRandomNumbers(int count, int maxValue) {
		System.Random random = new System.Random();
		List<int> randomNumbers = new List<int>();

		for (int i = 0; i < count; i++) {
			randomNumbers.Add(random.Next(maxValue));
		}

		return randomNumbers;
	}

	public override void OnBlockStart() {
		base.OnBlockStart();

	}

	public override void OnBlockEnd() {
		base.OnBlockEnd();

	}
}


