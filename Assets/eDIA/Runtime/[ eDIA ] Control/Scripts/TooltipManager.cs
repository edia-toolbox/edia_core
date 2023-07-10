using System.Diagnostics.Tracing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.InputSystem;

namespace eDIA {
	
	public class TooltipManager : MonoBehaviour {

		public TextMeshProUGUI textField = null;
		private bool isVisible = false;
		private RectTransform tooltipWindow;
		private float offsetY = -15f;
		private float offsetX = 20f;

		void Awake() {
			tooltipWindow = gameObject.GetComponent<RectTransform>();
		}

		void OnEnable() {
			EventManager.StartListening(eDIA.Events.ControlPanel.EvShowTooltip, OnEvShowTooltip);
			EventManager.StartListening(eDIA.Events.ControlPanel.EvHideTooltip, OnEvHideTooltip);
		}

		void OnDestroy() {
			EventManager.StopListening(eDIA.Events.ControlPanel.EvShowTooltip, OnEvShowTooltip);
			EventManager.StopListening(eDIA.Events.ControlPanel.EvHideTooltip, OnEvHideTooltip);
		}

		void Start() {
			OnEvHideTooltip(null);
		}

		public void OnEvShowTooltip (eParam obj) {
			
			textField.text = obj.GetString();
			tooltipWindow.transform.position = CalculateTooltipScreenPosition();
			tooltipWindow.sizeDelta = new Vector2(textField.preferredWidth > 400 ? textField.preferredWidth : 400, textField.preferredHeight);
			
			gameObject.SetActive(true);
			isVisible = true;
		}

		public void OnEvHideTooltip (eParam obj) {

			textField.text = default;
			gameObject.SetActive(false);
		}

		private Vector2 CalculateTooltipScreenPosition () {
			return new Vector2(Mouse.current.position.ReadValue().x + offsetX, Mouse.current.position.ReadValue().y + offsetY);
		}

		void Update() {
			if (!isVisible) 
				return;
			
			tooltipWindow.transform.position = CalculateTooltipScreenPosition();
		}
	}
}