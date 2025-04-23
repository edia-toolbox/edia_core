using UnityEngine;

public class XblockHeaderAttribute : PropertyAttribute {

	public string Label;
	// public string Sublabel;
	
	public XblockHeaderAttribute(string label) {
		this.Label = label;
		// this.Sublabel = sublabel;
	}
}
