using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Hands.Samples.VisualizerSample;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;
using Edia.Tracking;

namespace Edia {

    /// <summary>
    /// Represents an XR controller used in the application and provides configuration for the controller model,
    /// interaction modes, and trackers.
    /// </summary>
    [System.Serializable]
    public class XRController {
        public Constants.Sides         Side = Constants.Sides.Left;
        public Transform               Controller;
        public Transform               Hand;
        public GameObject              ControllerModel;
        public Tracker                 PoseTracker;
        public List<NearFarInteractor> NearFarInteractors = new();
        public List<XRPokeInteractor>  PokeInteractors    = new();
        public GameObject              TeleportInteractorObject;
    }

    /// <summary>
    /// Manages XR-related functionality within the framework, providing tools and settings for
    /// interaction, tracking, and visualization in an XR environment.
    /// </summary>
    [EdiaHeader("EDIA CORE", "XR Rig", "Manages all XR related calls for the framework")]
    public class XRManager : Singleton<XRManager> {

#region PROPERTIES

        [Header("Settings")]
        public bool AllowTeleportation = false;
        public bool AllowClimbing      = false;
        public bool AllowMoving        = false;
        public bool AllowTurning       = false;

        [Space(10)]
        [Tooltip("Track and save XR Rig Position & Rotation data")]
        public bool TrackXrRig = false;

        [Header("Debug")]
        public bool ShowConsoleMessages = false;

        [Space(10f)]
        [Header("References")]
        public HandVisualizer HandVisualizer;

        public Transform XRCam;

        public XRController XRLeft;
        public XRController XRRight;
        public GameObject   TurningLogicObject;
        public GameObject   MovingLogicObject;
        public GameObject   ClimbingLogicObject;

        // Internals
        bool isInteractive = false;

#endregion

        private void OnDrawGizmos() {
            Gizmos.color  = Edia.Constants.EdiaColors["blue"];
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.5f, 0.0f, 0.5f));
            Gizmos.DrawLine(Vector3.zero, Vector3.forward);
        }

        void Awake() {
            EventManager.StartListening(Edia.Events.XR.EvUpdateInteractiveSide, OnEvUpdateInteractiveSide);
            CheckAndSetReferences();
        }

        private void OnDestroy() {
            EventManager.StopListening(Edia.Events.XR.EvUpdateInteractiveSide, OnEvUpdateInteractiveSide);
        }

        private void Start() {
            InitialiseInteractors(XRLeft.NearFarInteractors);
            InitialiseInteractors(XRRight.NearFarInteractors);

            DisableAllInteractors(); // Unity enables them by default.

            EnableTeleportation(AllowTeleportation);
            EnableClimbing(AllowClimbing);
            EnableMoving(AllowMoving);
            EnableTurning(AllowTurning);

            if (TrackXrRig)
                AddXRRigToTracking();
        }

#region XR Initialisation

        private void InitialiseInteractors(List<NearFarInteractor> interactors) {
            foreach (var interactor in interactors) {
                var farCasterMask = interactor.GetComponent<CurveInteractionCaster>().raycastMask;

                int msgPanelLayer = LayerMask.NameToLayer(Constants.MsgPanelLayerName);
                if (msgPanelLayer == -1) {
                    Debug.LogError($"Layer '{nameof(msgPanelLayer)}' not found. Add this in the Unity editor project with the Edia Configurator");
                    Experiment.Instance.ShowMessageToExperimenter("MessagePanel layer not found! See log for details. ");
                }

                farCasterMask                                                 |= 1 << msgPanelLayer;
                interactor.GetComponent<CurveInteractionCaster>().raycastMask =  farCasterMask;
            }
        }

        private void CheckAndSetReferences() {
            if (XRCam is null) Debug.LogError("XR Camera reference not set");
        }

        private void AddXRRigToTracking() {
            if (XRCam.GetComponent<PositionRotationTracker>() == null)
                XRCam.gameObject.AddComponent<PositionRotationTracker>();

            XRCam.GetComponent<PositionRotationTracker>().enabled = TrackXrRig;
            Experiment.Instance.trackedObjects.Add(XRManager.Instance.XRCam.GetComponent<PositionRotationTracker>());
            XRLeft.PoseTracker.enabled = TrackXrRig;
            Experiment.Instance.trackedObjects.Add(XRLeft.PoseTracker);
            XRRight.PoseTracker.enabled = TrackXrRig;
            Experiment.Instance.trackedObjects.Add(XRRight.PoseTracker);
        }

        /// <summary>
        /// XR Input Modality Manager seems to enable all interaction by default when hand or controller detected.
        /// This method fires on 'TrackedHandModeStarted' and 'MotionControllerModeStarted' (set in inspector)
        /// </summary>
        public async void SetInitialInteractionStateAsync() {
            AddToConsole("Set Initial Interaction State on hands&controllers");
            await Task.Delay(500);
            DisableAllInteractors();
            EventManager.TriggerEvent(Edia.Events.XR.EvUpdateInteractiveSide, null);
        }

#endregion
#region Event Handlers

        private void OnEvUpdateInteractiveSide(eParam obj) {
            DisableAllInteractors();

            if (isInteractive) // If we were interacting, active with new settings
                EnableAllInteraction(true);
        }

#endregion
#region XR methods

        [ContextMenu("DisableAllInteractors")]
        public void DisableAllInteractors() {
            SetNearFarInteractor(XRLeft.NearFarInteractors, false, false);
            SetPokeInteractor(XRLeft.PokeInteractors, false, false);
            SetNearFarInteractor(XRRight.NearFarInteractors, false, false);
            SetPokeInteractor(XRRight.PokeInteractors, false, false);
        }

        public void MoveXRRigToOverlayLayer(string layerName) {
            SetLayerRecursively(XRLeft.Controller.gameObject, LayerMask.NameToLayer(layerName));
            SetLayerRecursively(XRRight.Controller.gameObject, LayerMask.NameToLayer(layerName));
            SetLayerRecursively(XRRight.Hand.gameObject, LayerMask.NameToLayer(layerName));
            SetLayerRecursively(XRLeft.Hand.gameObject, LayerMask.NameToLayer(layerName));
        }

        public void MovePlayarea(Transform newTransform) {
            transform.position = newTransform.position;
            transform.rotation = newTransform.rotation;
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region XR Locomotion

        public void EnableTeleportation(bool onOff) {
            XRLeft.TeleportInteractorObject.SetActive(onOff);
            XRRight.TeleportInteractorObject.SetActive(onOff);
        }

        public void EnableClimbing(bool onOff) {
            ClimbingLogicObject.SetActive(onOff);
        }

        public void EnableMoving(bool onOff) {
            MovingLogicObject.SetActive(onOff);
        }

        public void EnableTurning(bool onOff) {
            TurningLogicObject.SetActive(onOff);
        }

#endregion
#region Debug

        [ContextMenu("TurnOnRayInteractor")]
        public void TurnOnRayInteractor() {
            EnableRayInteraction(true);
        }

        [ContextMenu("TurnOnPokeInteractor")]
        public void TurnOnPokeInteractor() {
            EnablePokeInteraction(true);
        }

        [ContextMenu("ShowHands")]
        public void ShowHands() {
            ShowHands(true);
        }

        [ContextMenu("HideHands")]
        public void HideHands() {
            ShowHands(false);
        }

        [ContextMenu("ShowControllers")]
        public void ShowControllers() {
            ShowControllers(true);
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region Interaction

        public void EnableAllInteraction(bool onOff) {
            EnableRayInteraction(onOff);
            EnablePokeInteraction(onOff);
        }

        public void EnableRayInteraction(bool onOff) {
            if (SystemSettings.Instance.Settings.IsRightInteractive) SetNearFarInteractor(XRRight.NearFarInteractors, onOff, onOff);
            if (SystemSettings.Instance.Settings.IsLeftInteractive) SetNearFarInteractor(XRLeft.NearFarInteractors, onOff, onOff);
            isInteractive = onOff;
        }

        private void SetNearFarInteractor(List<NearFarInteractor> interactors, bool onOffNear, bool onOffFar) {
            foreach (NearFarInteractor interactor in interactors) {
                interactor.gameObject.SetActive(onOffFar);
                interactor.gameObject.SetActive(onOffNear);
            }
        }

        public void EnablePokeInteraction(bool onOff) {
            if (SystemSettings.Instance.Settings.IsRightInteractive) SetPokeInteractor(XRRight.PokeInteractors, onOff, onOff);
            if (SystemSettings.Instance.Settings.IsLeftInteractive) SetPokeInteractor(XRLeft.PokeInteractors, onOff, onOff);
            isInteractive = onOff;
        }

        private void SetPokeInteractor(List<XRPokeInteractor> interactors, bool onOffNear, bool onOffFar) {
            foreach (XRPokeInteractor interactor in interactors) {
                interactor.enabled = onOffFar;
                interactor.enabled = onOffNear;
            }
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region Sight

        [ContextMenu("HideVR")]
        public void HideVR() {
            Fade(true, -1f);
        }

        [ContextMenu("ShowVR")]
        public void ShowVR() {
            Fade(false, -1f);
        }

        public void ShowVRInstantly() {
            XRCam.GetComponent<ScreenFader>().HideBlocking();
        }

        private void ShowVR(float fadeSpeed) {
            Fade(false, fadeSpeed);
        }

        void Fade(bool _onOff, float fadeSpeed) {
            if (_onOff) XRCam.GetComponent<ScreenFader>().StartFadeBlackIn(fadeSpeed);
            else XRCam.GetComponent<ScreenFader>().StartFadeBlackOut(fadeSpeed);
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region Hands

        public void ShowHands(bool onOff) {
            HandVisualizer.drawMeshes = onOff;
        }

        public void ShowControllers(bool onOff) {
            XRLeft.ControllerModel.SetActive(onOff);
            XRRight.ControllerModel.SetActive(onOff);
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region Helper methods

        private static void SetLayerRecursively(GameObject go, int layer) {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }

        public void AddToConsole(string _msg) {
            if (ShowConsoleMessages)
                Edia.Utilities.Log.AddToConsoleLog(_msg, "XRManager");
        }

#endregion
    }
}
