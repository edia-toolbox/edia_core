using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MonoBehaviour), true)]
public class EdiaHeaderEditor : Editor {
    private EdiaHeaderAttribute headerAttribute;
    private Texture2D           headerBG;

    private void OnEnable() {
        // Check if the inspected object’s script/class has the attribute
        var type = target.GetType();
        headerAttribute = (EdiaHeaderAttribute)System.Attribute.GetCustomAttribute(type, typeof(EdiaHeaderAttribute));

        // Only load or assign your header background if attribute exists
        if (headerAttribute != null)
            headerBG = Resources.Load<Texture2D>("Icons/EdiaHeader");
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

        float headerHeight = 120;

        var labelStyle = new GUIStyle {
            fontSize  = 36,
            alignment = TextAnchor.UpperLeft,
            font      = Resources.Load<Font>("Fonts/Bahnschrift-BoldSemiCondensed"),
            normal    = { textColor = Edia.Constants.EdiaColors["white"] }
        };

        var subLabelStyle = new GUIStyle {
            fontSize  = 22,
            alignment = TextAnchor.LowerRight,
            font      = Resources.Load<Font>("Fonts/Bahnschrift-Condensed"),
            normal    = { textColor = Edia.Constants.EdiaColors["grey"] }
        };

        var descriptionStyle = new GUIStyle {
            fontSize = 12,
            wordWrap = true,
            font     = Resources.Load<Font>("Fonts/Bahnschrift-Regular"),
            normal   = { textColor = Edia.Constants.EdiaColors["grey"] }
        };

        // Full rect
        Rect rect = EditorGUILayout.GetControlRect(false, headerHeight);
        
        // BG Texture
        GUI.Box(new Rect(0, 0, 1000, 100), GUIContent.none, new GUIStyle { normal = { background = headerBG } }); 
        GUI.color = Color.clear;

        // Icon
        string iconName = "IconEdia";
        if (header.Title.ToUpper().Contains("EYE"))
            iconName = "IconEye";
        else if (header.Title.ToUpper().Contains("LSL"))
            iconName = "IconLSL";
        else if (header.Title.ToUpper().Contains("STREAM"))
            iconName = "IconStreamer";
        else if (header.Title.ToUpper().Contains("RCAS"))
            iconName = "IconRCAS";

        Texture2D iconTexture = Resources.Load<Texture2D>($"Icons/{iconName}");
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