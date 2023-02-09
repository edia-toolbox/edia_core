using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace eDIA.Manager {

	public class PanelTaskControl : ExperimenterPanel {

		[Tooltip("If the panel needs to be active at start")]
		public bool showPanelAtStart = false;

		private void Start() {
			ShowPanel(showPanelAtStart);
		}
	
	}
}