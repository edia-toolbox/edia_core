using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Edia {
    [CreateAssetMenu(fileName = "ColorTheme", menuName = "EDIA/UI/Color Theme File")]
    public class ThemeDefinition : ScriptableObject {

#region Interactive Elements

        [Space(15)]
        [Header("Interactive Elements")]
        [Space(5)]
        [Header("Global Colors")]
        [Space(15)]
        [SerializeField] private ColorBlock _globalColorBlock = ColorBlock.defaultColorBlock;
        
        public ColorBlock GlobalColorBlock {
            get { return _globalColorBlock; }
            set { _globalColorBlock = value; }
        }
        
        [Space(5)]
        public Color PrimaryTextColor = Edia.Constants.EdiaColors["black"];
        public Color SecundaryTextColor = Edia.Constants.EdiaColors["white"];
        public Color TertiaryTextColor = Edia.Constants.EdiaColors["white"];
        
        [Space(5)]
        public Color OutlinesColor = Edia.Constants.EdiaColors["black"];
        
        [Space(15)]
        [Header("Progress bar colors")]
        [Tooltip("Progress bar static background.")]
        public Color progressBarBGColor = Edia.Constants.EdiaColors["grey"];
        [Tooltip("Animated progress.")]
        public Color progressBarFillColor = Edia.Constants.EdiaColors["blue"];
        [Tooltip("Color of textual data.")]
        public Color progressBarTextColor = Edia.Constants.EdiaColors["black"];
        
        [Space(15)]
        [Header("Timer bar colors")]
        public Color TimerBarBGColor = Edia.Constants.EdiaColors["grey"];
        public Color TimerBarFrontColor = Edia.Constants.EdiaColors["yellow"];
        
#endregion

#region Panels

        [Space(15)]
        public Color ControllerPanelColor = new Color(0.39f, 0.39f, 0.39f, 0.6f);
        public Color PanelColor = new Color(0.35f, 0.44f, 0.52f, 0.59f);
        public Color SubPanelColor = new Color(0.33f, 0.69f, 0.76f, 0.38f);

        [Space(15)]
        public Color MsgPanelColor = new Color(0.33f, 0.69f, 0.76f, 0.38f);
        public Color MsgPanelTextBGColor = new Color(0.78f, 0.78f, 0.78f);
        public Color MsgPanelTextColor = new Color(0.78f, 0.78f, 0.78f);
        
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
                PlayerPrefs.SetString(Constants.THEME_PATH_KEY, themePath);
                Constants.UpdateTheme();
            }
        }
    }
#endif
}