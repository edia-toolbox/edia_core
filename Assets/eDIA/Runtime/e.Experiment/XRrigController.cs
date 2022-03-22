using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace eDIA {
	
	public class XRrigController : MonoBehaviour	{

		[Header("Settings")]
		public eDIA.Constants.Interactor interactorType = eDIA.Constants.Interactor.LEFT;

		public bool isVisible = false;
		public bool isInteractive = false;

		SkinnedMeshRenderer handSMR = null;
		XRInteractorLineVisual lineVisual = null;

		void Awake() {
			handSMR = GetComponentInChildren<SkinnedMeshRenderer>(true);
			lineVisual = GetComponent<XRInteractorLineVisual>();

			MakeVisible(isVisible);
			MakeInteractive(isInteractive);

			EventManager.StartListening(eDIA.Events.Interaction.EvUpdateVisableInteractor, OnEvUpdateVisableInteractor);
			EventManager.StartListening(eDIA.Events.Interaction.EvUpdateInteractiveInteractor, OnEvUpdateInteractiveInteractor);
		}

		void OnDestroy() {
			EventManager.StopListening(eDIA.Events.Interaction.EvUpdateVisableInteractor, OnEvUpdateVisableInteractor);
			EventManager.StopListening(eDIA.Events.Interaction.EvUpdateInteractiveInteractor, OnEvUpdateInteractiveInteractor);
		}

		private void OnEvUpdateVisableInteractor(eParam obj)
		{
			eDIA.Constants.Interactor receivedInteractor = (eDIA.Constants.Interactor)obj.GetInt();
			
			if ((receivedInteractor == eDIA.Constants.Interactor.BOTH) || (receivedInteractor == interactorType)) {

				MakeVisible(true);
			} else MakeVisible(false);
		}

		private void OnEvUpdateInteractiveInteractor(eParam obj)
		{
			eDIA.Constants.Interactor receivedInteractor = (eDIA.Constants.Interactor)obj.GetInt();

			if ((receivedInteractor == eDIA.Constants.Interactor.BOTH) || (receivedInteractor == interactorType)) {
				MakeInteractive(true);
			} else MakeInteractive(false);
		}

		public void MakeVisible (bool _onOff) {
			handSMR.enabled = _onOff;		
			isVisible = _onOff;
		}

		public void MakeInteractive (bool _onOff) {
			isInteractive = _onOff;
			lineVisual.enabled = _onOff;
		}

    	}
}