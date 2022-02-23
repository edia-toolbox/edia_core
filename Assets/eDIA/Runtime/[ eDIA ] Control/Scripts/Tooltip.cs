using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace eDIA {
	/// <summary>Enables a tooltip on the component this is on.</summary>
	public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

		public string message = "This will be shown in the tooltip";
		private float timeToWait = 0.5f;
		private bool isHovering = false;

		public void OnPointerEnter (PointerEventData eventData) {
			isHovering = true;
			StopAllCoroutines ();
			StartCoroutine (StartTimer ());
		}

		public void OnPointerExit (PointerEventData eventData) {
			StopAllCoroutines ();
			
			if (isHovering)
				EventManager.TriggerEvent(eDIA.Events.GUI.EvMouseExit, null);
			
			isHovering = false;
		}

		void ShowMessage () {
			EventManager.TriggerEvent(eDIA.Events.GUI.EvMouseEnter, new eParam(message));
		}

		IEnumerator StartTimer () {
			yield return new WaitForSeconds (timeToWait);

			ShowMessage ();
		}
	}
}