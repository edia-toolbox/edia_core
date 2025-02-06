using System.Collections.Generic;
using UnityEngine;

namespace Edia {
    /// <summary>Static definitions</summary>
    public static class Constants {
        public enum ManipulatorSides {
            Left,
            Right,
        }

        // Fixed FPS target
        public enum TargetHZ {
            NONE,
            H60,
            H72,
            H90,
            H120
        }

        // System language
        public enum Languages {
            ENG,
            DU
        }

        // List of possible resolutions
        public static List<Vector2> screenResolutions = new List<Vector2>() {
            new Vector2(1280, 720),
            new Vector2(1920, 1080),
            new Vector2(2048, 1440)
        };

        public enum EyeId {
            LEFT,
            RIGHT,
            CENTER
        }

        [System.Serializable]
        public enum ControlMode {
            Local,
            Remote
        };

        public enum PanelMode {
            Hidden,
            OnScreen,
            InWorld
        };

        // File names and paths
        public static string FileNameSessionSequence = "session-sequence.json";
        public static string FileNameSessionInfo = "session-info.json";
        public static string PathToParticipantFiles = "configs/participants/";
        public static string PathToBaseDefinitions = "configs/base-definitions/";
        public static string FolderNameXBlockDefinitions = "block-definitions";

        public static string FileNameEdiaSettings = "Edia-settings.json";
    }
}