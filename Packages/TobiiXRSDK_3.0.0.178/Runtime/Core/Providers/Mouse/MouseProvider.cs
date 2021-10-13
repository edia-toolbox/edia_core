// Copyright © 2019 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;
using UnityEngine.XR;

namespace Tobii.XR
{
    /// <summary>
    /// Provides emulated gaze data to TobiiXR using the mouse. 
    /// Only use this provider for debugging in Unity Editor.
    /// </summary>
    [ProviderDisplayName("Mouse")]
    public class MouseProvider : IEyeTrackingProvider
    {
        private const float OpenVRVisibleRatio = 0.7f; // This is a magic number that describes how much of the OpenVR eye buffer is visible in editor window. Surprisingly it doesn't change when super sampling ratio is changed.
        private const float OpenVRVisibleOffset = 0.15f;
        private readonly TobiiXR_EyeTrackingData _eyeTrackingDataLocal = new TobiiXR_EyeTrackingData();

        public Matrix4x4 LocalToWorldMatrix
        {
            get { return CameraHelper.GetCameraTransform().localToWorldMatrix; }
        }

        public void GetEyeTrackingDataLocal(TobiiXR_EyeTrackingData data)
        {
            EyeTrackingDataHelper.Copy(_eyeTrackingDataLocal, data);
        }

        public bool Initialize()
        {
            if (XRDisplaySubSystemHelper.IsRunning())
            {
                Debug.LogWarning("Mouse provider will give wrong screen coordinates when XR is turned on. Turn off XR when you want to test with mouse provider.");
            }

            return true;
        }

        public void Tick()
        {
            var cam = CameraHelper.GetMainCamera();
            Ray mouseRay;
            if (XRSettings.enabled && XRSettings.loadedDeviceName == "OpenVR")
            {
                // OpenVR crops the game view window so these complex calculations are needed to compensate
                var screenPos = Vector3.zero;
                var u = Input.mousePosition.x / Screen.width;
                var v = Input.mousePosition.y / Screen.height;

                // Handle aspect mismatch between window and eye buffer
                var eyeAspect = (float) XRSettings.eyeTextureWidth / XRSettings.eyeTextureHeight;
                var screenAspect = (float) Screen.width / Screen.height;
                var visibleWidthRatio = 1.0f;
                var visibleHeightRatio = 1.0f;
                if (screenAspect > eyeAspect) visibleHeightRatio = eyeAspect / screenAspect;
                else visibleWidthRatio = 1 / (eyeAspect / screenAspect);

                // Pad with half aspect mismatch overflow, if any
                var overflowX = 0.5f * (1.0f - visibleWidthRatio) * OpenVRVisibleRatio * XRSettings.eyeTextureWidth;
                var overflowY = 0.5f * (1.0f - visibleHeightRatio) * OpenVRVisibleRatio * XRSettings.eyeTextureHeight;

                screenPos.x = overflowX + OpenVRVisibleOffset * XRSettings.eyeTextureWidth + visibleWidthRatio * OpenVRVisibleRatio * XRSettings.eyeTextureWidth * u;
                screenPos.y = overflowY + OpenVRVisibleOffset * XRSettings.eyeTextureHeight + visibleHeightRatio * OpenVRVisibleRatio * XRSettings.eyeTextureHeight * v;
                mouseRay = cam.ScreenPointToRay(screenPos, cam.stereoActiveEye);
            }
            else
            {
                mouseRay = cam.ScreenPointToRay(Input.mousePosition);
            }

            _eyeTrackingDataLocal.Timestamp = Time.unscaledTime;

            // Mouse ray is in world space so transform it to local space
            var mat = cam.transform.worldToLocalMatrix;
            var origin = mat.MultiplyPoint3x4(mouseRay.origin);
            var dir = mat.MultiplyVector(mouseRay.direction.normalized);

            // Reproject mouse ray from camera origin to make it more similar to combined gaze ray
            var projectedPoint = origin + 2 * (dir / dir.z);
            _eyeTrackingDataLocal.GazeRay.Origin = Vector3.zero;
            _eyeTrackingDataLocal.GazeRay.Direction = projectedPoint.normalized;
            _eyeTrackingDataLocal.GazeRay.IsValid = true;
        }

        public void Destroy()
        {
        }
    }
}