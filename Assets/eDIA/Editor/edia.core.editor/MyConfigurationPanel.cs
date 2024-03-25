using UnityEngine;
using UnityEngine.UI;

public class MyConfigurationPanel : MonoBehaviour {
	// Reference to the UI text component
	public Text textComponent;

	// Static method to open the configuration panel
	public static void Open() {
		// Instantiate the configuration panel prefab
		GameObject panelPrefab = Resources.Load<GameObject>("MyConfigurationPanel");
		if (panelPrefab != null) {
			Instantiate(panelPrefab);
		}
		else {
			Debug.LogError("MyConfigurationPanel prefab not found in Resources folder.");
		}
	}

	// Start is called before the first frame update
	void Start() {
		// Update the text component with your desired text
		if (textComponent != null) {
			textComponent.text = "This is my configuration panel!";
		}
		else {
			Debug.LogError("Text component reference not set in MyConfigurationPanel.");
		}
	}
}