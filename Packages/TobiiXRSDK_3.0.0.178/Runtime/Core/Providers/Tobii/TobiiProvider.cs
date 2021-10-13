// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Tobii.StreamEngine;
using Unity.Jobs;
using UnityEngine;

namespace Tobii.XR
{
    /// <summary>
    /// Uses Tobii's Stream Engine library to provide eye tracking data to TobiiXR
    /// </summary>
    [ProviderDisplayName("Tobii")]
    public class TobiiProvider : IEyeTrackingProvider
    {
        private const int AdvancedDataQueueSize = 30;
        private readonly object _lockEyeTrackingDataLocal = new object();
        private readonly TobiiXR_EyeTrackingData _eyeTrackingDataLocal = new TobiiXR_EyeTrackingData();
        private readonly TobiiXR_EyeTrackingData _eyeTrackingDataLocalInternal = new TobiiXR_EyeTrackingData();

        private readonly object _lockAdvancedData = new object();
        private readonly TobiiXR_AdvancedEyeTrackingData _advancedEyeTrackingData =
            new TobiiXR_AdvancedEyeTrackingData();

        private readonly Queue<TobiiXR_AdvancedEyeTrackingData> _advancedInternalQueue =
            new Queue<TobiiXR_AdvancedEyeTrackingData>();

        private readonly Queue<TobiiXR_AdvancedEyeTrackingData> _advancedPublicQueue =
            new Queue<TobiiXR_AdvancedEyeTrackingData>();

        private Vector3 _foveatedGazeDirectionLocal;
        private StreamEngineTracker _streamEngineTracker;
        private HmdToWorldTransformer _hmdToWorldTransformer;
        private Matrix4x4 _localToWorldMatrix;
        private Vector3 _headToCenterEyeTranslation;

        private PositionGuideData _positionGuideData;
        private readonly object _lockPositionGuideData = new object();
        
        public Matrix4x4 LocalToWorldMatrix
        {
            get { return _localToWorldMatrix; }
        }

        public void GetEyeTrackingDataLocal(TobiiXR_EyeTrackingData data)
        {
            EyeTrackingDataHelper.Copy(_eyeTrackingDataLocal, data);
        }

        public TobiiXR_AdvancedEyeTrackingData AdvancedEyeTrackingData
        {
            get { return _advancedEyeTrackingData; }
        }

        public Vector3 FoveatedGazeDirectionLocal
        {
            get { return _foveatedGazeDirectionLocal; }
        }

        public bool HasValidOcumenLicense
        {
            get { return _streamEngineTracker.LicenseLevel >= tobii_feature_group_t.TOBII_FEATURE_GROUP_PROFESSIONAL; }
        }

        public Queue<TobiiXR_AdvancedEyeTrackingData> AdvancedData
        {
            get { return _advancedPublicQueue; }
        }

        public PositionGuideData PositionGuideData
        {
            get { return _positionGuideData; }
        }

        public StreamEngineContext InternalHandle
        {
            get { return _streamEngineTracker.Context; }
        }

        public TobiiXR_EyeTrackerMetadata GetMetadata()
        {
            tobii_device_info_t info;
            Interop.tobii_get_device_info(_streamEngineTracker.Context.Device, out info);
            float outputFrequency;
            Interop.tobii_get_output_frequency(_streamEngineTracker.Context.Device, out outputFrequency);
            var result = new TobiiXR_EyeTrackerMetadata
            {
                SerialNumber = info.serial_number,
                Model = info.model,
                RuntimeVersion = info.runtime_build_version,
                OutputFrequency = outputFrequency > 1 ? outputFrequency.ToString(CultureInfo.InvariantCulture) : "Unknown",
            };
            return result;
        }

        public bool Initialize()
        {
            return InitializeWithLicense(null, false);
        }

        public bool InitializeWithLicense(string licenseKey, bool enableAdvanced)
        {
            var createInfo = new StreamEngineTracker_Description();
            if (!string.IsNullOrEmpty(licenseKey))
            {
                createInfo.License = new[] {licenseKey};
            }
            
            try
            {
                _hmdToWorldTransformer = new HmdToWorldTransformer(estimatedEyeTrackerLatency_s: 0.012f);
                _streamEngineTracker = new StreamEngineTracker(createInfo);

                // Subscribe to relevant streams
                var startInfo = new StreamEngineTrackerStartInfo();
                if (enableAdvanced) startInfo.WearableAdvancedDataCallback = OnAdvancedWearableData;
                else startInfo.WearableDataCallback = OnWearableData;
                _streamEngineTracker.Start(startInfo);
                
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return false;
            }
        }

        public void Tick()
        {
            _headToCenterEyeTranslation = CoordinatesHelper.GetHeadToCenterEyeTranslation();
            _hmdToWorldTransformer.Tick();
            _localToWorldMatrix = _hmdToWorldTransformer.GetLocalToWorldMatrix();

            // Copy consumer data
            lock (_lockEyeTrackingDataLocal)
            {
                EyeTrackingDataHelper.Copy(_eyeTrackingDataLocalInternal, _eyeTrackingDataLocal);
            }
            _eyeTrackingDataLocal.Timestamp = Time.unscaledTime;

            // Shuffle data from internal queue to public queue
            lock (_lockAdvancedData)
            {
                while (_advancedInternalQueue.Count > 1)
                {
                    _advancedPublicQueue.Enqueue(_advancedInternalQueue.Dequeue());
                }

                // Copy newest data to _advancedEyeTrackingData
                if (_advancedInternalQueue.Count == 1)
                {
                    var data = _advancedInternalQueue.Dequeue();
                    EyeTrackingDataHelper.Copy(data, _advancedEyeTrackingData);
                    _advancedPublicQueue.Enqueue(data);
                }
                
                // Limit size of public queue
                while (_advancedPublicQueue.Count > AdvancedDataQueueSize) _advancedPublicQueue.Dequeue();
            }
        }

        public void Destroy()
        {
            if (_streamEngineTracker != null)
            {
                _streamEngineTracker.Destroy();
                _streamEngineTracker = null;
            }
        }

        private void OnWearableData(ref tobii_wearable_consumer_data_t data)
        {
            lock (_lockEyeTrackingDataLocal)
            {
                StreamEngineDataMapper.FromConsumerData(_eyeTrackingDataLocalInternal, ref data,
                    _streamEngineTracker.ConvergenceDistanceSupported, _headToCenterEyeTranslation);
            }
            
            lock (_lockPositionGuideData)
            {
                StreamEngineDataMapper.FillPositionGuideData(ref _positionGuideData, ref data);
            }
        }

        private void OnAdvancedWearableData(ref tobii_wearable_advanced_data_t data)
        {
            lock (_lockAdvancedData)
            {
                var advancedData = _advancedInternalQueue.Count >= AdvancedDataQueueSize
                    ? _advancedInternalQueue.Dequeue()
                    : new TobiiXR_AdvancedEyeTrackingData();

                StreamEngineDataMapper.MapAdvancedData(advancedData, ref data,
                    _streamEngineTracker.ConvergenceDistanceSupported, _headToCenterEyeTranslation);
                _advancedInternalQueue.Enqueue(advancedData);
            }

            // Also fill in consumer api
            lock (_lockEyeTrackingDataLocal)
            {
                StreamEngineDataMapper.FromAdvancedData(_eyeTrackingDataLocalInternal, ref data,
                    _streamEngineTracker.ConvergenceDistanceSupported, _headToCenterEyeTranslation);
            }
            
            lock (_lockPositionGuideData)
            {
                StreamEngineDataMapper.FillPositionGuideData(ref _positionGuideData, ref data);
            }
        }

        private void OnFoveatedData(ref tobii_wearable_foveated_gaze_t data)
        {
            _foveatedGazeDirectionLocal.x =
                data.gaze_direction_combined_normalized_xyz.x * -1; // Tobii to Unity CS conversion
            _foveatedGazeDirectionLocal.y = data.gaze_direction_combined_normalized_xyz.y;
            _foveatedGazeDirectionLocal.z = data.gaze_direction_combined_normalized_xyz.z;
        }

        #region Timesync

        public JobHandle StartTimesyncJob()
        {
            return _streamEngineTracker.StartTimesyncJob();
        }

        public TobiiXR_AdvancedTimesyncData? FinishTimesyncJob()
        {
            return _streamEngineTracker.FinishTimesyncJob();
        }

        public long GetSystemTimestamp()
        {
            long timestamp;
            Interop.tobii_system_clock(_streamEngineTracker.Context.Api, out timestamp);
            return timestamp;
        }

        #endregion
    }

    public struct PositionGuideData
    {
        public Vector2 Left;
        public bool LeftIsValid;
        public Vector2 Right;
        public bool RightIsValid;
    }

}