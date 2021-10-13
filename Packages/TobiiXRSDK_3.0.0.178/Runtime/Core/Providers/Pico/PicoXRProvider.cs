using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Debug = UnityEngine.Debug;

namespace Tobii.XR
{
    [ProviderDisplayName("Pico XR"), SupportedPlatform(XRBuildTargetGroup.Android)]
    public class PicoXRProvider : IEyeTrackingProvider
    {
        private readonly InputFeatureUsage<Vector3> _gazeOriginFeature = new InputFeatureUsage<Vector3>("CombinedEyeGazePoint");
        private readonly InputFeatureUsage<Vector3> _gazeDirectionFeature = new InputFeatureUsage<Vector3>("CombinedEyeGazeVector");
        private readonly InputFeatureUsage<float> _leftEyeOpennessFeature = new InputFeatureUsage<float>("LeftEyeOpenness");
        private readonly InputFeatureUsage<float> _rightEyeOpennessFeature = new InputFeatureUsage<float>("RightEyeOpenness");
        private InputDevice _eyeTrackingDevice;
        private readonly TobiiXR_EyeTrackingData _eyeTrackingDataLocal = new TobiiXR_EyeTrackingData();

        public void GetEyeTrackingDataLocal(TobiiXR_EyeTrackingData data)
        {
            EyeTrackingDataHelper.Copy(_eyeTrackingDataLocal, data);
        }

        public Matrix4x4 LocalToWorldMatrix => CameraHelper.GetCameraTransform().localToWorldMatrix;

        public bool Initialize()
        {
            return XRSettings.loadedDeviceName.Contains("Pico");
        }

        public void Tick()
        {
            // The eye tracker input device is usually not visible first frame so retry this until eye tracker found
            if (!_eyeTrackingDevice.isValid)
            {
                var devices = new List<InputDevice>();
                InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.HeadMounted, devices);
                _eyeTrackingDevice = devices.FirstOrDefault();
            }
            
            if (!_eyeTrackingDevice.isValid) return;

            _eyeTrackingDataLocal.GazeRay.IsValid = false;
            if (!_eyeTrackingDevice.TryGetFeatureValue(_gazeOriginFeature, out var origin)) return;
            if (!_eyeTrackingDevice.TryGetFeatureValue(_gazeDirectionFeature, out var direction)) return;

            _eyeTrackingDataLocal.GazeRay.IsValid = true;
            _eyeTrackingDataLocal.GazeRay.Origin = origin;
            _eyeTrackingDataLocal.GazeRay.Direction = direction;

            _eyeTrackingDataLocal.IsLeftEyeBlinking = _eyeTrackingDevice.TryGetFeatureValue(_leftEyeOpennessFeature, out var leftEyeOpenness) && Mathf.Approximately(leftEyeOpenness, 0.0f);
            _eyeTrackingDataLocal.IsRightEyeBlinking = _eyeTrackingDevice.TryGetFeatureValue(_rightEyeOpennessFeature, out var rightEyeOpenness) && Mathf.Approximately(rightEyeOpenness, 0.0f);
            _eyeTrackingDataLocal.Timestamp = Time.unscaledTime;
        }

        public void Destroy()
        {
        }
    }
}