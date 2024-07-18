using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Edia.Controller {

	[System.Serializable]
	public enum ControlMode { Local, Remote };
	public enum PanelMode { Hidden, OnScreen, InWorld };
	public enum ScreenSize { Default, Wide, Full };

	[System.Serializable]
	public class ControlSettings {
		public ControlMode ControlMode = ControlMode.Local;
		//public PanelMode PanelMode = PanelMode.OnScreen;
		//public ScreenSize ScreenSize = ScreenSize.Default;	
	}


	public static class Constants 
	{
		// File names and paths
		public static string FileNameSessionSequence		= "session-sequence.json";
		public static string FileNameSessionInfo			= "session-info.json";
		public static string PathToParticipantFiles			= "configs/participants/";
		public static string PathToBaseDefinitions			= "configs/base-definitions/";
		public static string FolderNameXBlockDefinitions	= "block-definitions";

	}
	
}
