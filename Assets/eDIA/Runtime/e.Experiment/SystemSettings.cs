using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA {

	/// <summary>Global settings of the application</summary>
	public class SystemSettings : MonoBehaviour {

#region DECLARATIONS

		/// <summary>Instance of the Settings declaration class in order to (de)serialize to JSON</summary>
		public SettingsDeclaration systemSettings = new SettingsDeclaration();
		static SettingsDeclaration receivedSettings = new SettingsDeclaration();

		static UXF.LocalFileDataHander UXFFilesaver = null;

		private void Awake() {

			InitSystemSettings();
		}		



#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region MAIN METHODS

		/// <summary>Gets called from XRrigmanager to init the system. </summary>
		public void InitSystemSettings () {

			UXFFilesaver = GameObject.FindObjectOfType<UXF.LocalFileDataHander>();

			// Listen to update settings requests
			EventManager.StartListening(eDIA.Events.Settings.EvUpdateSystemSettings, OnEvUpdateSystemSettings);
			EventManager.StartListening(eDIA.Events.Settings.EvRequestSystemSettings, OnEvRequestSystemSettings);
			
			// Set time and location to avoid comma / period issues
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

			// Any settings on disk? > load them
			LoadSettings();
		}

		void SaveSettings () {
			FileManager.WriteString("settings.json", UnityEngine.JsonUtility.ToJson(systemSettings,true), true);
		}

		async void LoadSettings () {

			if (!FileManager.FileExists("settings.json")) {
				Debug.Log("Settings file not found, saving defaults");
				SaveSettings();
				return;
			}

			string loadedSettings = FileManager.ReadStringFromApplicationPath("settings.json");
			
			await Task.Delay(500); // 1 second delay

			//! Send with event so it can go over the network to the manager
			EventManager.TriggerEvent(eDIA.Events.Settings.EvUpdateSystemSettings, new eParam(loadedSettings));

			//! Locally
			OnEvUpdateSystemSettings(new eParam(loadedSettings));
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EVENT LISTENERS

		public void OnEvUpdateSystemSettings (eParam obj) {
			
			receivedSettings = new SettingsDeclaration();
			receivedSettings = UnityEngine.JsonUtility.FromJson<SettingsDeclaration>(obj.GetString());

			systemSettings.VisableInteractor = receivedSettings.VisableInteractor;
			EventManager.TriggerEvent(eDIA.Events.XR.EvUpdateVisableInteractor, new eParam((int)receivedSettings.VisableInteractor));

			systemSettings.InteractiveInteractor = receivedSettings.InteractiveInteractor;
			EventManager.TriggerEvent(eDIA.Events.XR.EvUpdateInteractiveInteractor, new eParam((int)receivedSettings.InteractiveInteractor));

			// Resolution of the app
			if (systemSettings.screenResolution != receivedSettings.screenResolution) {
				systemSettings.screenResolution = receivedSettings.screenResolution;
				
				//TODO Change actual value
			}

			// Save Path for logfiles
			systemSettings.pathToLogfiles = receivedSettings.pathToLogfiles;
			
			EventManager.TriggerEvent(eDIA.Events.Settings.EvSetCustomStoragePath, new eParam(receivedSettings.pathToLogfiles));
			UXFFilesaver.storagePath = systemSettings.pathToLogfiles;

			// Volume of the app
			if (systemSettings.volume != receivedSettings.volume) {
				systemSettings.volume = receivedSettings.volume;
				
				//TODO Change actual value
			}

			// language 
			if (systemSettings.language != receivedSettings.language) {
				systemSettings.language = receivedSettings.language;
				
				//TODO Change actual value 
			}
			
			SaveSettings();
		}

		/// <summary> Catches request to show system settings, collects them and send them out with a OPEN settings panel event. </summary>
		private void OnEvRequestSystemSettings(eParam obj)
		{
			EventManager.TriggerEvent(eDIA.Events.Settings.EvOpenSystemSettings, new eParam( GetSettingsAsJSONstring()));
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HELPERS

		/// <summary>Gets all settings from the 'SettingsDeclaration' instance 'systemSettings' as a JSON string</summary>
		/// <returns>JSON string</returns>
		public string GetSettingsAsJSONstring () {
			return UnityEngine.JsonUtility.ToJson(systemSettings,false);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------

	}
}