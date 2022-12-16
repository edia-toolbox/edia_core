using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA.Manager {

	[System.Serializable]
	public enum ControlMode { StandAlone, Slave };
	public enum PanelMode { Hidden, OnScreen, InWorld };
	
	[System.Serializable]
	public class ControlSettings {
		public ControlMode ControlMode = ControlMode.StandAlone;
		public PanelMode PanelMode = PanelMode.OnScreen;
		public bool LookForConfigs = true;
		public bool AutoInitialise = true;
	}


	public class Constants 
	{

	}
	
}
