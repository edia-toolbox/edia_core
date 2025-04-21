using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Edia {
    /// <summary>Static definitions</summary>
    public static class Constants {
        public enum Sides {
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
            MainBG,
            PanelBG,
            SubPanelBG,
            MainText,
            PanelText,
            SubPanelText,
            ButtonText,
            Outline,
            MessagePanelBG,
            MessagePanelTextPanelBG,
            MessagePanelText,
            ProgressbarBG,
            ProgressbarFill,
            ProgressbarText
        }

        public static event System.Action OnThemeChanged;

        private static ThemeDefinition _activeTheme = null;

        public static ThemeDefinition ActiveTheme {
            get {
#if UNITY_EDITOR
                // Lazy initialization if no theme has been set yet
                if (_activeTheme == null) {
                    string themeGuid = EditorPrefs.GetString("EDIA_SelectedThemeGuid", "");
                    if (!string.IsNullOrEmpty(themeGuid)) {
                        string themePath = AssetDatabase.GUIDToAssetPath(themeGuid);
                        _activeTheme = AssetDatabase.LoadAssetAtPath<ThemeDefinition>(themePath);

                        if (_activeTheme == null) {
                            Debug.LogWarning("Failed to load the theme from EditorPrefs. Make sure the theme asset exists in the project.");
                        }
                    }

                    if (_activeTheme == null) {
                        _activeTheme = ScriptableObject.CreateInstance<ThemeDefinition>();
                        Debug.Log("Created default theme as none was set");
                    }
                }
#endif

                return _activeTheme;
            }
            set {
#if UNITY_EDITOR
                _activeTheme = value;

                if (OnThemeChanged != null) {
                    OnThemeChanged.Invoke();
                }
                else {
                    Debug.LogWarning("Theme changed but no listeners were registered");
                }
#endif
            }
        }

#endregion

#region EDIA hardcoded colors

        // EDIA defined system colors
        public static Dictionary<string, Color> EdiaColors = new Dictionary<string, Color>() {
            { "blue", ParseColor("#347FAA", Color.blue) },
            { "cyan", ParseColor("#34AAAA", Color.cyan) },
            { "green", ParseColor("#428360", Color.green) },
            { "grey", ParseColor("#797873", Color.grey) },
            { "orange", ParseColor("#D9740D", Color.white) },
            { "yellow", ParseColor("#FFDC4A", Color.yellow) },
            { "purple", ParseColor("#C36897", Color.magenta) },
            { "white", ParseColor("#F2F2F2", Color.white) },
            { "black", ParseColor("#0D0D0D", Color.black) }
        };

        public static Color RandomEdiaColor() {
            var colorsList = new List<Color>(EdiaColors.Values);
            return colorsList[Random.Range(0, colorsList.Count - 3)];
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