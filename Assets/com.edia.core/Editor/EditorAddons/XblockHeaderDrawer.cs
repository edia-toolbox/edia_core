using UnityEngine;
using UnityEditor;

namespace Edia.Editor.Utils {

    [CustomPropertyDrawer(typeof(XblockHeaderAttribute))]
    public class XblockHeaderDrawer : DecoratorDrawer {

        private GUIStyle labelStyle = new GUIStyle {
            fontSize = 22,
            font     = Resources.Load<Font>("Bahnschrift-BoldSemiCondensed"),
            normal   = { textColor = Constants.EdiaColors["white"] }
        };

        public override float GetHeight() {
            return 30f; 
        }

        public override void OnGUI(Rect position) {
            var xblockHeaderAttribute = attribute as XblockHeaderAttribute;
            if (xblockHeaderAttribute == null) return;

            Color backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f);
            EditorGUI.DrawRect(new Rect(0, 20, position.width, GetHeight()), backgroundColor);
            
            GUI.color = Color.clear;
            Texture2D iconTexture = Resources.Load<Texture2D>("IconEdia");
            EditorGUI.DrawTextureTransparent(new Rect(20,30, 30, 30), iconTexture, ScaleMode.ScaleToFit);
            GUI.color = Color.white;

            EditorGUI.LabelField(
                new Rect(60, 36, 200, 26), xblockHeaderAttribute.Label, labelStyle);
        }
    }
 
}
