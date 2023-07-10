using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace eDIA {

    /// <summary>Static definitions</summary>
    public static class Constants {
        public static string localConfigDirectoryName = "Configs";

        public enum Interactor { LEFT, RIGHT, BOTH, NONE }

        // Fixed FPS target
        public enum TargetHZ { NONE, H60, H72, H90, H120 }

        // System language
        public enum Languages { ENG, DU }

        // List of possible resolutions
        public static List<Vector2> screenResolutions = new List<Vector2> () {
            new Vector2 (1280, 720),
            new Vector2 (1920, 1080),
            new Vector2 (2048, 1440)
        };
    }
}