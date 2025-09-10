using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Edia {
    [CreateAssetMenu(fileName = "ColorTheme", menuName = "EDIA/Color Theme File")]
    [EdiaHeader("EDIA THEME", "CUSTOM COLOR THEME", "Configure the visual theme colors for your UI here.")]
    public class ThemeDefinition : ScriptableObject {

#region Interactive Elements

        [Header("Color Theme ")]
        [Space(15)]
        [Header("Color block applied to interactive elements")]
        public ColorBlock GlobalColorBlock = new ColorBlock {
            normalColor      = Color.white,
            highlightedColor = new Color(0.36f, 0.66f, 0.87f),
            pressedColor     = new Color(0.63f, 0.84f, 0.96f),
            selectedColor    = new Color(0.82f, 0.44f, 0.05f),
            disabledColor    = new Color(0.7f, 0.7f, 0.7f, 0.5f),
            colorMultiplier  = 1f,
            fadeDuration     = 0.1f
        };

        [Header("Text")]
        [Space(5)]
        [Tooltip("Main text color used for important UI elements and headers")]
        public Color PrimaryTextColor = Edia.Constants.EdiaColors["black"];

        [Tooltip("Secondary text color used for subtitles and less prominent text")]
        public Color SecundaryTextColor = Edia.Constants.EdiaColors["white"];

        [Tooltip("Tertiary text color used for additional information and details")]
        public Color TertiaryTextColor = Edia.Constants.EdiaColors["white"];

        [Header("Outlines")]
        [Space(5)]
        [Tooltip("Color used for UI element borders and outlines")]
        public Color OutlinesColor = Edia.Constants.EdiaColors["black"];

        [Space(15)]
        [Header("Progress bar")]
        [Tooltip("Progress bar static background.")]
        public Color progressBarBGColor = Edia.Constants.EdiaColors["grey"];
        [Tooltip("Animated progress.")]
        public Color progressBarFillColor = new Color(0.82f, 0.44f, 0.05f);
        [Tooltip("Textual data.")]
        public Color progressBarTextColor = Edia.Constants.EdiaColors["white"];

        [Space(15)]
        [Header("Slider bar")]
        [Tooltip("The slider colorblock will use the GlobalColorBLock")]
        public Color SliderBarBGColor = Edia.Constants.EdiaColors["grey"];
        public Color SliderBarFillColor = new Color(0.82f, 0.44f, 0.05f);

        [Space(15)]
        [Header("Timer bar")]
        public Color TimerBarBGColor = Edia.Constants.EdiaColors["grey"];
        public Color TimerBarFrontColor = new Color(0.82f, 0.44f, 0.05f);

#endregion

#region Panels
        [Header("Panels")]
        [Space(15)]
        public Color ControllerPanelColor = new Color(0.11f, 0.15f, 0.17f, 0.95f);
        public Color PanelColor    = new Color(0.29f, 0.39f, 0.44f, 0.6f);
        public Color SubPanelColor = new Color(0.44f, 0.53f, 0.58f, 0.4f);

        [Space(15)]
        public Color MsgPanelColor = new Color(0.29f, 0.39f, 0.44f, 0.8f);
        public Color MsgPanelTextBGColor = new Color(0.78f, 0.78f, 0.8f);
        public Color MsgPanelTextColor   = new Color(0.04f, 0.04f, 0.04f);

#endregion

#if UNITY_EDITOR
        private void OnValidate() {
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

#if UNITY_EDITOR
    // Custom editor for even more control (optional)
    [CustomEditor(typeof(ThemeDefinition))]
    public class ColorThemeDefinitionEditor : Editor {

        public override void OnInspectorGUI() {
            ThemeDefinition theme = (ThemeDefinition)target;
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("Configure the visual theme colors for your UI here.", MessageType.Info);

            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Apply Theme")) {
                string themePath = AssetDatabase.GetAssetPath(theme);
                Constants.ApplyTheme(themePath);
            }
        }
    }
#endif
}