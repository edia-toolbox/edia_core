using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA
{
    /// <summary>Static definitions</summary>
    public static class Constants
    {

    #region LOCATIONS
        // Config directory name
        public static string localConfigDirectoryName = "Configs";

        // Custom path
        // public static string localPathToLogfiles = "";

    #endregion // -------------------------------------------------------------------------------------------------------------------------------
    #region RENDERING

        // Fixed FPS target
		public enum TargetHZ { NONE, H60, H72, H90, H120 };

    #endregion // -------------------------------------------------------------------------------------------------------------------------------
    #region USER 

        // 
        public enum PrimaryInteractor { LEFTHANDED, RIGHTHANDED };


    #endregion // -------------------------------------------------------------------------------------------------------------------------------

    }
}
