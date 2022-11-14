using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA
{
    /// <summary>Container to hold main settings of the application </summary>
    [System.Serializable]
	public class SettingsDeclaration {
        public Constants.Interactor VisableInteractor = Constants.Interactor.BOTH;
		public Constants.Interactor InteractiveInteractor = Constants.Interactor.RIGHT;
		public int screenResolution = 0;
		public float volume = 50f;
        public Constants.Languages language = Constants.Languages.ENG;

        public string pathToLogfiles = "logfiles";
        public static string localConfigDirectoryName = "Configs";
	}

    /// <summary>Static definitions</summary>
    public static class Constants
    {
        public static string localConfigDirectoryName = "Configs";

        public enum Interactor { LEFT, RIGHT, BOTH };

        // Fixed FPS target
		public enum TargetHZ { NONE, H60, H72, H90, H120 };

        // System language
        public enum Languages { ENG, DU };

        // List of possible resolutions
        public static List<Vector2> screenResolutions = new List<Vector2>() { 
            new Vector2(1280,720),
            new Vector2(1920,1080),
            new Vector2(2048,1440) 
        };
        
    }
}
