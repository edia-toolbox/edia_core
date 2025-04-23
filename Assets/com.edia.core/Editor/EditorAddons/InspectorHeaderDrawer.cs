using UnityEngine;
using UnityEditor;

namespace Edia.Editor.Utils {

    [CustomPropertyDrawer(typeof(InspectorHeaderAttribute))]
    public class InspectorHeaderDrawer : DecoratorDrawer {

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

        private GUIStyle descriptionStyle = new GUIStyle {
            fontSize = 12,
            wordWrap = true,
            font     = Resources.Load<Font>("Bahnschrift-Regular"),
            normal   = { textColor = Constants.EdiaColors["grey"] }
        };

        public override float GetHeight() {
            return 100f; 
        }

        public override void OnGUI(Rect position) {
            var InspectorHeaderAttribute = attribute as InspectorHeaderAttribute;
            if (InspectorHeaderAttribute == null) return;

            Texture2D headerBG = Resources.Load<Texture2D>("EdiaHeader");
            GUI.Box(new Rect(0, 0, 1000, 100), GUIContent.none, new GUIStyle { normal = { background = headerBG } });
            GUI.color = Color.clear;
            
            string iconName = "IconEdia";
            if (InspectorHeaderAttribute.Label.ToUpper().Contains("EYE"))
                iconName = "IconEye";
            else if (InspectorHeaderAttribute.Label.ToUpper().Contains("LSL"))
                iconName = "IconLSL";
            else if (InspectorHeaderAttribute.Label.ToUpper().Contains("STREAM"))
                iconName = "IconStreamer";
            else if (InspectorHeaderAttribute.Label.ToUpper().Contains("RCAS"))
                iconName = "IconRCAS";
                
            Texture2D iconTexture = Resources.Load<Texture2D>(iconName);
            EditorGUI.DrawTextureTransparent(new Rect(18, 20, 60, 60), iconTexture, ScaleMode.ScaleToFit);
            GUI.color = Color.white;

            // Label
            EditorGUI.LabelField(
                new Rect(90, 34, 200, 40), InspectorHeaderAttribute.Label, labelStyle);

            // Sub label
            float textWidth = subLabelStyle.CalcSize(new GUIContent(InspectorHeaderAttribute.Sublabel)).x;
            EditorGUI.LabelField(
                new Rect(position.x - 6, position.y, Mathf.Max(position.width, textWidth + 20), 40), InspectorHeaderAttribute.Sublabel, subLabelStyle);

            Rect lineRect = new Rect(position.x, position.y + 50, position.width, 1);
            EditorGUI.DrawRect(lineRect, Constants.EdiaColors["grey"]);

            // Description
            float descriptionHeight = descriptionStyle.CalcHeight(new GUIContent(InspectorHeaderAttribute.Description), position.width);

            EditorGUI.LabelField(
                new Rect(position.x, position.y + 60, position.width, descriptionHeight), InspectorHeaderAttribute.Description, descriptionStyle);
        }
    }

}