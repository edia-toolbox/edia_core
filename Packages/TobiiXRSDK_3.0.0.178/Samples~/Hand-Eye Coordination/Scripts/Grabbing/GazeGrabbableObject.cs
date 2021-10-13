// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.G2OM;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tobii.XR.Examples
{
    /// <summary>
    /// Monobehaviour which can be put on objects to allow the user to look at it and grab it with <see cref="GazeGrab"/>.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class GazeGrabbableObject : MonoBehaviour, IGazeFocusable
    {
        [SerializeField, Tooltip("Time in seconds for the object to fly to controller.")]
        private float flyToControllerTimeSeconds = 0.2f;

        public float FlyToControllerTimeSeconds => flyToControllerTimeSeconds;

        [SerializeField, Tooltip("The animation curve of how the object flies to the controller.")]
        private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public AnimationCurve AnimationCurve => animationCurve;

        public float GazeStickinessSeconds => 0.1f;

        /// <summary>
        /// Called by TobiiXR when the object receives focus.
        /// </summary>
        /// <param name="hasFocus"></param>
        public void GazeFocusChanged(bool hasFocus)
        {
        }

        /// <summary>
        /// Called by <see cref="GazeGrab"/> when the object is flying towards the hand.
        /// </summary>
        public void ObjectGrabbing()
        {
        }

        /// <summary>
        /// Called by <see cref="GazeGrab"/> when the object has been grabbed to the hand.
        /// </summary>
        public void ObjectGrabbed()
        {
        }


        /// <summary>
        /// Called by <see cref="GazeGrab"/> when the object has been ungrabbed.
        /// </summary>
        public void ObjectUngrabbed()
        {
        }
    }
}
