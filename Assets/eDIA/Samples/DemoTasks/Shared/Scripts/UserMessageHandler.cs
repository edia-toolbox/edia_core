using System.Collections;
using System.Collections.Generic;
using eDIA;
using TMPro;
using UnityEngine;

namespace TASK {

	/// <summary>
	/// Sample script to show the user a message in VR canvas
	/// </summary>
	public class UserMessageHandler : MonoBehaviour {
		public TextMeshProUGUI msgField = null;

		void Start () {
			EventManager.StartListening ("EvShowMessage", OnEvShowMessage);
		}

		void OnDestroy () {
			EventManager.StopListening ("EvShowMessage", OnEvShowMessage);
		}

		public void OnEvShowMessage (eParam e) {
			if (msgField == null)
				return;

			msgField.text = e.GetString ();
		}
	}
}