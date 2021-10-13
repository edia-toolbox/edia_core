using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Tobii.XR.Examples
{
    public class DataTransparencyPopup : MonoBehaviour
    {
#pragma warning disable 649 // Field is never assigned
        [SerializeField] private GameObject dataTransparencyUiCanvas;
        [SerializeField] private float maxViewAngle = 20f;
        [SerializeField] private float preferredDistance = 2f;
        [SerializeField] private float clampBottomY = .74f;
        [SerializeField] private float slerpRate = .1f;
#pragma warning restore 649

        private Quaternion _targetRot;
        private Vector3 _targetPos;
        private Transform _camera;

        private void Start()
        {
            _camera = CameraHelper.GetCameraTransform();
        }

        private void Update()
        {
            var angleBetweenHeadsetAndPopup = Vector3.Angle(dataTransparencyUiCanvas.transform.position - _camera.transform.position, _camera.transform.forward);
            if (angleBetweenHeadsetAndPopup > maxViewAngle) // if too far from center of view
            {
                MovePopupIntoFov(); // set target location back inside user's field of view
            }

            dataTransparencyUiCanvas.transform.position = Vector3.Slerp(dataTransparencyUiCanvas.transform.position, _targetPos, slerpRate);
            dataTransparencyUiCanvas.transform.rotation = Quaternion.Slerp(dataTransparencyUiCanvas.transform.rotation, _targetRot, slerpRate);
        }

        private void MovePopupIntoFov()
        {
            _targetPos = _camera.transform.position + _camera.transform.forward * preferredDistance; // project ahead of gaze 

            if (_targetPos.y < clampBottomY)
                _targetPos.Scale(new Vector3(1, 0, 1)); // wipe out y level

            while (_targetPos.y < clampBottomY) // keep bumping it up until we hit minimum desired location
            {
                _targetPos += new Vector3(0, clampBottomY, 0);
                _targetPos += Vector3.Scale(_camera.transform.forward, new Vector3(1, 0, 1));
            }

            Quaternion newRotation = Quaternion.LookRotation(_targetPos - _camera.transform.position); // face user
            _targetRot.eulerAngles.Set(_targetRot.eulerAngles.x, _targetRot.eulerAngles.y, 0);
            _targetRot = newRotation;
        }

        public void QuitApplication()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void LoadNextScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}