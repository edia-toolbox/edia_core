///<summary>
/// Interfaces used across EDIA Core and other EDIA packages.
/// </summary>

using UnityEngine;

namespace Edia {

    /// <summary>
    /// Interface for pushing eye tracking data to an LSL (Lab Streaming Layer) stream.
    /// Required in order not to need to make EDIA LSL a dependency for EDIA Eye.
    /// </summary>
    public interface ILslEyeOutlet : ILslTimeAccessible {

        /// <summary>
        /// Get or set the identifier for the eye (LEFT, RIGHT, CENTER).
        /// </summary>
        public Constants.EyeId EyeId { get; set; }

        ///<summary>
        /// Builds and pushes a sample with eye tracking data.
        /// </summary>
        /// <param name="eyePositionLocal">The local eye position.</param>
        /// <param name="eyeRotationLocalEuler">The local eye rotation in Euler Angles.</param>
        /// <param name="pupilDiameter">The diameter of the pupil. Default is 0f.</param>
        /// <param name="confidence">The confidence level of the eye tracking data. Default is 0f.</param>
        /// <param name="timestampEt">The eye tracker timestamp. Default is 0f.</param>
        /// <param name="timestampLsl">The LSL timestamp. Default is 0 and will timestamp the sample on sending.</param>
        public void PushSample(Vector3 eyePositionLocal,
                               Vector3 eyeRotationLocalEuler,
                               float pupilDiameter = 0f,
                               float confidence = 0f,
                               float timestampEt = 0f,
                               double timestampLsl = 0);
    }


    public interface ILslTimeAccessible {
        public abstract double GetLslTime();
    }

}
