using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Edia {

    /// <summary> Handles fading the camera view </summary>
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] private float _speed = 1.0f;
        [SerializeField] private float _intensity = 0.0f;
        [SerializeField] private Color _color = Color.black;
		public Image _fadeImage = null;
		bool isBlack = false;


        private void OnDrawGizmos() { // Draw HMD gizmo
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(Vector3.zero, 0.1f);
            Gizmos.DrawCube(Vector3.zero + new Vector3(0,0,0.08f),new Vector3(0.125f,0.075f,0.075f));
            Gizmos.DrawLine(Vector3.zero, Vector3.forward);
        }

        public void HideBlockingImage() {
            if (!isBlack)
                return;

            StopAllCoroutines();
			_fadeImage.color = new Color(_color.r, _color.g, _color.b, 0f);
			_fadeImage.enabled = false;
		}

        public Coroutine StartFadeBlackIn()
        {
            StopAllCoroutines();
            return StartCoroutine(FadeBlackIn());
        }

		IEnumerator FadeBlackIn()
        {
            _fadeImage.enabled = true;

            while (_intensity <= 1.0f)
            {
                _intensity += _speed * Time.deltaTime;
                _fadeImage.color = new Color(_color.r, _color.g, _color.b, _intensity);
                yield return null;
            }

			_fadeImage.color = new Color(_color.r, _color.g, _color.b, 1f);
            isBlack = true;
		}

        public Coroutine StartFadeBlackOut()
        {
            StopAllCoroutines();
            return StartCoroutine(FadeBlackOut());
        }

        private IEnumerator FadeBlackOut()
        {
            while (_intensity >= 0.0f)
            {
                _intensity -= _speed * Time.deltaTime;
				_fadeImage.color = new Color(_color.r, _color.g, _color.b, _intensity);
				yield return null;
            }

            HideBlockingImage();

			isBlack = false;
        }
    }
}

