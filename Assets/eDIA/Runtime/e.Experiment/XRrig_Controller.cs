using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace eDIA {
	
	public class XRrig_Controller : MonoBehaviour	{

		[Header("Settings")]
		public eDIA.Constants.Interactor interactorType = eDIA.Constants.Interactor.LEFT;

		SkinnedMeshRenderer handSMR = null;
		XRInteractorLineVisual lineVisual = null;

		void Awake() {
			handSMR = GetComponentInChildren<SkinnedMeshRenderer>(true);
			lineVisual = GetComponent<XRInteractorLineVisual>();

			EventManager.StartListening(eDIA.Events.Interaction.EvUpdateAvailableInteractor, OnEvUpdateAvailableInteractor);
		}

		private void OnEvUpdateAvailableInteractor(eParam obj)
		{
			eDIA.Constants.Interactor receivedInteractor = (eDIA.Constants.Interactor)obj.GetInt();
			if ((receivedInteractor == eDIA.Constants.Interactor.BOTH) || (receivedInteractor == interactorType)) {
				// Enable this controller
				EnableInteraction();
			}
		}

		void EnableInteraction () {
			Debug.Log( name + " EnableInteraction");
		}

		void ShowHand (bool _onOff) {
			handSMR.enabled = _onOff;		
		}
    	}
}