using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Edia {
    /// <summary>Base class for a 'screen' in VR (canvas)</summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(TrackedDeviceGraphicRaycaster))]
    public class ScreenInVR : MonoBehaviour {
        [Header("Settings")]
        [Tooltip("Auto orientates itself in front of user. Draws on top of the 3D environment.")]
        public bool StickToHMD = false;
        [SerializeField] private float _distanceFromHMD = 2f;
        [Space(20)]
        [SerializeField] private bool _startVisible = false;

        [HideInInspector]
        public bool isActive = false;

        private void Awake() {
            if (GetComponent<Canvas>().worldCamera == null)
                GetComponent<Canvas>().worldCamera = StickToHMD ? XRManager.Instance.CamOverlay.GetComponent<Camera>() : XRManager.Instance.XRCam.GetComponent<Camera>();

            if (StickToHMD) {
                transform.parent = XRManager.Instance.XRCam.transform;
                transform.localPosition = new Vector3(0, 0, _distanceFromHMD);
                transform.localRotation = Quaternion.identity;
            }

            if (!_startVisible) Show(false);
        }

        /// <summary>Shows the actual panel</summary>
        public virtual void Show(bool onOff) {
            GetComponent<Canvas>().enabled = onOff;
            if (GetComponent<GraphicRaycaster>() != null)
                GetComponent<GraphicRaycaster>().enabled = onOff;
            GetComponent<TrackedDeviceGraphicRaycaster>().enabled = onOff;

            isActive = onOff;
        }

        /// <summary>Hide panel</summary>
        public virtual void HidePanel() {
            Show(false);
        }
    }
}