using UnityEngine;
using UnityEngine.XR.Management;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Tobii.XR
{
    public enum ControllerButton
    {
        Menu,
        Touchpad,
        TouchpadTouch,
        Trigger
    }

    public interface IControllerAdapter
    {
        /// <summary>
        /// The velocity of the controller.
        /// </summary>
        Vector3 Velocity { get; }

        /// <summary>
        /// The angular velocity of the controller.
        /// </summary>
        Vector3 AngularVelocity { get; }

        /// <summary>
        /// The position of the controller in world space. 
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// The rotation of the controller in world space.
        /// </summary>
        Quaternion Rotation { get; }

        /// <summary>
        /// Is a given button pressed or not during this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if button is being pressed this frame, otherwise false.</returns>
        bool GetButtonPress(ControllerButton button);

        /// <summary>
        /// Did a button go from not pressed to pressed this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button went from being up to being pressed down this frame, otherwise false.</returns>
        bool GetButtonPressDown(ControllerButton button);

        /// <summary>
        /// Did a button go from pressed to not pressed this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button went from being pressed down to not being pressed this frame, otherwise false.</returns>
        bool GetButtonPressUp(ControllerButton button);

        /// <summary>
        /// Is a given button being touched or not during this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if button is being touched this frame, otherwise false.</returns>
        bool GetButtonTouch(ControllerButton button);

        /// <summary>
        /// Did a button go from not being touched to being touched this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button went from not being touched to being touched this frame, otherwise false.</returns>
        bool GetButtonTouchDown(ControllerButton button);

        /// <summary>
        /// Did a button go from being touched to not touched this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button went from being touched to not being touched this frame, otherwise false.</returns>
        bool GetButtonTouchUp(ControllerButton button);

        /// <summary>
        /// Trigger a haptic pulse on the controller.
        /// </summary>
        /// <param name="hapticStrength">The normalized (0 to 1) strength of the haptic impulse to play on the device.</param>
        void TriggerHapticPulse(float hapticStrength);

        /// <summary>
        /// Get the touchpad touch position.
        /// </summary>
        /// <returns>Vector2 with the thumb's position on the touchpad.</returns>
        Vector2 GetTouchpadAxis();

        void Update();
    }

    internal class DummyControllerAdapter : IControllerAdapter
    {
        public Vector3 Velocity { get; }
        public Vector3 AngularVelocity { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public bool GetButtonPress(ControllerButton button)
        {
            return false;
        }

        public bool GetButtonPressDown(ControllerButton button)
        {
            return false;
        }

        public bool GetButtonPressUp(ControllerButton button)
        {
            return false;
        }

        public bool GetButtonTouch(ControllerButton button)
        {
            return false;
        }

        public bool GetButtonTouchDown(ControllerButton button)
        {
            return false;
        }

        public bool GetButtonTouchUp(ControllerButton button)
        {
            return false;
        }

        public void TriggerHapticPulse(float durationSeconds)
        {
        }

        public Vector2 GetTouchpadAxis()
        {
            return Vector2.zero;
        }

        public void Update()
        {
        }
    }

    [DefaultExecutionOrder(-4000)]
    public class ControllerManager : MonoBehaviour
    {
        private Transform _roomOrigin;
        private static GameObject _gameObject;
        private static ControllerManager _instance;
        private static IControllerAdapter _controllerAdapter = new DummyControllerAdapter();

        private Vector3 _lastPosition = Vector3.zero;
        private Quaternion _lastRotation = Quaternion.identity;
        private Vector3 _velocity;
        private Vector3 _angularVelocity;
        private const float VelocitySmoothingAlpha = 0.3f;

        /// <summary>
        /// Instance of the controller manager which can be statically accessed.
        /// </summary>
        public static ControllerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _gameObject = new GameObject("TobiiXR Controller Manager");
                    _instance = _gameObject.AddComponent<ControllerManager>();
                }

                return _instance;
            }
        }

        private static void SetAdapter()
        {
            if (XRGeneralSettings.Instance.Manager.activeLoader == null) return; // No XR loaded yet

#if OPENVR_ENABLED && !UNITY_ANDROID
            if (XRGeneralSettings.Instance.Manager.activeLoader.name.Contains("Open VR"))
            {
                _controllerAdapter = new OpenVRControllerAdapter();
            }
            else
            {
                _controllerAdapter = new XRInputSystemControllerAdapter();
            }
#else
            _controllerAdapter = new XRInputSystemControllerAdapter();
#endif
        }

        private void Awake()
        {
            _roomOrigin = FindRoomOrigin();
            if (_controllerAdapter == null) SetAdapter();

            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        private void Update()
        {
            VelocityUpdate();
            if (_controllerAdapter.GetType() == typeof(DummyControllerAdapter)) SetAdapter();
            _controllerAdapter.Update();
        }

        private void VelocityUpdate()
        {
            // Velocity
            var velocity = (_controllerAdapter.Position - _lastPosition) / Time.deltaTime;
            _velocity = VelocitySmoothingAlpha * velocity + (1 - VelocitySmoothingAlpha) * _velocity;

            // Angular rotation
            var velocityDiff = (_controllerAdapter.Rotation * Quaternion.Inverse(_lastRotation));
            var angularVelocity = (new Vector3(Mathf.DeltaAngle(0f, velocityDiff.eulerAngles.x), Mathf.DeltaAngle(0f, velocityDiff.eulerAngles.y), Mathf.DeltaAngle(0f, velocityDiff.eulerAngles.z)) / Time.deltaTime) * Mathf.Deg2Rad;
            _angularVelocity = VelocitySmoothingAlpha * angularVelocity + (1 - VelocitySmoothingAlpha) * _angularVelocity;

            _lastPosition = _controllerAdapter.Position;
            _lastRotation = _controllerAdapter.Rotation;
        }

        public Vector3 Velocity => _velocity;

        public Vector3 AngularVelocity => _angularVelocity;

        public Vector3 Position => _roomOrigin.TransformPoint(_controllerAdapter.Position);

        public Quaternion Rotation => _roomOrigin.rotation * _controllerAdapter.Rotation;

        private static Transform FindRoomOrigin()
        {
            var cam = CameraHelper.GetMainCamera().transform;
            if (cam.root == null)
            {
                Debug.LogError("ControllerManager is only supported when camera is placed under an XR Rig");
            }

            return cam.root;
        }

        public bool GetButtonPress(ControllerButton button)
        {
            return _controllerAdapter.GetButtonPress(button);
        }

        public bool GetButtonPressDown(ControllerButton button)
        {
            return _controllerAdapter.GetButtonPressDown(button);
        }

        public bool GetButtonPressUp(ControllerButton button)
        {
            return _controllerAdapter.GetButtonPressUp(button);
        }

        public bool GetButtonTouch(ControllerButton button)
        {
            return _controllerAdapter.GetButtonTouch(button);
        }

        public bool GetButtonTouchDown(ControllerButton button)
        {
            return _controllerAdapter.GetButtonTouchDown(button);
        }

        public bool GetButtonTouchUp(ControllerButton button)
        {
            return _controllerAdapter.GetButtonTouchUp(button);
        }

        public void TriggerHapticPulse(float hapticStrength)
        {
            _controllerAdapter.TriggerHapticPulse(hapticStrength);
        }

        public Vector2 GetTouchpadAxis()
        {
            return _controllerAdapter.GetTouchpadAxis();
        }

        public bool AnyTriggerHeld()
        {
            if (GetButtonPress(ControllerButton.Trigger)) return true;
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current.spaceKey.isPressed || (Joystick.current?.trigger.isPressed).GetValueOrDefault(false);
#else
            return Input.GetKey(KeyCode.JoystickButton0) || Input.GetKey(KeyCode.Space);
#endif
        }
        
        public bool AnyTriggerPressed()
        {
            if (GetButtonPressDown(ControllerButton.Trigger)) return true;
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current.spaceKey.wasPressedThisFrame || (Joystick.current?.trigger.wasPressedThisFrame).GetValueOrDefault(false);
#else
            return Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Space);
#endif
        }
    }
}