// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;
using UnityEngine.Serialization;

namespace Tobii.XR.Examples.DevTools
{
    public class SimpleMovement : MonoBehaviour
    {
        public Vector3 lengthAndDirection = new Vector3(5, 0, 0);

        private Vector3 _startPosition;

        private void Start()
        {
            _startPosition = transform.position;
        }

        private void Update()
        {
            var offset = Mathf.Sin(Time.time);
            transform.position = _startPosition + lengthAndDirection * offset;
        }
    }
}