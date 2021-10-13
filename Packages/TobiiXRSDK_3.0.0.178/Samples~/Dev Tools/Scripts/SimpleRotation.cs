// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;
using UnityEngine.Serialization;

namespace Tobii.XR.Examples.DevTools
{
    public class SimpleRotation : MonoBehaviour
    {
        public Vector3 lengthAndDirection = new Vector3(5, 5, 0);

        private void Update()
        {
            transform.Rotate(lengthAndDirection * Time.deltaTime);
        }
    }
}