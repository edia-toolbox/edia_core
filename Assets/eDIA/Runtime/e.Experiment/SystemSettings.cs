using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA {

	/// <summary>Global settings of the application</summary>
	public static class SystemSettings {

#region DECLARATIONS

		/// <summary>Instance of the Settings declaration class in order to (de)serialize to JSON</summary>
		public static SettingsDeclaration systemSettings = new SettingsDeclaration();

		
#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region MAIN METHODS

		/// <summary>Gets called from XRrigmanager to init the system. </summary>
		public static void InitSystemSettings () {

			// Listen to update settings requests
			EventManager.StartListening(eDIA.Events.Core.EvUpdateSystemSettings, OnEvUpdateSystemSettings);
			EventManager.StartListening(eDIA.Events.Core.EvRequestSystemSettings, EvRequestSystemSettings);
			
			// systemSettings.language = Constants.Languages.DU;
			// systemSettings.volume = 30f;
			// systemSettings.primaryInteractor = Constants.PrimaryInteractor.RIGHTHANDED;
			// systemSettings.screenResolution = Constants.screenResolutions[0];

			// Any settings on disk? > load them
			LoadSettings ();
			
		}

		static void SaveSettings () {
			FileManager.WriteString("settings.json", UnityEngine.JsonUtility.ToJson(systemSettings,true), true);
		}

		static void LoadSettings () {
			
			string loadedSettings = FileManager.ReadStringFromApplicationPath("settings.json");
			EventManager.TriggerEvent(eDIA.Events.Core.EvUpdateSystemSettings, new eParam(loadedSettings));
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EVENT LISTENERS

		public static void OnEvUpdateSystemSettings (eParam obj) {

			SettingsDeclaration receivedSettings = UnityEngine.JsonUtility.FromJson<SettingsDeclaration>(obj.GetString());
			
			// Primary hand interaction
			if (systemSettings.primaryInteractor != receivedSettings.primaryInteractor) {
				Debug.Log("new hand");
				systemSettings.primaryInteractor = receivedSettings.primaryInteractor;
				EventManager.TriggerEvent(eDIA.Events.Interaction.EvUpdatePrimaryInteractor, new eParam((int)systemSettings.primaryInteractor));
			}

			// Resolution of the app
			if (systemSettings.screenResolution != receivedSettings.screenResolution) {
				systemSettings.screenResolution = receivedSettings.screenResolution;
				
				//TODO Change actual value
			}

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


			Debug.Log("OnEvUpdateSystemSettings");
			
			SaveSettings();

		}

		/// <summary> Catches request to show system settings, collects them and send them out with a OPEN settings panel event. </summary>
		private static void EvRequestSystemSettings(eParam obj)
		{
			EventManager.TriggerEvent(eDIA.Events.Core.EvOpenSystemSettings, new eParam( GetSettingsAsJSONstring()));
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HELPERS

		/// <summary>Gets all settings from the 'SettingsDeclaration' instance 'systemSettings' as a JSON string</summary>
		/// <returns>JSON string</returns>
		public static string GetSettingsAsJSONstring () {
			return UnityEngine.JsonUtility.ToJson(systemSettings,false);
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------

	}
}