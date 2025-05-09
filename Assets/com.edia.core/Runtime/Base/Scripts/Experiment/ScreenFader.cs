using System.Collections;
using System.Linq;
using Edia.Utilities;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Edia {

    /// <summary> Handles fading the camera view </summary>
    public class ScreenFader : MonoBehaviour {

        /// <summary>
        /// Represents the mesh renderer used for blocking or fading the camera's view.
        /// </summary>
        /// <remarks>
        /// This renderer is utilized in conjunction with the <see cref="ScreenFader"/> component to display
        /// and manage fade effects such as transitioning in or out of scenes. The material of this object
        /// is dynamically adjusted during fade operations to manipulate color intensity and transparency.
        /// Ensure that this reference is set in the inspector or during runtime to avoid null reference issues.
        /// </remarks>
        [Header("Refs")]
        public MeshRenderer BlockObject = null;

        /// <summary>
        /// Defines the color used for screen fading effects.
        /// </summary>
        /// <remarks>
        /// This color determines the appearance of the fade effect applied by the <see cref="ScreenFader"/> component.
        /// The alpha value of the color is dynamically adjusted during fade transitions to create a smooth effect.
        /// By default, the color is set to black.
        /// </remarks>
        [Header("Settings")]
        public Color FadeColor = Color.black;

        /// <summary>
        /// Determines the speed at which the screen fades to and from black.
        /// </summary>
        /// <remarks>
        /// This value is used by the <see cref="ScreenFader"/> methods to control
        /// the rate of fading transitions. Typically, higher values result in faster fades,
        /// while lower values create slower transitions. The default value is 1.0f.
        /// </remarks>
        public float FadeSpeed = 1f;

        // Internal
        private float    _speed         = 1f;
        private float    _intensity     = 0.0f;
        private bool     _isFaded       = false;
        private Material _blockMaterial = null;
        private Camera   _overlayCamera  = null;

        private void OnDrawGizmos() {
            Gizmos.color  = Edia.Constants.EdiaColors["blue"];
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(Vector3.zero, 0.1f);
            Gizmos.DrawCube(Vector3.zero + new Vector3(0, 0, 0.08f), new Vector3(0.125f, 0.075f, 0.075f));
            Gizmos.DrawLine(Vector3.zero, Vector3.forward);
        }

        private void Awake() {
            if (BlockObject == null)
                XRManager.Instance.AddToConsole("BlockObject is not referenced on ScreenFader component");
            _blockMaterial = BlockObject.material;
            GenerateFadeCamera();
        }

        private void GenerateFadeCamera() {
            _overlayCamera = new GameObject("OverlayCamera").AddComponent<Camera>();
            _overlayCamera.transform.SetParent(XRManager.Instance.XRCam.transform, false);
            _overlayCamera.transform.localPosition = Vector3.zero;
            _overlayCamera.transform.localRotation = Quaternion.identity;
            
            // Set up rendering properties
            int msgPanelLayer = LayerMask.NameToLayer(Constants.MsgPanelLayerName);
            if (msgPanelLayer == -1) {
                Debug.LogError($"Layer '{nameof(msgPanelLayer)}' not found. Add this in the Unity editor project with the Edia Configurator");
                Experiment.Instance.ShowMessageToExperimenter("MessagePanel layer not found! See log for details. ");
            }

            _overlayCamera.cullingMask = LayerMask.GetMask(Constants.MsgPanelLayerName);
            _overlayCamera.clearFlags    = CameraClearFlags.Depth; // Only render specified layers without clearing color
            _overlayCamera.nearClipPlane = 0.01f;
            _overlayCamera.enabled       = false;

            Camera baseCamera = XRManager.Instance.XRCam.GetComponent<Camera>();
            baseCamera.cullingMask |= 1 << LayerMask.NameToLayer(Constants.MsgPanelLayerName);
            
            // For Universal Render Pipeline (URP)
            if (IsUsingUniversalRenderPipeline()) {
                _overlayCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
                baseCamera.GetUniversalAdditionalCameraData().cameraStack.Add(_overlayCamera);
            }
            else {
                _overlayCamera.depth = baseCamera.depth + 1;
            }
        }

        private bool IsUsingUniversalRenderPipeline() {
            // Check if we can find the URP assembly
            var assembly = System.AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Unity.RenderPipelines.Universal.Runtime");

            if (assembly == null)
                return false;

            // Check if the current pipeline is URP
            try {
                var rpAssetType      = assembly.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset");
                var graphicsSettings = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
                return graphicsSettings != null && rpAssetType.IsInstanceOfType(graphicsSettings);
            }
            catch {
                return false;
            }
        }

#region Fade black in

        public Coroutine StartFadeBlackIn() {
            StopAllCoroutines();
            _speed = FadeSpeed;
            return StartCoroutine(FadeBlackIn());
        }

        /// <summary>Fade to black with given speed</summary>
        /// <param name="fadeSpeed">Default = 1</param>
        public Coroutine StartFadeBlackIn(float fadeSpeed) {
            StopAllCoroutines();
            
            _overlayCamera.enabled = true;
            XRManager.Instance.MoveXRRigToOverlayLayer(Constants.MsgPanelLayerName);
            
            _speed = fadeSpeed < 0 ? _speed : fadeSpeed;
            return StartCoroutine(FadeBlackIn());
        }

        IEnumerator FadeBlackIn() {
            BlockObject.enabled = true;

            while (_intensity <= 1.0f) {
                _intensity           += _speed * Time.deltaTime;
                _blockMaterial.color =  new Color(FadeColor.r, FadeColor.g, FadeColor.b, _intensity);
                yield return null;
            }

            _blockMaterial.color = new Color(FadeColor.r, FadeColor.g, FadeColor.b, 1f);
            _isFaded             = true;
        }

#endregion
#region Fade black out

        public Coroutine StartFadeBlackOut() {
            StopAllCoroutines();
            _speed = FadeSpeed;
            return StartCoroutine(FadeBlackOut());
        }

        public Coroutine StartFadeBlackOut(float fadeSpeed) {
            StopAllCoroutines();
            _speed = fadeSpeed < 0 ? _speed : fadeSpeed;
            return StartCoroutine(FadeBlackOut());
        }

        public void HideBlocking() {
            if (!_isFaded)
                return;

            _overlayCamera.enabled = false;
            XRManager.Instance.MoveXRRigToOverlayLayer("Default");
            
            StopAllCoroutines();
            _blockMaterial.color = new Color(FadeColor.r, FadeColor.g, FadeColor.b, 0);
            BlockObject.enabled  = false;
        }

        private IEnumerator FadeBlackOut() {
            while (_intensity >= 0.0f) {
                _intensity           -= _speed * Time.deltaTime;
                _blockMaterial.color =  new Color(FadeColor.r, FadeColor.g, FadeColor.b, _intensity);
                yield return null;
            }

            HideBlocking();

            _isFaded = false;
        }

#endregion
    }
}