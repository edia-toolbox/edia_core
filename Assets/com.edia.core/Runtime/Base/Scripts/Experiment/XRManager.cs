using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Hands.Samples.VisualizerSample;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;
using UXF;

namespace Edia {

    /// <summary>
    /// Represents an XR controller used in the application and provides configuration for the controller model,
    /// interaction modes, and trackers.
    /// The XRController class is used to manage and reference key properties for XR controllers in the system,
    /// including associated models, pose trackers, and interaction mechanisms such as near/far and poke interaction.
    /// </summary>
    [System.Serializable]
    public class XRController {
        public Constants.Sides         Side = Constants.Sides.Left;
        public Transform               Controller;
        public Transform               Hand;
        public GameObject              ControllerModel;
        public UXF.Tracker             UXFPoseTracker;
        public List<NearFarInteractor> NearFarInteractors = new();
        public List<XRPokeInteractor>  PokeInteractors    = new();
    }

    public class XRManager : Singleton<XRManager> {

#region PROPERTIES

        [Header("Settings")]
        [InspectorHeader("EDIA CORE", "XR Rig", "Manages all XR related calls for the framework")]
        [Tooltip("Use UXF to track and save XR Rig Position & Rotation data")]
        public bool TrackXrRigWithUxf = false;

        [Tooltip("TODO: Use OPENXR handtracking")] // TODO Make functional + How will this work together with i.e. META handtracking
        public bool AllowHands = false;

        [Header("Debug")]
        public bool ShowConsoleMessages = false;

        [Space(10f)]
        [Header("References")]
        public HandVisualizer HandVisualizer;

        public Transform XRCam;

        public XRController XRLeft;
        public XRController XRRight;

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

            if (TrackXrRigWithUxf)
                AddXRRigToUXFTracking();
        }

        private void InitialiseInteractors(List<NearFarInteractor> interactors) {
            foreach (var interactor in interactors) {
                var farCasterMask = interactor.GetComponent<CurveInteractionCaster>().raycastMask;
                farCasterMask |= 1 << LayerMask.NameToLayer("MsgPanelUI");
                interactor.GetComponent<CurveInteractionCaster>().raycastMask = farCasterMask;
            }
        }

        private void CheckAndSetReferences() {
            if (XRCam is null) Debug.LogError("XR Camera reference not set");
        }

        private void AddXRRigToUXFTracking() {
            if (XRCam.GetComponent<PositionRotationTracker>() == null)
                XRCam.gameObject.AddComponent<PositionRotationTracker>();

            XRCam.GetComponent<PositionRotationTracker>().enabled = TrackXrRigWithUxf;
            Session.instance.trackedObjects.Add(XRManager.Instance.XRCam.GetComponent<PositionRotationTracker>());
            XRLeft.UXFPoseTracker.enabled = TrackXrRigWithUxf;
            Session.instance.trackedObjects.Add(XRLeft.UXFPoseTracker);
            XRRight.UXFPoseTracker.enabled = TrackXrRigWithUxf;
            Session.instance.trackedObjects.Add(XRRight.UXFPoseTracker);
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
            // Disable all
            SetNearFarInteractor(XRLeft.NearFarInteractors, false, false);
            SetPokeInteractor(XRLeft.PokeInteractors, false, false);
            SetNearFarInteractor(XRRight.NearFarInteractors, false, false);
            SetPokeInteractor(XRRight.PokeInteractors, false, false);
        }

        /// <summary>
        /// Moves the XR Rig's controllers and hands to the specified overlay layer by changing their layer assignments.
        /// </summary>
        /// <param name="layerName">The name of the layer to which the XR Rig components should be moved.</param>
        public void MoveXRRigToOverlayLayer(string layerName) {
            SetLayerRecursively(XRLeft.Controller.gameObject, LayerMask.NameToLayer(layerName));
            SetLayerRecursively(XRRight.Controller.gameObject, LayerMask.NameToLayer(layerName));
            SetLayerRecursively(XRRight.Hand.gameObject, LayerMask.NameToLayer(layerName));
            SetLayerRecursively(XRLeft.Hand.gameObject, LayerMask.NameToLayer(layerName));
        }

        // TODO test and document
        /// <summary>The pivot of the player will be set on the location of this Injector</summary>
        public void MovePlayarea(Transform newTransform) {
            transform.position = newTransform.position;
            transform.rotation = newTransform.rotation;
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region Helper methods

        private static void SetLayerRecursively(GameObject go, int layer) {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }

#endregion
#region XR Locomotion

        public void EnableTeleportation(bool onOff) {
            // TODO implement
        }

        public void EnableClimbing(bool onOff) {
            // TODO implement
        }

        public void EnableMoving(bool onOff) {
            // TODO implement
        }

        public void EnableTurning(bool onOff) {
            // TODO implement
        }

#endregion
#region Inspector debug calls

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

#region INTERACTION

        // TODO document this
        /// <summary> Control all possible interactions. Use this if you have all possible interactive options in your project. </summary>
        /// <param name="onOff">On or Off</param>
        public void EnableAllInteraction(bool onOff) {
            EnableRayInteraction(onOff);
            EnablePokeInteraction(onOff);
        }

        // TODO document this
        /// <summary> Control RAY interaction </summary>
        /// <param name="onOff">On or Off</param>
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

        // TODO document this
        /// <summary> Control POKE interaction </summary>
        /// <param name="onOff">On or Off</param>
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
#region SIGHT

        // TODO document this
        /// <summary>Fades VR user view to black</summary>
        [ContextMenu("HideVR")]
        public void HideVR() {
            Fade(true, -1f);
        }

        // TODO document this
        /// <summary>Fades VR user view from black</summary>
        [ContextMenu("ShowVR")]
        public void ShowVR() {
            Fade(false, -1f);
        }

        // TODO document this
        /// <summary>Instantly shows VR user view</summary>
        public void ShowVRInstantly() {
            XRCam.GetComponent<ScreenFader>().HideBlocking();
        }

        private void ShowVR(float fadeSpeed) {
            Fade(false, fadeSpeed);
        }

        void Fade(bool _onOff, float _fadeSpeed) {
            if (_onOff) XRCam.GetComponent<ScreenFader>().StartFadeBlackIn(_fadeSpeed);
            else XRCam.GetComponent<ScreenFader>().StartFadeBlackOut(_fadeSpeed);
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HANDS

        // TODO document this
        /// <summary>Controls the visibility of hand meshes in the XR environment.</summary>
        /// <param name="onOff">True to display hand meshes, false to hide them.</param>
        public void ShowHands(bool onOff) {
            HandVisualizer.drawMeshes = onOff;
        }

        // TODO document this
        /// <summary> Toggles the visibility of the controller models associated with the XR system. </summary>
        /// <param name="onOff">A boolean value indicating whether to show the controllers (true) or hide them (false).</param>
        public void ShowControllers(bool onOff) {
            XRLeft.ControllerModel.SetActive(onOff);
            XRRight.ControllerModel.SetActive(onOff);
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

        public void AddToConsole(string _msg) {
            if (ShowConsoleMessages)
                Edia.LogUtilities.AddToConsoleLog(_msg, "XRManager");
        }
    }
}