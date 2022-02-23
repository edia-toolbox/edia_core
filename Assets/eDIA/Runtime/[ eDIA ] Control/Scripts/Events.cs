using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA.Events {

	/// <summary>UI related events </summary>
	public class GUI {

		// Show info on screen to Experimenter
		public static string EvShowMessageBox = "EvShowMessageBox";

		// Fired when mouse hovers over a GUI item that has 'tooltip' script on it
		public static string EvMouseEnter = "EvMouseEnter";

		// Fired when mouse hovers over a GUI item that has 'tooltip' script on it
		public static string EvMouseExit = "EvMouseExit";

	}
}