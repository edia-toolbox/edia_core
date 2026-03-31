using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Edia.UI {

    /// <summary>
    /// UI Element that visually shows a timer and fired events when the timer reaches 0.
    /// </summary>
    public class HorizontalTimer : MonoBehaviour {

        private Slider TimerSlider;

        [Space(20)]
        public UnityEvent TimerDone;

        // Local
        private float     _duration = 6f;
        private Coroutine _timerCoroutine;

        private List<GameObject> AllChildren() {
            List<GameObject> children = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++) {
                children.Add(transform.GetChild(i).gameObject);
            }

            children.RemoveAt(0);
            return children;
        }

        private void Awake() {
            TimerSlider = GetComponent<Slider>();
            ActivateChildObjects(false);
        }

        private void ActivateChildObjects(bool onOff) {
            foreach (var child in AllChildren()) {
                child.SetActive(onOff);
            }
        }

        public void StartTimer(float duration) {
            if (_timerCoroutine != null)
                StopCoroutine(_timerCoroutine);

            _duration                = duration;
            TimerSlider.maxValue     = _duration;
            TimerSlider.value        = _duration;
            TimerSlider.minValue     = 0f;
            TimerSlider.wholeNumbers = false;

            ActivateChildObjects(true);
            _timerCoroutine = StartCoroutine(TimerRunning());
        }

        public void StopTimer() {
            if (_timerCoroutine != null)
                StopCoroutine(_timerCoroutine);

            ActivateChildObjects(false);
            TimerSlider.value = _duration;
        }

        private System.Collections.IEnumerator TimerRunning() {
            float _timer = _duration;

            while (_timer > 0f) {
                _timer            -= Time.deltaTime;
                TimerSlider.value =  _timer;
                yield return null;
            }

            TimerDone?.Invoke();
            StopTimer();
        }
    }
}