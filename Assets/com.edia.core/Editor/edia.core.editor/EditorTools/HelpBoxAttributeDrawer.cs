using UnityEngine;
using UnityEditor;
using Edia;

[CustomPropertyDrawer(typeof(HelpBoxAttribute))]
public class HelpBoxAttributeDrawer : DecoratorDrawer {
	// TODO rename this to InfoBox or something.
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

		GUIStyle style = new GUIStyle(EditorStyles.helpBox);
		style.richText = true;
		GUI.color = Color.clear;
		EditorGUI.DrawTextureTransparent(new Rect(position.x + 12, position.y + 20, 40, 40), GetMessageType(helpBoxAttribute.messageType));
		GUI.color = Color.white;
		EditorGUI.TextArea(new Rect(position.x + 70, position.y + 10, position.width - 80, 60), helpBoxAttribute.text, style);
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
