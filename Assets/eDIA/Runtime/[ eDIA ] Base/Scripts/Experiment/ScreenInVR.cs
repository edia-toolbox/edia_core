using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace eDIA {
	/// <summary>Sample script to show the user a message in VR canvas</summary>
	[RequireComponent(typeof(Canvas))]
	[RequireComponent(typeof(TrackedDeviceGraphicRaycaster))]
	public class ScreenInVR : MonoBehaviour {

		[Header("Locked to User")]
		public bool StickToHMD = true;
		public float DistanceFromHMD = 2f;

		[Header("Settings")]
		public bool StartVisible = false;

		Canvas _myCanvas = null;
		GraphicRaycaster _graphicRaycaster = null;
		TrackedDeviceGraphicRaycaster _trackedDeviceGraphicRaycaster = null;

		private void Awake() {
			_myCanvas = GetComponent<Canvas>();
			_myCanvas.enabled = StartVisible;
			_graphicRaycaster = GetComponent<GraphicRaycaster>();
			_graphicRaycaster.enabled = StartVisible;
			_trackedDeviceGraphicRaycaster = GetComponent<TrackedDeviceGraphicRaycaster>();
			_trackedDeviceGraphicRaycaster.enabled = StartVisible;

			if (_myCanvas.worldCamera == null)
				_myCanvas.worldCamera = XRManager.Instance.camOverlay.GetComponent<Camera>();

			if (StickToHMD) {
				transform.SetParent(XRManager.Instance.XRCam, true);
				transform.localPosition = new Vector3(0, 0, DistanceFromHMD);
			}
		}

		#region PANEL

		/// <summary>Shows the actual panel</summary>
		public void ShowPanel(bool onOff) {
			_myCanvas.enabled = onOff;
			_myCanvas.worldCamera.enabled = onOff;
			_graphicRaycaster.enabled = onOff;
			_trackedDeviceGraphicRaycaster.enabled = onOff;
		}

		/// <summary>Doublecheck</summary>
		public void HidePanel() {
			ShowPanel(false);
		}

		#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}
