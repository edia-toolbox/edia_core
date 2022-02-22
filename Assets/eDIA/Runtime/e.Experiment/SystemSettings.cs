using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA {

	public static class SystemSettings {

		// Which hand is primary
		public static Constants.PrimaryInteractor primaryInteractor;

		// Resolution on standalone windows player
		public static UnityEngine.Vector2 onScreenResolution = new UnityEngine.Vector2(1920f,1080f);

		// Master volume
		public static float volume = 50f;


		public static void SavePrefs () {
			PlayerPrefs.SetInt ("primaryInteraction", (int)primaryInteractor);
			PlayerPrefs.Save ();
		}

		public static void LoadPrefs () {
			int volume = PlayerPrefs.GetInt ("Volume", 1);
			Debug.Log(volume);
		}







	}
}