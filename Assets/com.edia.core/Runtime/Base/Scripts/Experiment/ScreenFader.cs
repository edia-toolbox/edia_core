using System.Collections;
using UnityEngine;

namespace Edia {

    /// <summary> Handles fading the camera view </summary>
    public class ScreenFader : MonoBehaviour {

        [Header("Refs")]
        public MeshRenderer BlockObject = null;

        [Header("Settings")]
        public Color FadeColor = Color.black;

        public float FadeSpeed = 1f;

        // Internal
        private float _speed         = 1f;
        private float _intensity     = 0.0f;
        private bool  _isFaded       = false;
        Material      _blockMaterial = null;

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
#region Fade block out

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