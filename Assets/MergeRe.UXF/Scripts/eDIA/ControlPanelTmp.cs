using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using eDIA;
using System;

namespace eDIA
{
	public class ControlPanelTmp : MonoBehaviour
	{
		public Button proceedBtn;
		public int counting = 0;

		private void Start()
		{
			EventManager.StartListening(eDIA.Events.ControlPanel.EvEnableButton, OnEvEnableButton);
		}

		private void OnEvEnableButton(eParam param)
		{
			bool turnOn = param.GetStrings()[1].ToUpper() == "TRUE";

			switch (param.GetStrings()[0].ToUpper())
			{ 
				case "PROCEED":
					proceedBtn.interactable = turnOn;
					counting++;
					break;
			}
		}

		public void ProceedBtnPressed()
		{
			EventManager.TriggerEvent(eDIA.Events.StateMachine.EvProceed);
		}


		



	}
}