// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.XR
{
    using UnityEngine;

    public interface IEyeTrackingProvider
    {
        void GetEyeTrackingDataLocal(TobiiXR_EyeTrackingData data);

        Matrix4x4 LocalToWorldMatrix { get; }

        bool Initialize();

        void Tick();

        void Destroy();
    }
}
