using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA.Manager {

	[System.Serializable]
	public enum ControlMode { Local, Remote };
	public enum PanelMode { Hidden, OnScreen, InWorld };
	public enum ScreenSize { Default, Wide, Full };

	[System.Serializable]
	public class ControlSettings {
		public ControlMode ControlMode = ControlMode.Local;
		public PanelMode PanelMode = PanelMode.OnScreen;
		// public bool LookForLocalConfigs = true;
		public ScreenSize ScreenSize = ScreenSize.Default;	
	}


	public class Constants 
	{

	}
	
}
