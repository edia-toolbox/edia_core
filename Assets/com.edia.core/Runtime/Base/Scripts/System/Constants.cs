using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Edia {
    /// <summary>Theme and color definitions</summary>
    public static partial class Constants {

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
            _activeTheme = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeDefinition>(themePath);
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
