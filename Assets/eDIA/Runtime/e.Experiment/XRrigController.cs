using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace eDIA {
	
	public class XRrigController : MonoBehaviour	{

		[Header("Settings")]
		public eDIA.Constants.Interactor interactorType = eDIA.Constants.Interactor.LEFT;

		bool isEnabled = false;
		bool isVisible = false;

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
				EnableInteraction(true);
			} else EnableInteraction(false);
		}

		public void EnableInteraction (bool _onOff) {
			Debug.Log( name + " EnableInteraction:" + _onOff);
			isEnabled = _onOff;
		}

		public void ShowHand (bool _onOff) {
			handSMR.enabled = _onOff;		
			isVisible = _onOff;
		}
    	}
}