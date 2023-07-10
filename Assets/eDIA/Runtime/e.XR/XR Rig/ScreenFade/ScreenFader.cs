using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA {

    /// <summary> Handles fading the camera view </summary>
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] private float _speed = 1.0f;
        [SerializeField] private float _intensity = 0.0f;
        [SerializeField] private Color _color = Color.black;
        [SerializeField] private Material _fadeMaterial = null;

        private void OnDrawGizmos() {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(Vector3.zero, 0.1f);
            Gizmos.DrawCube(Vector3.zero + new Vector3(0,0,0.08f),new Vector3(0.125f,0.075f,0.075f));
            Gizmos.DrawLine(Vector3.zero, Vector3.forward);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _fadeMaterial.SetFloat("_Intensity", _intensity);
            _fadeMaterial.SetColor("_FadeColor", _color);
            Graphics.Blit(source, destination, _fadeMaterial);
        }

        public Coroutine StartFadeIn()
        {
            StopAllCoroutines();
            return StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            Debug.Log("StartFadeIn");
            while (_intensity <= 1.0f)
            {
                _intensity += _speed * Time.deltaTime;
                yield return null;
            }
        }

        public Coroutine StartFadeOut()
        {
            StopAllCoroutines();
            return StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            while (_intensity >= 0.0f)
            {
                _intensity -= _speed * Time.deltaTime;
                yield return null;
            }
        }
    }

}

