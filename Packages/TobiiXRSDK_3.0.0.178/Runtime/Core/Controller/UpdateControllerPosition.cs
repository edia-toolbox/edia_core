using UnityEngine;

namespace Tobii.XR
{
    public class UpdateControllerPosition : MonoBehaviour
    {
        private Transform _transform;

        private void Start()
        {
            _transform = transform;
        }

        private void Update()
        {
            _transform.position = ControllerManager.Instance.Position;
            _transform.rotation = ControllerManager.Instance.Rotation;
        }
    }
}