using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA.Events {

	/// <summary>UI related events </summary>
	public class GUI {

		/// <summary>Shows message to experimenter canvas. Expects message as string, autohide as bool</summary>
		public static string EvShowMessageBox = "EvShowMessageBox";

		// Fired when mouse hovers over a GUI item that has 'tooltip' script on it. Expects null.
		public static string EvShowTooltip = "EvShowTooltip";

		// Fired when mouse hovers over a GUI item that has 'tooltip' script on it. Expects null.
		public static string EvHideTooltip = "EvHideTooltip";

		/// <summary>Sets the controlpanelmode. Exprects int. 0=hidden, 1=2Dcanvas, 2=3Dcanvas</summary>
		public static string EvSetControlPanelMode = "EvSetControlPanelMode";


	}
}