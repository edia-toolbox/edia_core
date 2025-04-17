using System.Collections.Generic;
using UnityEngine;
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

    /// <summary>
    /// Manages all call XR related
    /// </summary>
    public class XRManager : Singleton<XRManager> {

        [Header("Debug")]
        public bool ShowConsoleMessages = false;

        [Space(10f)]
        [Header("References")]
        public Transform XRCam;
        // public Transform XRLeft;
        // public Transform XRRight;

        public List<XRController> XRControllers = new();

        [Header("Settings")]
        [Tooltip("Enable Position&Rotation tracker from UXF which stores data to session folder. !Might have impact on FPS with long trials.")]
        public bool TrackXrRigWithUxf = false;
        
bool isInteractive = false;

        void Awake() {
            CheckReferences();
        }

        private void Start() {
            // TODO: fix with new Interactiontoolkit setup
            // EnableXRRayInteraction(false); // Start the system with interaction rays disabled
            ConfigureXRrigTracking();
        }

        void CheckReferences() {
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
            
            XRCam.GetComponent<PositionRotationTracker>().enabled   = TrackXrRigWithUxf;

            
            XRControllers.Find(x => x.Side == Constants.Sides.Left).UXFPoseTracker.enabled = TrackXrRigWithUxf;
            Session.instance.trackedObjects.Add(XRManager.Instance.XRCam.GetComponent<PositionRotationTracker>());

            // Session.instance.trackedObjects.Add(XRManager.Instance.XRLeft.GetComponent<PositionRotationTracker>());
            // Session.instance.trackedObjects.Add(XRManager.Instance.XRRight.GetComponent<PositionRotationTracker>());
        }

#region Inspector debug calls

        [ContextMenu("TurnOnRayInteractor")]
        public void TurnOnRayInteractor() {
            EnableXRRayInteraction(true);
        }

        [ContextMenu("ShowHands")]
        public void ShowHands() {
            ShowHands(true);
        }

        [ContextMenu("ShowControllers")]
        public void ShowControllers() {
            ShowControllers(true);
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region XR Helper methods

        /// <summary>The pivot of the player will be set on the location of this Injector</summary>
        public void MovePlayarea(Transform newTransform) {
            transform.position = newTransform.position;
            transform.rotation = newTransform.rotation;
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region INTERACTION

        /// <summary>Turn XR hand / controller interaction possibility on or off.</summary>
        /// <param name="onOff">Boolean</param>
        public void EnableXRRayInteraction(bool onOff) {
            AddToConsole("EnableXRInteraction " + onOff);
            isInteractive = onOff;
        }

        private void SetNearFarInteractor(List<NearFarInteractor> interactors, bool onOffNear, bool onOffFar) {
            foreach (NearFarInteractor interactor in interactors) {
                interactor.enableFarCasting  = onOffFar;
                interactor.enableNearCasting = onOffNear;
            }
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region SIGHT

        /// <summary>Fades VR user view to black</summary>
        public void HideVR() {
            Fade(true, -1f);
        }

        /// <summary>Fades VR user view to black</summary>
        /// <param name="fadeSpeed">Speed to fade, default: 1</param>
        public void HideVR(float fadeSpeed) {
            Fade(true, fadeSpeed);
        }

        void Fade(bool _onOff, float _fadeSpeed) {
            if (_onOff) XRCam.GetComponent<ScreenFader>().StartFadeBlackIn(_fadeSpeed);
            else XRCam.GetComponent<ScreenFader>().StartFadeBlackOut(_fadeSpeed);
        }

        /// <summary>Fades VR user view from black</summary>
        public void ShowVR() {
            Fade(false, -1f);
        }

        /// <summary>Fades VR user view from black</summary>
        /// <param name="fadeSpeed">Speed to fade, default: 1</param>
        public void ShowVR(float fadeSpeed) {
            Fade(false, fadeSpeed);
        }

        /// <summary>Instantly shows VR user view</summary>
        public void ShowVRInstantly() {
            XRCam.GetComponent<ScreenFader>().HideBlockingImage();
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HANDS

        /// <summary>Shows the hands that are set to be allowed visible on/off</summary>
        public void ShowHands(bool onOff) {
            // TODO Can we just hide ONE hand, or only TWO? (see HandVisualizer script)


            // XRLeft.GetComponent<XRController>().ShowHandModel(onOff);
            // XRRight.GetComponent<XRController>().ShowHandModel(onOff);
        }

        public void ShowControllers(bool onOff) {
            // TODO Just show hide the `Left Controller VIsual` gameobject or is it more complex than that?
            // XRLeft.GetComponent<XRController>().ShowControllerModel(onOff);
            // XRRight.GetComponent<XRController>().ShowControllerModel(onOff);
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

        private void AddToConsole(string _msg) {
            if (ShowConsoleMessages)
                Edia.LogUtilities.AddToConsoleLog(_msg, "XRManager");
        }
    }
}