using System;
using UnityEngine;

namespace Tobii.XR
{
    /// <summary>
    /// Contains advanced eye tracking signals.
    /// </summary>
    [Serializable]
    public class TobiiXR_AdvancedEyeTrackingData
    {
        /// <summary>
        /// Timestamp, synchronized to host system clock, when the data used to calculate eye tracking signals was
        /// captured. Measured in microseconds. Not guaranteed to be monotonic. If monotonic time is important to
        /// your use case, use <see cref="TobiiXR_AdvancedTimesyncData"/> to calculate your own system timestamp
        /// from <see cref="DeviceTimestamp"/>.
        /// If the eye tracker uses a different clock than the host system, the accuracy of this timestamp relies
        /// on <see cref="TobiiXRAdvanced.StartTimesyncJob"/> being called regularly.
        /// </summary>
        /// <remarks>
        /// To get the current time with the host system clock use <see cref="TobiiXRAdvanced.GetSystemTimestamp"/>.
        /// To calculate the eye tracker system latency, take current time from host system clock minus the system
        /// timestamp from the advanced eye tracking data package.
        /// </remarks>
        public long SystemTimestamp;
        
        /// <summary>
        /// Timestamp, from the eye tracker clock, when the data used to calculate eye tracking signals was captured.
        /// Measured in microseconds. Guaranteed to be monotonic.
        /// Depending on the eye tracker integration, the clock being used by the eye tracker may not be the same as
        /// the clock being used by the system. In that case you will need to synchronize the clocks to get accurate
        /// timestamps in system time.
        /// </summary>
        public long DeviceTimestamp;

        /// <summary>
        /// Stores eye tracking data specific to the left eye.
        /// </summary>
        public TobiiXR_AdvancedPerEyeData Left;

        /// <summary>
        /// Stores eye tracking data specific to the right eye.
        /// </summary>
        public TobiiXR_AdvancedPerEyeData Right;

        /// <summary>
        /// Stores origin and direction of a virtual gaze ray that originates from a point between the user's
        /// eyes and is compensated on a best-effort level for loss of tracking of a single eye.
        /// </summary>
        public TobiiXR_GazeRay GazeRay;

        /// <summary>
        /// Convergence distance is the distance in meters from the middle of your eyes to the point where the
        /// gaze of your eyes meet.
        /// </summary>
        public float ConvergenceDistance;

        /// <summary>
        /// This flag is true when the Convergence Distance value can be used.
        /// </summary>
        public bool ConvergenceDistanceIsValid;
    }

    /// <summary>
    /// Contains advanced eye tracking signals that have a separate value per eye.
    /// </summary>
    [Serializable]
    public struct TobiiXR_AdvancedPerEyeData
    {
        /// <summary>
        /// Stores origin and direction of the gaze ray.
        /// </summary>
        public TobiiXR_GazeRay GazeRay;

        /// <summary>
        /// True if the user's eye is closed. False if open.
        /// </summary>
        public bool IsBlinking;

        /// <summary>
        /// True when pupil diameter value is valid.
        /// </summary>
        public bool PupilDiameterValid;

        /// <summary>
        /// Unit is mm, but it is only reliable to measure relative changes of the pupil diameter unless your
        /// eye tracker hardware specification says otherwise. Most eye trackers cannot compensate for the
        /// magnification caused by a pair of eye glasses. 
        /// </summary>
        public float PupilDiameter;

        /// <summary>
        /// True when position guide value is valid.
        /// </summary>
        public bool PositionGuideValid;

        /// <summary>
        /// Normalized (0.0-1.0) position of pupil in relation to the optical axis of the lens, where 0.5, 0.5
        /// means the eye aligns with the optical axis. This information is useful to tell you if the user
        /// is wearing the headset correctly and if the user's interpupillary distance matches the lens separation.
        /// </summary>
        public Vector2 PositionGuide;
    }

    /// <summary>
    /// Contains the result of a completed timesync operation.
    /// </summary>
    [Serializable]
    public struct TobiiXR_AdvancedTimesyncData
    {
        /// <summary>
        /// Timestamp, from system clock, when the sync operation started.
        /// Unit is microseconds.
        /// </summary>
        public long StartSystemTimestamp;

        /// <summary>
        /// Timestamp, from system clock, when the sync operation ended.
        /// Unit is microseconds.
        /// </summary>
        public long EndSystemTimestamp;

        /// <summary>
        /// Timestamp, from the clock used by the eye tracker, when the sync package was received by the eye tracker.
        /// Unit is microseconds. 
        /// </summary>
        public long DeviceTimestamp;
    }

    /// <summary>
    /// Metadata about the eye tracker
    /// </summary>
    [Serializable]
    public struct TobiiXR_EyeTrackerMetadata
    {
        /// <summary>
        /// Serial number is unique to the eye tracker. However, for most eye tracker models, the first five
        /// characters are the same for all devices of the same model.
        /// </summary>
        public string SerialNumber;

        /// <summary>
        /// The eye tracker model name.
        /// </summary>
        public string Model;

        /// <summary>
        /// Version of the Tobii Runtime currently being used to connect to the eye tracker.
        /// </summary>
        public string RuntimeVersion;

        /// <summary>
        /// The expected frequency with which the eye tracker will deliver data.
        /// </summary>
        public string OutputFrequency;
    }
}