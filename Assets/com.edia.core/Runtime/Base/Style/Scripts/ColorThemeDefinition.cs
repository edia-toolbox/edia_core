using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Edia {
    [CreateAssetMenu(fileName = "ColorTheme", menuName = "EDIA/UI/Color Theme File")]
    public class ColorThemeDefinition : ScriptableObject {

        #region Branding Elements
        [Header("Branding")]
        [Tooltip("The logo displayed in UI elements")]
        public Sprite logoSprite = null;
        
        [Tooltip("Primary brand color")]
        public Color primaryBrandColor = new Color(0.2f, 0.6f, 0.8f);
        
        [Tooltip("Secondary brand color")]
        public Color secondaryBrandColor = new Color(0.8f, 0.2f, 0.2f);
        #endregion

        #region Interactive Elements
        [Space(15)]
        [Header("Interactive Elements")]
        
        [Tooltip("Colors for buttons in different states")]
        [SerializeField] private ColorBlock buttonColorBlock = ColorBlock.defaultColorBlock;
        
        // Property with custom getter/setter if needed
        public ColorBlock ButtonColorBlock {
            get { return buttonColorBlock; }
            set { buttonColorBlock = value; }
        }
        
        [Tooltip("Toggle colors")]
        public ColorBlock ToggleColorBlock = ColorBlock.defaultColorBlock;
        #endregion

        #region Panel Colors
        [Space(15)]
        [Header("Panel Colors")]
        
        [Tooltip("Main control panel background color")]
        public Color ControlPanelBackgroundColor = Color.white;
        
        [Tooltip("Secondary panel background color")]
        public Color SubPanelBackgroundColor = Color.white;
        
        [Tooltip("Panel color for individual item containers")]
        public Color ItemPanelBackgroundColor = Color.white;
        #endregion

        #region Text Styling
        [Space(15)]
        [Header("Text Styling")]
        
        [Tooltip("Color for primary headings")]
        public Color HeaderTextColor = Color.black;
        
        [Tooltip("Color for body text")]
        public Color BodyTextColor = Color.black;
        
        [Range(0.5f, 2f)]
        [Tooltip("Text scale factor")]
        public float TextScaleFactor = 1.0f;
        #endregion

#if UNITY_EDITOR
        // This helps preview changes in the editor immediately
        private void OnValidate() {
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

#if UNITY_EDITOR
    // Custom editor for even more control (optional)
    [CustomEditor(typeof(ColorThemeDefinition))]
    public class ColorThemeDefinitionEditor : Editor {
        public override void OnInspectorGUI() {
            ColorThemeDefinition theme = (ColorThemeDefinition)target;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("Configure the visual theme colors for your UI here.", MessageType.Info);
            
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Reset to Default Colors")) {
                // Reset logic here
                theme.ButtonColorBlock = ColorBlock.defaultColorBlock;
                theme.ControlPanelBackgroundColor = Color.white;
                theme.SubPanelBackgroundColor = Color.white;
                theme.ItemPanelBackgroundColor = Color.white;
                EditorUtility.SetDirty(theme);
            }
        }
    }
#endif
}