using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Edia.Controller;
using UnityEngine;
using UXF;

namespace Edia {

	/// <summary>Global settings of the application</summary>
	public class SystemSettings : Singleton<SystemSettings> {

#region DECLARATIONS

		/// <summary>Instance of the Settings declaration class in order to (de)serialize to JSON</summary>
		public SettingsDeclaration systemSettings = new SettingsDeclaration();
		static SettingsDeclaration receivedSettings = new SettingsDeclaration();

		static UXF.LocalFileDataHander UXFFilesaver = null;

		public bool isRemote = false;
		
		private void Awake() {

			InitSystemSettings();
		}		


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region MAIN METHODS

		public void InitSystemSettings () {

			UXFFilesaver = GameObject.FindObjectOfType<UXF.LocalFileDataHander>();

			// Listen to update settings requests
			EventManager.StartListening(Edia.Events.Settings.EvUpdateSystemSettings, OnEvUpdateSystemSettings);
			EventManager.StartListening(Edia.Events.Settings.EvRequestSystemSettings, OnEvRequestSystemSettings);
			
			// Set time and location to avoid comma / period issues
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

			// If there is no controlpanel in memory, we must be remote
			// isRemote = ControlPanel.Instance == null;
			
			// Any settings on disk? > load them
			LoadSettings();
		}

		void SaveSettings () {
			FileManager.WriteString("Edia-settings", UnityEngine.JsonUtility.ToJson(systemSettings,true), true);
		}

		async void LoadSettings () {

			if (!FileManager.FileExists("Edia-settings.json")) {
				Debug.Log("Settings file not found, saving defaults");
				SaveSettings();
				return;
			}

			string loadedSettings = FileManager.ReadStringFromApplicationPath("Edia-settings.json");
			
			await Task.Delay(500); //  delay

			//! Send with event so it can go over the network to the controlpanel
			EventManager.TriggerEvent(Edia.Events.Settings.EvProvideSystemSettings, new eParam(loadedSettings));

			//! Locally
			OnEvUpdateSystemSettings(new eParam(loadedSettings));
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EVENT LISTENERS

		public void OnEvUpdateSystemSettings (eParam obj) {
			
			receivedSettings = new SettingsDeclaration();
			receivedSettings = UnityEngine.JsonUtility.FromJson<SettingsDeclaration>(obj.GetString());

			systemSettings.VisibleSide = receivedSettings.VisibleSide;
			EventManager.TriggerEvent(Edia.Events.XR.EvUpdateVisableSide, new eParam((int)receivedSettings.VisibleSide));

			systemSettings.InteractiveSide = receivedSettings.InteractiveSide;
			EventManager.TriggerEvent(Edia.Events.XR.EvUpdateInteractiveSide, new eParam((int)receivedSettings.InteractiveSide));

			// Save Path for logfiles
			systemSettings.pathToLogfiles = receivedSettings.pathToLogfiles;
			
			EventManager.TriggerEvent(Edia.Events.Settings.EvSetCustomStoragePath, new eParam(receivedSettings.pathToLogfiles));

			if (isRemote)
				UXFFilesaver.storagePath = Application.dataPath; 
			else
				UXFFilesaver.storagePath = systemSettings.pathToLogfiles;

			SaveSettings();
		}

		/// <summary> Catches request to show system settings, collects them and send them out with a OPEN settings panel event. </summary>
		private void OnEvRequestSystemSettings(eParam obj)
		{
			EventManager.TriggerEvent(Edia.Events.Settings.EvProvideSystemSettings, new eParam( GetSettingsAsJSONstring()));
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