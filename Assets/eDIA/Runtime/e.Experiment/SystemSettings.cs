using System.Threading.Tasks;
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
			EventManager.StartListening(eDIA.Events.Core.EvRequestSystemSettings, OnEvRequestSystemSettings);
			
			// Any settings on disk? > load them
			LoadSettings();
		}

		static void SaveSettings () {
			FileManager.WriteString("settings.json", UnityEngine.JsonUtility.ToJson(systemSettings,true), true);
		}

		async static void LoadSettings () {
			
			string loadedSettings = FileManager.ReadStringFromApplicationPath("settings.json");
			Debug.Log(" settings loaded ");
			
			await Task.Delay(500); // 1 second delay
			EventManager.TriggerEvent(eDIA.Events.Core.EvUpdateSystemSettings, new eParam(loadedSettings));
		}


#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EVENT LISTENERS

		public static void OnEvUpdateSystemSettings (eParam obj) {
			Debug.Log("OnEvUpdateSystemSettings");

			SettingsDeclaration receivedSettings = UnityEngine.JsonUtility.FromJson<SettingsDeclaration>(obj.GetString());

			EventManager.TriggerEvent(eDIA.Events.Interaction.EvUpdateVisableInteractor, new eParam((int)receivedSettings.VisableInteractor));
			
			// if (systemSettings.VisableInteractor != receivedSettings.VisableInteractor) {
			// 	Debug.Log("Visable interactor change " + systemSettings.VisableInteractor + " <> " + receivedSettings.VisableInteractor);
			// 	systemSettings.VisableInteractor = receivedSettings.VisableInteractor;
			// }

			EventManager.TriggerEvent(eDIA.Events.Interaction.EvUpdateInteractiveInteractor, new eParam((int)receivedSettings.InteractiveInteractor));
			
			// // Primary hand interaction
			// if (systemSettings.InteractiveInteractor != receivedSettings.InteractiveInteractor) {
			// 	Debug.Log("Interactive interactor change");
			// 	systemSettings.InteractiveInteractor = receivedSettings.InteractiveInteractor;
			// }

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
			
			SaveSettings();
		}

		/// <summary> Catches request to show system settings, collects them and send them out with a OPEN settings panel event. </summary>
		private static void OnEvRequestSystemSettings(eParam obj)
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