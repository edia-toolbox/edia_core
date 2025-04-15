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

        public enum EyeId {
            LEFT,
            RIGHT,
            CENTER
        }

        [System.Serializable]
        public enum ControlModes {
            Local,
            Remote
        };

        public enum PanelMode {
            Hidden,
            OnScreen,
            InWorld
        };

        // File names and paths
        public static string FileNameSessionSequence     = "session-sequence.json";
        public static string FileNameSessionInfo         = "session-info.json";
        public static string PathToParticipantFiles      = "configs/participants/";
        public static string PathToBaseDefinitions       = "configs/base-definitions/";
        public static string FolderNameXBlockDefinitions = "block-definitions";

        public static string FileNameEdiaSettings = "Edia-settings.json";


#region Color Theme

        public enum ThemeComponents {
            Button,
            Toggle,
            Slider,
            Dropdown,
            ControlPanelBG,
            ItemPanelBG,
            SubPanelBG
        }
        
        public static event System.Action OnThemeChanged;

        private static ColorThemeDefinition _activeTheme = null;

        public static ColorThemeDefinition ActiveTheme {
            get {
                // Lazy initialization if no theme has been set yet
                if (_activeTheme == null) {
                    _activeTheme = ScriptableObject.CreateInstance<ColorThemeDefinition>();
                    Debug.Log("Created default theme as none was set");
                }

                return _activeTheme;
            }
            set {
                _activeTheme = value;

                if (OnThemeChanged != null) {
                    OnThemeChanged.Invoke();
                }
                else {
                    Debug.LogWarning("Theme changed but no listeners were registered");
                }
            }
        }

        
#endregion

#region EDIA hardcoded colors

        // EDIA defined system colors
        public static Dictionary<string, Color> EdiaColors = new Dictionary<string, Color>() {
            { "Blue", ParseColor("#347FAA", Color.blue) },
            { "Cyan", ParseColor("#34AAAA", Color.cyan) },
            { "Green", ParseColor("#428360", Color.green) },
            { "Grey", ParseColor("#797873", Color.grey) },
            { "Orange", ParseColor("#D9740D", Color.white) },
            { "Purple", ParseColor("#C36897", Color.magenta) },
            { "Yellow", ParseColor("#FFDC4A", Color.yellow) },
            { "White", ParseColor("#F2F2F2", Color.white) },
            { "Black", ParseColor("#0D0D0D", Color.black) }
        };

        public static Color RandomEdiaColor() {
            var colorsList = new List<Color>(EdiaColors.Values);
            return colorsList[Random.Range(0, colorsList.Count - 2)];
        }

        private static Color ParseColor(string hex, Color fallback) {
            if (ColorUtility.TryParseHtmlString(hex, out var colorResult)) {
                return colorResult;
            }

            return fallback;
        }

#endregion
    }
}