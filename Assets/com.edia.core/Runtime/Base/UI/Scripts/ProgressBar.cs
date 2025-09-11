using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Edia.UI {

    public class ProgressBar : MonoBehaviour {

        [Header("References")]
        public TextMeshProUGUI currentValueField;
        public TextMeshProUGUI maxValueField;
        public TextMeshProUGUI descriptionField;
        public bool            useCapitals = true;

        Coroutine _animationRoutine = null;
        Slider    _mySlider         = null;

        private void Awake() {
            _mySlider = GetComponent<Slider>();
        }

        public int maxValue {
            get { return (int)_mySlider.maxValue; }

            set {
                _mySlider.maxValue = value;
                maxValueField.text = value.ToString();
            }
        }

        public int currentValue {
            get { return (int)_mySlider.value; }

            set {
                _mySlider.value        = value;
                currentValueField.text = value.ToString();
            }
        }

        public string description {
            get { return descriptionField.text; }

            set { descriptionField.text = useCapitals ? value.ToString().ToUpper() : value.ToString(); }
        }

        // --

        public void StartAnimation(float duration) {
            gameObject.SetActive(true);
            _animationRoutine = StartCoroutine("AnimateSliderOverTime", duration);
        }

        public void StopAnimation() {
            if (_animationRoutine != null)
                StopCoroutine(_animationRoutine);

            _mySlider.value = 0f;
        }

        IEnumerator AnimateSliderOverTime(float duration) {
            float animationTime = 0f;

            while (animationTime < duration) {
                animationTime += Time.deltaTime;
                float lerpValue = animationTime / duration;
                _mySlider.value = Mathf.Lerp(1f, 0f, lerpValue);
                yield return null;
            }
        }

        // --

        public void AnimateToValue(float endValue) {
            _animationRoutine = StartCoroutine("AnimateSliderToValue", endValue);
        }

        void CleanCoroutine() {
            _animationRoutine = null;
        }

        IEnumerator AnimateSliderToValue(float endValue) {
            float animationTime = 0f;
            float duration      = 0.5f;
            float oldValue      = _mySlider.value;

            while (animationTime < duration) {
                animationTime += Time.deltaTime;
                float lerpValue = animationTime / duration;
                _mySlider.value = Mathf.Lerp(oldValue, endValue, lerpValue);
                yield return null;
            }

            CleanCoroutine();
        }

    }
}