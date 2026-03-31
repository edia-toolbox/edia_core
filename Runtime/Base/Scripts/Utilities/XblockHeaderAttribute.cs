using UnityEngine;

public class XblockHeaderAttribute : PropertyAttribute {

	public string Label;
	
	public XblockHeaderAttribute(string label) {
		this.Label = label;
	}
}
