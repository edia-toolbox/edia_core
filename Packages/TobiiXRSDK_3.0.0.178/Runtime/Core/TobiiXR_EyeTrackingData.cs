// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System;
using UnityEngine;

namespace Tobii.XR
{
    [Serializable]
    public struct TobiiXR_GazeRay
    {
        /// <summary>
        /// Unit vector describing the direction of the eye.
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// True when the origin and direction of the gaze ray is valid.
        /// </summary>
        public bool IsValid;

        /// <summary>
        /// The 3D position of the origin of the gaze ray given in meters.
        /// </summary>
        public Vector3 Origin;
    }

    /// <summary>
    /// Stores eye data.
    /// </summary>
    public class TobiiXR_EyeTrackingData
    {
        /// <summary>
        /// Timestamp when the package was received measured in seconds from the application started.
        /// </summary>
        public float Timestamp;

        /// <summary>
        /// Convergence distance is the distance in meters from the middle of your eyes to the point where the gaze of your eyes meet.
        /// </summary>
        public float ConvergenceDistance;

        /// <summary>
        /// This flag is true when the Convergence Distance value can be used.
        /// </summary>
        public bool ConvergenceDistanceIsValid;

        /// <summary>
        /// Stores origin and direction of the gaze ray.
        /// </summary>
        public TobiiXR_GazeRay GazeRay;

        /// <summary>
        /// Flag for closed left eye.
        /// </summary>
        public bool IsLeftEyeBlinking;

        /// <summary>
        /// Flag for closed right eye.
        /// </summary>
        public bool IsRightEyeBlinking;
    }
}