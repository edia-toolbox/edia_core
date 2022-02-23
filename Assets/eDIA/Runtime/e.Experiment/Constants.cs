using Microsoft.VisualBasic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA
{
    /// <summary>Container to hold main settings of the application </summary>
    [System.Serializable]
	public class SettingsDeclaration {
		public Constants.PrimaryInteractor primaryInteractor = Constants.PrimaryInteractor.RIGHTHANDED;
		public Vector2 onScreenResolution = new Vector2(1920f,1080f);
		public float volume = 50f;
        public Constants.Languages language = Constants.Languages.ENG;

        public string pathToLogfiles = "logfiles";
        public static string localConfigDirectoryName = "Configs";

	}

    /// <summary>Static definitions</summary>
    public static class Constants
    {
        public static string localConfigDirectoryName = "Configs";

        // Fixed FPS target
		public enum TargetHZ { NONE, H60, H72, H90, H120 };

        public enum PrimaryInteractor { LEFTHANDED, RIGHTHANDED };

        public enum Languages { ENG, DU };


    }
}
