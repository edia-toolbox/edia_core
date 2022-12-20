using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace eDIA {
	
	/// <summary>Enables a tooltip on the component this is on.</summary>
	public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

		public string message = "This message will be shown in the tooltip";

		private float timeToWait = 0.5f;
		private bool isShowing = false;

		public void OnPointerEnter (PointerEventData eventData) {
			StopAllCoroutines ();
			StartCoroutine (StartTimer ());
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (isShowing)
				OnPointerExit(eventData);
		}

		public void OnPointerExit (PointerEventData eventData) {
			StopAllCoroutines ();
			
			if (isShowing)
				EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvHideTooltip, null);
			
			isShowing = false;
		}

		void ShowMessage () {
			isShowing = true;
			EventManager.TriggerEvent(eDIA.Events.ControlPanel.EvShowTooltip, new eParam(message));
		}

		IEnumerator StartTimer () {
			yield return new WaitForSeconds (timeToWait);

			ShowMessage ();
		}
	}
}