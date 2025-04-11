using UnityEngine;
using UnityEditor;

namespace Edia.Editor.Utils {

    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxAttributeDrawer : DecoratorDrawer {
        
        // // TODO rename this to InfoBox or something.
        public override float GetHeight() {
            try {
                var helpBoxAttribute = attribute as HelpBoxAttribute;
                if (helpBoxAttribute == null) return base.GetHeight();
                var helpBoxStyle = (GUI.skin != null) ? GUI.skin.GetStyle("helpbox") : null;
                if (helpBoxStyle == null) return base.GetHeight();
                return Mathf.Max(80f, helpBoxStyle.CalcHeight(new GUIContent(helpBoxAttribute.text), EditorGUIUtility.currentViewWidth) + 4);
            }
            catch (System.ArgumentException) {
                return 3 * EditorGUIUtility.singleLineHeight; // Handle Unity 2022.2 bug by returning default value.
            }
        }

        public override void OnGUI(Rect position) {
            var helpBoxAttribute = attribute as HelpBoxAttribute;
            if (helpBoxAttribute == null) return;
            
            var originalColor = GUI.color;
            GUI.color = Constants.EdiaColors["Blue"];
            GUI.Box(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 80), GUIContent.none, new GUIStyle { normal = { background = Texture2D.whiteTexture } });
            // GUI.color = Constants.EdiaColors["Grey"];
            // GUI.Box(new Rect(0, 81, EditorGUIUtility.currentViewWidth, 30), GUIContent.none, new GUIStyle { normal = { background = Texture2D.whiteTexture } });
            GUI.color = originalColor;
        
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.richText = true;
            GUI.color      = Color.clear;
            
            EditorGUI.DrawTextureTransparent(new Rect(20, 20, 40, 40), GetMessageType(helpBoxAttribute.messageType), ScaleMode.ScaleToFit);
            GUI.color = Color.white;
            EditorGUI.LabelField(
                new Rect(70, 23, 120, 40), 
                "EXPERIMENT", new GUIStyle { 
                    fontSize = 36, 
                    font = Resources.Load<Font>("Bahnschrift"), 
                    normal = { textColor = Constants.EdiaColors["White"] } }
                );
            
            EditorGUI.TextArea(new Rect(10, 90, EditorGUIUtility.currentViewWidth - 10, 60), helpBoxAttribute.text, style);
        }

        private Texture GetMessageType(HelpBoxMessageType helpBoxMessageType) {
            switch (helpBoxMessageType) {
                default:
                case HelpBoxMessageType.None: return Resources.Load<Texture>("IconEdia");
                case HelpBoxMessageType.Info: return Resources.Load<Texture>("IconInfo");
                case HelpBoxMessageType.Warning: return Resources.Load<Texture>("IconWarning");
                case HelpBoxMessageType.Error: return Resources.Load<Texture>("IconError");
            }
        }
    }
}