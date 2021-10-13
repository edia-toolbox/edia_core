using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Tobii.XR.Examples
{
    public class PulsateColor : MonoBehaviour
    {
#pragma warning disable 649 // Field is never assigned
        [SerializeField] private Color normalColor;
        [SerializeField] private Color pulsateColor;
        [SerializeField] private Image image;
        [SerializeField] private float duration;
#pragma warning restore 649

        private Color _fromColor;
        private Color _toColor;
        private float _time;

        private void Start()
        {
            _fromColor = normalColor;
            _toColor = pulsateColor;
            _time = 0;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            var elapsedAmount = _time / duration;
            var r = Mathf.SmoothStep(_fromColor.r, _toColor.r, elapsedAmount);
            var g = Mathf.SmoothStep(_fromColor.g, _toColor.b, elapsedAmount);
            var b = Mathf.SmoothStep(_fromColor.b, _toColor.b, elapsedAmount);
            image.color = new Color(r, g, b);

            if (_time > duration)
            {
                _time = 0;
                var previousFromColor = _fromColor;
                _fromColor = _toColor;
                _toColor = previousFromColor;
            }
        }
    }
}