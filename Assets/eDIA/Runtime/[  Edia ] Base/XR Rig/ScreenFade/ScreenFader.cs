using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Edia {

    /// <summary> Handles fading the camera view </summary>
    public class ScreenFader : MonoBehaviour
    {
		[SerializeField] private float _defaultSpeed = 1f;
		private float _speed = 1f;
        private float _intensity = 0.0f;
        private Color _color = Color.black;
		public Image FadeImage = null;
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
			FadeImage.color = new Color(_color.r, _color.g, _color.b, 0f);
			FadeImage.enabled = false;
		}

        public Coroutine StartFadeBlackIn()
        {
            StopAllCoroutines();
			_speed = _defaultSpeed;
			return StartCoroutine(FadeBlackIn());
        }

        /// <summary>Fade to black with given speed</summary>
        /// <param name="_fadeSpeed">Default = 1</param>
		public Coroutine StartFadeBlackIn(float _fadeSpeed) {
			StopAllCoroutines();
            _speed = _fadeSpeed == -1 ? _speed : _fadeSpeed;
			return StartCoroutine(FadeBlackIn());
		}

		IEnumerator FadeBlackIn()
        {
            FadeImage.enabled = true;

            while (_intensity <= 1.0f)
            {
                _intensity += _speed * Time.deltaTime;
                FadeImage.color = new Color(_color.r, _color.g, _color.b, _intensity);
                yield return null;
            }

			FadeImage.color = new Color(_color.r, _color.g, _color.b, 1f);
            isBlack = true;
		}

        public Coroutine StartFadeBlackOut()
        {
            StopAllCoroutines();
            _speed = _defaultSpeed;
            return StartCoroutine(FadeBlackOut());
        }

		public Coroutine StartFadeBlackOut(float _fadeSpeed) {
			StopAllCoroutines();
			_speed = _fadeSpeed == -1 ? _speed : _fadeSpeed;
			//this.Add2Console("fading to VR with " + _speed);
			return StartCoroutine(FadeBlackOut());
		}

		private IEnumerator FadeBlackOut()
        {
            while (_intensity >= 0.0f)
            {
                _intensity -= _speed * Time.deltaTime;
				FadeImage.color = new Color(_color.r, _color.g, _color.b, _intensity);
				yield return null;
            }

            HideBlockingImage();

			isBlack = false;
        }
    }
}

