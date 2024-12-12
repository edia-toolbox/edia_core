using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Edia {
	/// <summary>Sample script to show the user a message in VR canvas</summary>
	[RequireComponent(typeof(Canvas))]
	[RequireComponent(typeof(TrackedDeviceGraphicRaycaster))]
	public class ScreenInVR : MonoBehaviour {

		[Header("Locked to User")]
		public bool StickToHMD = false;
		public float DistanceFromHMD = 2f;

		[Header("Settings")]
		public bool StartVisible = false;

		[HideInInspector]
		public bool isActive = false;

		private void Awake() {

			if (GetComponent<Canvas>().worldCamera == null)
				GetComponent<Canvas>().worldCamera = XRManager.Instance.CamOverlay.GetComponent<Camera>();

			if (StickToHMD) {
				transform.SetParent(XRManager.Instance.XRCam, false);
				transform.localPosition = new Vector3(0, 0, DistanceFromHMD);
				transform.localRotation = Quaternion.identity;
			}

			if (!StartVisible) Show(false);
		}

		/// <summary>Shows the actual panel</summary>
		public virtual void Show(bool onOff) {
			GetComponent<Canvas>().enabled = onOff;
			GetComponent<GraphicRaycaster>().enabled = onOff;
			GetComponent<TrackedDeviceGraphicRaycaster>().enabled = onOff;
			
			isActive = onOff;
		}

		/// <summary>Doublecheck</summary>
		public virtual void HidePanel() {
			Show(false);
		}
	}
}
