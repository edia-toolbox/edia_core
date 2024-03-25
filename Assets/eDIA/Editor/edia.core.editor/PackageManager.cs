using UnityEditor.PackageManager.UI;
using UnityEditor.PackageManager;
using UnityEngine.UIElements;


public class MyPackageManagerExtension : IPackageManagerExtension {

	public void OnPackageSelectionChange(string packageId) {
		// Implement any logic you need when package selection changes
	}

	public void OnPackageAddedOrUpdated(PackageInfo packageInfo) {
		// Open your custom configuration panel when the package is added or updated
		MyConfigurationPanel.Open();
	}

	public void OnPackageRemoved(PackageInfo packageInfo) {
		// Implement any logic you need when the package is removed
	}

	public VisualElement CreateExtensionUI() {
		throw new System.NotImplementedException();
	}

	public void OnPackageSelectionChange(PackageInfo packageInfo) {
		throw new System.NotImplementedException();
	}
}