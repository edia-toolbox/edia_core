// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;
using UnityEngine.Serialization;

namespace Tobii.XR.Examples.DevTools
{
    public class SimpleScaling : MonoBehaviour
    {
        public Vector3 maximumScale = new Vector3(1, 1, 1);
        public Vector3 minimumScale = new Vector3(.25f, .25f, .25f);

        private void Update()
        {
            var offset = Mathf.Abs(Mathf.Sin(Time.time));
            transform.localScale = Vector3.Lerp(minimumScale, maximumScale, offset);
        }
    }
}