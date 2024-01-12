using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

namespace eDIA.Manager {

	/// <summary>Panel for setting up config files, for now choosing them from pre-set versions</summary>
	/// 
	public class PanelConfigMaker : ExperimenterPanel {

		[Header ("Refs")]

		public TextMeshProUGUI infoTextField;
		public TextMeshProUGUI taskConfigField;
		public TextMeshProUGUI expConfigField;

		public void Init() {

			Reset();
			ShowPanel();

			EventManager.StartListening(eDIA.Events.Config.EvReadyToGo, OnEvReadyToGo);
		}

		private void OnEvReadyToGo(eParam obj)
		{
			EventManager.StopListening(eDIA.Events.Config.EvReadyToGo, OnEvReadyToGo);
			HidePanel();
		}


		/// <summary>Clear everything to startstate</summary>
		void Reset () {
			infoTextField.text = "Config maker";

		}


		public void BtnTSubmitPressed () {
			//EventManager.TriggerEvent(eDIA.Events.Config.EvSetTaskConfig, new eParam(taskConfigField.text)); 
			
			// HidePanel();
		}

		public void BtnESubmitPressed () {
			//EventManager.TriggerEvent(eDIA.Events.Config.EvSetExperimentConfig, new eParam(expConfigField.text)); 
			
			// HidePanel();
		}

	}
}