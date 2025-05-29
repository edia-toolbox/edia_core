using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(InspectorHelpBoxAttribute))]
public class InspectorHelpBoxDrawer : PropertyDrawer {

    private static Texture2D customIcon;
    private static GUIStyle  style;
    
    static InspectorHelpBoxDrawer() {
        customIcon = Resources.Load<Texture2D>("Icons/IconEdia");
        
        style                  = new GUIStyle(EditorStyles.wordWrappedLabel);
        style.richText         = true;
        style.alignment        = TextAnchor.MiddleLeft;
        style.font             = Resources.Load<Font>("Fonts/Bahnschrift-BoldSemiCondensed");
        style.fontSize         = 14;
        style.normal.textColor = Edia.Constants.EdiaColors["white"];
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var helpBoxAttribute = attribute as InspectorHelpBoxAttribute;
        var helpBoxRect      = position;
        helpBoxRect.height = GetHelpBoxHeight(helpBoxAttribute.message);

        if (helpBoxAttribute.type != MessageType.None) {
            EditorGUI.HelpBox(helpBoxRect, helpBoxAttribute.message, helpBoxAttribute.type);
        }
        else {
            DrawCustomHelpBox(helpBoxRect, helpBoxAttribute.message);
        }
        
        var propertyRect = position;
        propertyRect.y      += helpBoxRect.height + EditorGUIUtility.standardVerticalSpacing;
        propertyRect.height =  EditorGUI.GetPropertyHeight(property, label, true);

        EditorGUI.PropertyField(propertyRect, property, true);
    }

    private void DrawCustomHelpBox(Rect position, string message) {
        GUI.Box(position, "", EditorStyles.helpBox);

        float iconSize = 32f;
        float padding  = 6f;

        Rect iconRect = new Rect(
            position.x + padding,
            position.y + padding,
            iconSize,
            iconSize
        );

        Rect textRect = new Rect(
            position.x + iconSize + padding * 2,
            position.y + padding,
            position.width - iconSize - padding * 3,
            position.height - padding * 2
        );
        GUI.DrawTexture(iconRect, customIcon);
        
        EditorGUI.LabelField(textRect, message, style);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        var   helpBoxAttribute = attribute as InspectorHelpBoxAttribute;
        float helpBoxHeight    = GetHelpBoxHeight(helpBoxAttribute.message);
        float propertyHeight   = EditorGUI.GetPropertyHeight(property, label, true);

        return helpBoxHeight + EditorGUIUtility.standardVerticalSpacing + propertyHeight;
    }

    private float GetHelpBoxHeight(string message) {
        float width   = EditorGUIUtility.currentViewWidth - 20; // account for margins
        var   content = new GUIContent(message);
        var   height  = style.CalcHeight(content, width);
        return height + 30; // padding
    }
}