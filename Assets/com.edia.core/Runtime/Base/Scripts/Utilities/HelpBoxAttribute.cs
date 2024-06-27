using UnityEngine;

public enum HelpBoxMessageType { None, Info, Warning, Error }

public class HelpBoxAttribute : PropertyAttribute {

	public string text;
	public HelpBoxMessageType messageType;

	public HelpBoxAttribute(string text, HelpBoxMessageType messageType = HelpBoxMessageType.None) {
		this.text = text;
		this.messageType = messageType;
	}
}
