using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Edia {
    public class XRController : MonoBehaviour {
        [Header("State")]
        public Constants.ManipulatorSides MySide = Constants.ManipulatorSides.Left;
        [SerializeField] private string InteractionSide = "LEFT";
        public bool isAllowedToBeVisible = false;
        public bool isAllowedToInteract = false;
        public bool isVisible = false;
        public bool isInteractive = false;
        bool isOverlayInteractive = false;

        [Header("Models")]
        public GameObject HandModel = null;
        private Transform[] HandModelChildren;
        public GameObject ControllerModel = null;
        private Transform[] ControllerModelChildren;

        [Tooltip("Ray interactor to interact with UI & Default ")]
        public Transform rayInteractor = null;

        [Tooltip("Ray interactor to interact messagepanel OVERLAY UI")]
        public Transform XROverlayRayInteractor = null;

        #region SETTING UP

        void Awake() {
            AllowVisible(isVisible);
            AllowInteractive(isAllowedToInteract);

            HandModelChildren = HandModel.GetComponentsInChildren<Transform>(true);
            ControllerModelChildren = ControllerModel.GetComponentsInChildren<Transform>(true);

            EventManager.StartListening(Edia.Events.XR.EvUpdateVisableSide, OnEvUpdateVisableSide);
            EventManager.StartListening(Edia.Events.XR.EvUpdateInteractiveSide, OnEvUpdateInteractiveSide);
            EventManager.StartListening(Edia.Events.XR.EvShowXRController, OnEvShowXRController);
            // EventManager.StartListening(Edia.Events.XR.EvEnableXROverlay, OnEvEnableXROverlay);
        }

        void OnDestroy() {
            EventManager.StopListening(Edia.Events.XR.EvUpdateVisableSide, OnEvUpdateVisableSide);
            EventManager.StopListening(Edia.Events.XR.EvUpdateInteractiveSide, OnEvUpdateInteractiveSide);
            EventManager.StopListening(Edia.Events.XR.EvShowXRController, OnEvShowXRController);
            // EventManager.StopListening(Edia.Events.XR.EvEnableXROverlay, OnEvEnableXROverlay);
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.1f, 0.02f, 0.15f));
            Gizmos.DrawWireCube(Vector3.zero - (InteractionSide.ToUpper() == "LEFT" ? new Vector3(-0.06f, 0.01f, 0.05f) : new Vector3(0.06f, 0.01f, 0.05f)),
                new Vector3(0.03f, 0.02f, 0.05f));
        }

        #endregion // -------------------------------------------------------------------------------------------------------------------------------

        #region EVENT LISTENERS

        /// <summary>Enable interaction with UI presented on layer 'camoverlay'</summary>
        private void PutModelsOnOverlayLayer (bool isOverlay) {
            foreach (Transform t in HandModelChildren) {
                t.gameObject.layer = LayerMask.NameToLayer(isOverlay ? "CamOverlay" : "Default");
            }

            foreach (Transform t in ControllerModelChildren) {
                t.gameObject.layer = LayerMask.NameToLayer(isOverlay ? "CamOverlay" : "Default");
            }
        }

        /// <summary>Change visible side(s)</summary>
        /// <param name="obj">Interactor name</param>
        private void OnEvUpdateVisableSide(eParam obj) {
            string recievedSide = obj.GetString().ToUpper();

            if ((recievedSide == "BOTH") || (recievedSide == MySide.ToString().ToUpper())) {
                isAllowedToBeVisible = true;
                ShowHandModel(true);
            }
            else {
                isAllowedToBeVisible = false;
                isVisible = false;
                ShowHandModel(false);
            }
        }

        /// <summary>Update interactive side(s)</summary>
        /// <param name="obj">Interactor name</param>
        private void OnEvUpdateInteractiveSide(eParam obj) {
            string receivedSide = obj.GetString().ToUpper();

            if ((receivedSide == "BOTH") || (receivedSide == MySide.ToString().ToUpper())) {
                isAllowedToInteract = true;
            }
            else {
                isAllowedToInteract = false;
                isInteractive = false;
                rayInteractor.gameObject.SetActive(false);

                isOverlayInteractive = false;
                XROverlayRayInteractor.gameObject.SetActive(false);
            }
            
            Invoke("EnableDelayedXRInteraction", 0.1f);
            Invoke("EnableDelayedXROverlayInteraction", 0.1f);
        }

        // Send out XR interaction
        private void EnableDelayedXRInteraction() {
            XRManager.Instance.EnableXRRayInteraction(XRManager.Instance.isInteractive);
        }

        // Send out XR OVERLAY interaction
        private void EnableDelayedXROverlayInteraction() {
            XRManager.Instance.EnableXROverlayRayInteraction(XRManager.Instance.isOverlayInteractive);
        }

        /// <summary>Change the controller / interactor that is visible</summary>
        /// <param name="obj">Interactor enum index</param>
        // private void OnEvEnableXRRayInteraction(eParam obj) {
        //     EnableRayInteraction(obj.GetBool());
        // }

        /// <summary>Change the controller / interactor that is visible</summary>
        /// <param name="obj">Interactor enum index</param>
        private void OnEvShowXRController(eParam obj) {
            ShowHandModel(obj.GetBool());
        }

        #endregion // -------------------------------------------------------------------------------------------------------------------------------

        #region MAIN METHODS

        /// <summary>Show the actual controller of hand visually</summary>
        /// <param name="onOff">True/false</param>
        void AllowVisible(bool onOff) {
            isAllowedToBeVisible = onOff;
        }

        /// <summary>Allow this controller to be interacting with the environment</summary>
        /// <param name="onOff">True/false</param>
        void AllowInteractive(bool onOff) {
            isAllowedToInteract = onOff;
        }

        /// <summary>Enable/Disable interaction</summary>
        /// <param name="onOff">True/false</param>
        public void EnableRayInteraction(bool onOff) {
            if (!isAllowedToInteract)
                return;

            if (!isVisible)
                return;

            rayInteractor.gameObject.SetActive(onOff);
            isInteractive = onOff;
        }


        /// <summary>Enable/Disable interaction</summary>
        /// <param name="onOff">True/false</param>
        public void EnableXROverlayRayInteraction(bool onOff) {
            if (!isAllowedToInteract)
                return;

            if (!isVisible)
                return;

            PutModelsOnOverlayLayer(onOff);
            
            XROverlayRayInteractor.gameObject.SetActive(onOff);
            isOverlayInteractive = onOff;
        }

        /// <summary>Show/Hide hand</summary>
        /// <param name="onOff">True/false</param>
        public void ShowHandModel(bool onOff) {
            isVisible = onOff && isAllowedToBeVisible;
            HandModel.SetActive(onOff && isAllowedToBeVisible);
        }

        /// <summary>Show/Hide controller</summary>
        /// <param name="onOff">True/false</param>
        public void ShowControllerModel(bool onOff) {
            isVisible = onOff && isAllowedToBeVisible;
            ControllerModel.SetActive(onOff && isAllowedToBeVisible);
        }

        #endregion // -------------------------------------------------------------------------------------------------------------------------------
    }
}