//Copyright © 2018 – Property of Tobii AB(publ) - All Rights Reserved

using UnityEngine;

namespace Tobii.XR
{
    public static class EyeTrackingDataHelper
    {
        public static void Copy(TobiiXR_EyeTrackingData src, TobiiXR_EyeTrackingData dest)
        {
            dest.Timestamp = src.Timestamp;
            dest.GazeRay = src.GazeRay;
            dest.ConvergenceDistance = src.ConvergenceDistance;
            dest.ConvergenceDistanceIsValid = src.ConvergenceDistanceIsValid;
            dest.IsLeftEyeBlinking = src.IsLeftEyeBlinking;
            dest.IsRightEyeBlinking = src.IsRightEyeBlinking;
        }

        public static void Copy(TobiiXR_AdvancedEyeTrackingData src, TobiiXR_AdvancedEyeTrackingData dest)
        {
            dest.SystemTimestamp = src.SystemTimestamp;
            dest.DeviceTimestamp = src.DeviceTimestamp;
            dest.GazeRay = src.GazeRay;
            dest.ConvergenceDistance = src.ConvergenceDistance;
            dest.ConvergenceDistanceIsValid = src.ConvergenceDistanceIsValid;
            dest.Left = src.Left;
            dest.Right = src.Right;
        }

        public static TobiiXR_EyeTrackingData Clone(TobiiXR_EyeTrackingData data)
        {
            var result = new TobiiXR_EyeTrackingData();
            Copy(data, result);
            return result;
        }

        public static void CopyAndTransformGazeData(TobiiXR_EyeTrackingData src, TobiiXR_EyeTrackingData dest, Matrix4x4 transformMatrix)
        {
            Copy(src, dest);
            if (src.GazeRay.IsValid)
            {
                dest.GazeRay.Origin = transformMatrix.MultiplyPoint(src.GazeRay.Origin);
                dest.GazeRay.Direction = transformMatrix.MultiplyVector(src.GazeRay.Direction);
            }
        }

        public static void TransformGazeData(TobiiXR_EyeTrackingData data, Matrix4x4 transformMatrix)
        {
            data.GazeRay.Origin = transformMatrix.MultiplyPoint(data.GazeRay.Origin);
            data.GazeRay.Direction = transformMatrix.MultiplyVector(data.GazeRay.Direction);
        }
    }
}
