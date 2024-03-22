using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Edia;
using UXF;

public class TaskD2 : XBlock {

	[Space(20)]
	public List<StimulusD2> Stimuli = new();
	public ScreenInVR TaskCanvas; 
	List<int> _validStimuliIndexes = new();
	int _validStimuliAmount = 0; 

	void Awake() {
		TaskCanvas.Show(false);

		trialSteps.Add(GenerateGrid);
		trialSteps.Add(RunTask);
		trialSteps.Add(CheckAndLogResults);
		trialSteps.Add(CleanUp);
	}

	void GenerateGrid() {
		// Determine random valid indexes
		_validStimuliAmount = Session.instance.CurrentBlock.settings.GetInt("validStimuliAmount");
		_validStimuliIndexes = GenerateRandomNumbers(_validStimuliAmount, Stimuli.Count);

		// Generate sheet
		for (int i = 0; i < Stimuli.Count; i++) {
			Stimuli[i].SetValid( _validStimuliIndexes.Contains(i));
		}

		TaskCanvas.Show(true);

		Experiment.Instance.WaitOnProceed();
		Experiment.Instance.Proceed();
	}

	void RunTask() {
		XRManager.Instance.EnableXRRayInteraction(true);

		// Wait on user to finish
		Experiment.Instance.WaitOnProceed();
	}

	public void TaskDoneBtnPressed() {
		this.Add2Console("TaskDoneBtnPressed");
		Experiment.Instance.Proceed();
	}

	public void Dummy() {
		Debug.Log($"Dummy {UnityEngine.Random.Range(0,3000)}");
	}

	void CheckAndLogResults () {
		XRManager.Instance.EnableXRRayInteraction(false);

		// Check sheets and log to UXF
		int correctlyTicked = 0;
		int incorrectlyTicked = 0;

		foreach (StimulusD2 s in Stimuli) {
			if(s.IsValid && s.IsTicked)
				correctlyTicked++;

			if (s.IsTicked && !s.IsValid)
				incorrectlyTicked++;
		}

		Session.instance.CurrentTrial.result["correctlyTicked"] = correctlyTicked;
		Session.instance.CurrentTrial.result["incorrectlyTicked"] = incorrectlyTicked;

		Experiment.Instance.WaitOnProceed();
		Experiment.Instance.Proceed();
	}

	private void CleanUp() {
		foreach (StimulusD2 s in Stimuli) {
			s.Reset();
		}

		TaskCanvas.Show(false);

		Experiment.Instance.WaitOnProceed();
		Experiment.Instance.Proceed();
	}

	static List<int> GenerateRandomNumbers(int count, int stimulusCount) {
		System.Random random = new System.Random();
		List<int> randomNumbers = new List<int>();

		while(randomNumbers.Count < count) {
			
			int randNum = random.Next(stimulusCount);
			if (!randomNumbers.Contains(randNum)) {
				randomNumbers.Add(randNum);
			}
		}

		return randomNumbers;
	}

}


