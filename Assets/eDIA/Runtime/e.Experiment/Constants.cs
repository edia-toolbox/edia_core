using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA
{
    /// <summary>Static data that can be used anywhere in the system</summary>
    public static class Constants
    {
        public static string localConfigDirectoryName = "Configs";

        // Systemwide main experimenter canvas button definition
        public enum ExperimenterCanvasButtons { NONE, EXP_START, EXP_PAUSE, EXP_PROCEED, SES_NEW, EYE_CALIBRATION };

    }
}
