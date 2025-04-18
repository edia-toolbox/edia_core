using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Hands.Samples.VisualizerSample;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UXF;

namespace Edia {

    [System.Serializable]
    public class XRController {
        public Constants.Sides         Side = Constants.Sides.Left;
        public GameObject              ControllerModel;
        public UXF.Tracker             UXFPoseTracker;
        public List<NearFarInteractor> NearFarInteractors = new();
        public List<XRPokeInteractor>  PokeInteractors    = new();
    }

    public class XRManager : Singleton<XRManager> {

        [Header("Settings")]
        [Tooltip("Use UXF to track and save XR Rig Position & Rotation data")]
        public bool TrackXrRigWithUxf = false;

        [Tooltip("Use OPENXR handtracking")] // TODO How will this work together with i.e. META handtracking
        public bool AllowHands = false;

        [Tooltip("Use controllers")]
        public bool AllowControllers = false;

        [Header("Debug")]
        public bool ShowConsoleMessages = false;

        [Space(10f)]
        [Header("References")]
        public Transform XRCam;

        public XRController   XRLeft;
        public XRController   XRRight;
        public HandVisualizer HandVisualizer;

        // Internals
        bool isInteractive = false;

#region --- PROPERTIES

        void Awake() {
            EventManager.StartListening(Edia.Events.XR.EvUpdateInteractiveSide, OnEvUpdateInteractiveSide);
            CheckAndSetReferences();
        }

        private void Start() {
            DisableAllInteractors();
            ConfigureXRrigTracking();
        }

        private void OnDestroy() {
            EventManager.StopListening(Edia.Events.XR.EvUpdateInteractiveSide, OnEvUpdateInteractiveSide);
        }

        void CheckAndSetReferences() {
            if (XRCam == null) Debug.LogError("XR Camera reference not set");
        }

        private void OnDrawGizmos() {
            Gizmos.color  = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.5f, 0.0f, 0.5f));
            Gizmos.DrawLine(Vector3.zero, Vector3.forward);
        }

        private void ConfigureXRrigTracking() {
            if (!TrackXrRigWithUxf)
                return;

            XRCam.GetComponent<PositionRotationTracker>().enabled = TrackXrRigWithUxf;
            Session.instance.trackedObjects.Add(XRManager.Instance.XRCam.GetComponent<PositionRotationTracker>());
            XRLeft.UXFPoseTracker.enabled = TrackXrRigWithUxf;
            Session.instance.trackedObjects.Add(XRLeft.UXFPoseTracker);
            XRRight.UXFPoseTracker.enabled = TrackXrRigWithUxf;
            Session.instance.trackedObjects.Add(XRRight.UXFPoseTracker);
        }

        private void OnEvUpdateInteractiveSide(eParam obj) {
            DisableAllInteractors();

            // If we were interacting, active with new settings
            if (isInteractive)
                EnableAllInteraction(true);
        }

        [ContextMenu("DisableAllInteractors")]
        public void DisableAllInteractors() {
            // Disable all
            SetNearFarInteractor(XRLeft.NearFarInteractors, false, false);
            SetPokeInteractor(XRLeft.PokeInteractors, false, false);
            SetNearFarInteractor(XRRight.NearFarInteractors, false, false);
            SetPokeInteractor(XRRight.PokeInteractors, false, false);
        }

        /// <summary>
        /// XR Input Modality Manager seems to enable all interaction by default when hand or controller detected.
        /// This method fires on 'TrackedHandModeStarted' and 'MotionControllerModeStarted' (set in inspector)
        /// </summary>
        public async void SetInitialInteractionStateAsync() {
            AddToConsole("Set Initial Interaction State on hands&controllers");
            await Task.Delay(100);
            EventManager.TriggerEvent(Edia.Events.XR.EvUpdateInteractiveSide, null);
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
#region XR Helper methods

        // TODO test and document
        /// <summary>The pivot of the player will be set on the location of this Injector</summary>
        public void MovePlayarea(Transform newTransform) {
            transform.position = newTransform.position;
            transform.rotation = newTransform.rotation;
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

        /// <summary>Controls the visibility of hand meshes in the XR environment.</summary>
        /// <param name="onOff">True to display hand meshes, false to hide them.</param>
        public void ShowHands(bool onOff) {
            HandVisualizer.drawMeshes = onOff;
        }

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