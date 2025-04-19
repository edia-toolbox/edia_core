using UnityEngine;
using UnityEditor;

namespace Edia.Editor.Utils {

    [CustomPropertyDrawer(typeof(XblockHeaderAttribute))]
    public class XblockHeaderDrawer : DecoratorDrawer {

        private GUIStyle labelStyle = new GUIStyle {
            fontSize = 36,
            font     = Resources.Load<Font>("Bahnschrift-BoldSemiCondensed"),
            normal   = { textColor = Constants.EdiaColors["white"] }
        };

        private GUIStyle subLabelStyle = new GUIStyle {
            fontSize  = 22,
            alignment = TextAnchor.LowerRight,
            font      = Resources.Load<Font>("Bahnschrift-Condensed"),
            normal    = { textColor = Constants.EdiaColors["grey"] }
        };

        public override float GetHeight() {
            return 50f; 
        }

        public override void OnGUI(Rect position) {
            var xblockHeaderAttribute = attribute as XblockHeaderAttribute;
            if (xblockHeaderAttribute == null) return;

            Color backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f);
            EditorGUI.DrawRect(new Rect(0, 20, position.width, GetHeight()), backgroundColor);
            
            GUI.color = Color.clear;
            Texture2D iconTexture = Resources.Load<Texture2D>("IconEdia");
            EditorGUI.DrawTextureTransparent(new Rect(15,30, 40, 40), iconTexture, ScaleMode.ScaleToFit);
            GUI.color = Color.white;

            EditorGUI.LabelField(
                new Rect(65, 34, 200, 30), xblockHeaderAttribute.Label, labelStyle);

            // Sub label
            float textWidth = subLabelStyle.CalcSize(new GUIContent(xblockHeaderAttribute.Sublabel)).x;
            EditorGUI.LabelField(
                new Rect(position.x - 6, position.y, Mathf.Max(position.width, textWidth + 20), 30), xblockHeaderAttribute.Sublabel, subLabelStyle);

            Rect lineRect = new Rect(position.x, position.y + 30, position.width, 1);
            EditorGUI.DrawRect(lineRect, Constants.EdiaColors["grey"]);

        }
    }
 
}
