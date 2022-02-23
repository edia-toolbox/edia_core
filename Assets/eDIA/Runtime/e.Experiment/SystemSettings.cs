using Microsoft.VisualBasic;
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
#region EVENT LISTENERS

		public static void OnEvUpdateSystemSettings (eParam e) {
			Debug.Log("Systemsettings OnEvent");

			// Check if any things have changed

		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region MAIN METHODS

		/// <summary>Gets called from XRrigmanager to init the system. </summary>
		public static void InitSystemSettings () {

			systemSettings.language = Constants.Languages.DU;
			systemSettings.volume = 30f;
			systemSettings.primaryInteractor = Constants.PrimaryInteractor.RIGHTHANDED;

			// Any settings on disk? > load them

			// Apply loaded or default settings

			// Listen to update settings request
			EventManager.StartListening(eDIA.Events.Core.EvUpdateSystemSettings, OnEvUpdateSystemSettings);
			
			
			// ff hier
			EventManager.TriggerEvent(eDIA.Events.Core.EvOpenSystemSettings, new eParam( GetSettingsAsJSONstring()));
		}


		public static void ApplySystemSettings () {

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