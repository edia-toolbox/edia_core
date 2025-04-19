using UnityEngine;

public class InspectorHeaderAttribute : PropertyAttribute {

	public string Label;
	public string Sublabel;
	public string Description;
	
	public InspectorHeaderAttribute(string label, string sublabel, string description) {
		this.Label = label;
		this.Sublabel = sublabel;
		this.Description = description;
	}
}
