using UnityEngine;

public class XblockHeaderAttribute : PropertyAttribute {

	public string Label;
	public string Sublabel;
	
	public XblockHeaderAttribute(string label, string sublabel) {
		this.Label = label;
		this.Sublabel = sublabel;
	}
}
