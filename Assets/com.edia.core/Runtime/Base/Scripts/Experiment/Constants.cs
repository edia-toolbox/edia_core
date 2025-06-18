using System.Collections.Generic;
using System.Linq;
using Edia.Utilities;
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
        public static string FileNameSession             = "session.json";

        public static string FileNameEdiaSettings = "Edia-settings.json";

        // Other
        public static string MsgPanelLayerName = "MsgPanelUI";
        
        
#region Color Theme

        
        public enum ThemeComponents {
            Button,
            Toggle,
            Slider,
            Dropdown,
            CTRLPanel,
            Panel,
            SubPanel,
            PrimaryText,
            SecundaryText,
            TertiaryText,
            Outlines,
            MgsPanelBG,
            MsgPanelTextBG,
            MsgPanelText,
            ProgressbarBG,
            ProgressbarFill,
            ProgressbarText,
            HorizontalTimer,
            Scrollbar
        }

#if UNITY_EDITOR

        private const string DefaultThemePath = "DefaultColorTheme";
        
        public static ThemeDefinition ActiveTheme { 
            get {
                if (_activeTheme is not null) return _activeTheme;
                _activeTheme = Resources.Load<ThemeDefinition>(DefaultThemePath);
                if (_activeTheme is null) {
                    Debug.LogWarning($"Default Theme not found at Resources/{DefaultThemePath}");
                }
                return _activeTheme;
            }
            private set => _activeTheme = value;
        }
        
        private static ThemeDefinition    _activeTheme;
        private static List<ThemeHandler> _themeHandlers = new();
         
        public static void ApplyTheme(string themePath) {
            _activeTheme = AssetDatabase.LoadAssetAtPath<ThemeDefinition>(themePath);
            Debug.Log($"Applying theme {ActiveTheme}");
            
            _themeHandlers = GameObject.FindObjectsByType<ThemeHandler>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
            foreach (var handler in _themeHandlers) {
                handler.ApplyTheme(ActiveTheme);
            }
        }
#endif
        
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