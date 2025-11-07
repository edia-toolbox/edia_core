using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MonoBehaviour), true)]
public class EdiaHeaderEditor : Editor {
    private EdiaHeaderAttribute headerAttribute;
    private Texture2D           headerBG;
    private Texture2D           iconTexture;
    private float               headerHeight = 120;
    private GUIStyle            labelStyle;
    private GUIStyle            subLabelStyle;
    private GUIStyle            descriptionStyle;

    private void OnEnable() {
        var type = target.GetType();
        headerAttribute = (EdiaHeaderAttribute)System.Attribute.GetCustomAttribute(type, typeof(EdiaHeaderAttribute));

        if (headerAttribute != null) {
            headerBG = Resources.Load<Texture2D>("Icons/EdiaHeader");

            // Icon
            string iconName = "IconEdia";
            if (headerAttribute.Title.ToUpper().Contains("EYE"))
                iconName = "IconEye";
            else if (headerAttribute.Title.ToUpper().Contains("LSL"))
                iconName = "IconLSL";
            else if (headerAttribute.Title.ToUpper().Contains("STREAM"))
                iconName = "IconStreamer";
            else if (headerAttribute.Title.ToUpper().Contains("RCAS"))
                iconName = "IconRCAS";

            iconTexture = Resources.Load<Texture2D>($"Icons/{iconName}");
        }

        // Styles
        labelStyle = new GUIStyle {
            fontSize  = 36,
            alignment = TextAnchor.UpperLeft,
            font      = Resources.Load<Font>("Fonts/Barlow/BarlowSemiCondensed-SemiBold"),
            normal    = { textColor = Edia.Constants.EdiaColors["white"] }
        };

        subLabelStyle = new GUIStyle {
            fontSize  = 22,
            alignment = TextAnchor.LowerRight,
            font      = Resources.Load<Font>("Fonts/Barlow/BarlowSemiCondensed-Medium"),
            normal    = { textColor = Edia.Constants.EdiaColors["grey"] }
        };

        descriptionStyle = new GUIStyle {
            fontSize = 12,
            wordWrap = true,
            font     = Resources.Load<Font>("Fonts/Barlow/BarlowSemiCondensed-Regular"),
            normal   = { textColor = Edia.Constants.EdiaColors["grey"] }
        };
    }

    public override void OnInspectorGUI() {
        if (headerAttribute != null) {
            DrawHeader(headerAttribute);
            GUILayout.Space(10);

            serializedObject.Update();
            SerializedProperty prop = serializedObject.GetIterator();

            bool enterChildren = true;
            while (prop.NextVisible(enterChildren)) {
                if (prop.name == "m_Script")
                    continue;
                EditorGUILayout.PropertyField(prop, true);
                enterChildren = false;
            }

            serializedObject.ApplyModifiedProperties();
        }
        else {
            base.OnInspectorGUI();
        }
    }

    private void DrawHeader(EdiaHeaderAttribute header) {
        Rect rect = EditorGUILayout.GetControlRect(false, headerHeight);

        // BG Texture
        GUI.Box(new Rect(0, 0, 1000, 100), GUIContent.none, new GUIStyle { normal = { background = headerBG } });
        GUI.color = Color.clear;

        EditorGUI.DrawTextureTransparent(new Rect(18, 20, 60, 60), iconTexture, ScaleMode.ScaleToFit);
        GUI.color = Color.white;

        // Title
        EditorGUI.LabelField(new Rect(90, 34, 200, 40), header.Title, labelStyle);

        // Subtitle
        float textWidth = subLabelStyle.CalcSize(new GUIContent(header.Subtitle)).x;
        EditorGUI.LabelField(new Rect(rect.width - textWidth - 22 + rect.x, 70, textWidth + 20, 22), header.Subtitle, subLabelStyle);

        // Line
        Rect lineRect = new Rect(rect.x, rect.y + headerHeight - 24, rect.width, 1);
        EditorGUI.DrawRect(lineRect, Edia.Constants.EdiaColors["grey"]);

        // Description
        EditorGUI.LabelField(new Rect(18, rect.height - 14, rect.width, 22), header.Description, descriptionStyle);
    }
}