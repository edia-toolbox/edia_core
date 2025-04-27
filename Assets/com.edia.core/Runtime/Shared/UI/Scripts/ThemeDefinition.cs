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
        [Header("Button colors")]
        [SerializeField] private ColorBlock _buttonColorBlock = ColorBlock.defaultColorBlock;

        public ColorBlock ButtonColorBlock {
            get { return _buttonColorBlock; }
            set { _buttonColorBlock = value; }
        }

        public Color ButtonTextColor = Edia.Constants.EdiaColors["grey"];
        public Color ButtonOutlineColor = Edia.Constants.EdiaColors["grey"];
        
        [Space(15)]
        [Header("Toggle colors")]
        [SerializeField] private ColorBlock _toggleColorBlock = ColorBlock.defaultColorBlock;

        public ColorBlock ToggleColorBlock {
            get { return _toggleColorBlock; }
            set { _toggleColorBlock = value; }
        }
        
        [Space(15)]
        [Header("Dropdown colors")]
        [SerializeField] private ColorBlock _dropDownColorBlock = ColorBlock.defaultColorBlock;

        public ColorBlock DropDownColorBlock {
            get { return _dropDownColorBlock; }
            set { _dropDownColorBlock = value; }
        }
        
        [Space(15)]
        [Header("Progress bar colors")]
        public Color progressBarBGColor = Edia.Constants.EdiaColors["grey"];
        public Color progressBarFillColor = Edia.Constants.EdiaColors["blue"];
        public Color progressBarTextColor = Edia.Constants.EdiaColors["black"];
        
#endregion

#region Panel Colors

        [Space(15)]
        public Color MainBGColor = new Color(0.39f, 0.39f, 0.39f, 0.6f);
        public Color MainPanelTextColor = new Color(0.76f, 0.76f, 0.76f);

        [Space(15)]
        public Color PanelBGColor = new Color(0.35f, 0.44f, 0.52f, 0.59f);
        public Color PanelTextColor = new Color(0.75f, 0.75f, 0.75f);
        
        [Space(15)]
        public Color SubPanelBGColor = new Color(0.33f, 0.69f, 0.76f, 0.38f);
        public Color SubPanelTextColor = new Color(0.78f, 0.78f, 0.78f);

        [Space(15)]
        public Color MsgPanelBGColor = new Color(0.33f, 0.69f, 0.76f, 0.38f);
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
            if (GUILayout.Button("Apply")) {
                Constants.ActiveTheme = theme;
            }
        }
    }
#endif
}