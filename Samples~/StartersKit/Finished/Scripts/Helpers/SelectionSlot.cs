using UnityEngine;
using UnityEngine.Events;
using Edia.UI;

namespace StartersKit {
    public class SelectionSlot : MonoBehaviour {

        public HorizontalTimer Timer;
        public MeshRenderer    ColorPlate;

        [Space(10)]
        public UnityEvent<int> Selected;

        private float _duration   = 6f;
        private int   _myColorIdx = 0;

        public void Init(Color color, int colorIdx, float duration) {
            _duration                 = duration;
            _myColorIdx               = colorIdx;
            ColorPlate.material.color = color;
            gameObject.SetActive(true);
        }

        private void OnTriggerEnter(Collider other) {
            Timer.TimerDone.AddListener(TimerDone);
            Timer.StartTimer(_duration);
        }

        private void OnTriggerExit(Collider other) {
            Timer.StopTimer();
            StopListening();
        }

        private void TimerDone() {
            Selected?.Invoke(_myColorIdx);
        }

        // --------------------------------------------------------

        private void OnDestroy() {
            StopListening();
        }

        private void OnDisable() {
            StopListening();
        }

        private void StopListening() {
            Timer.TimerDone.RemoveAllListeners();
        }

    }
}