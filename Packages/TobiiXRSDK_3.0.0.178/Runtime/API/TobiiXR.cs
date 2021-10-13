// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.XR
{
    using System.Collections.Generic;
    using System.Text;
    using Tobii.G2OM;
    using Tobii.XR.Internal;
    using UnityEngine;

    /// <summary>
    /// Static access point for Tobii XR eye tracker data.
    /// </summary>
    public static class TobiiXR
    {
        private static readonly TobiiXRInternal _internal = new TobiiXRInternal();
        private static GameObject _updaterGameObject;
        private static readonly TobiiXR_EyeTrackingData _eyeTrackingDataLocal = new TobiiXR_EyeTrackingData();
        private static readonly TobiiXR_EyeTrackingData _eyeTrackingDataWorld = new TobiiXR_EyeTrackingData();
        private static TobiiXRAdvanced _advanced;

        /// <summary>
        /// Gets eye tracking data in the selected tracking space. Unless the underlying eye tracking
        /// provider does prediction, this data is not predicted.
        /// Subsequent calls within the same frame will return the same value.
        /// </summary>
        /// <param name="trackingSpace">The tracking space to report eye tracking data in.</param>
        /// <returns>The last (newest) <see cref="TobiiXR_EyeTrackingData"/>.</returns>
        public static TobiiXR_EyeTrackingData GetEyeTrackingData(TobiiXR_TrackingSpace trackingSpace)
        {
            switch (trackingSpace)
            {
                case TobiiXR_TrackingSpace.Local:
                    return _eyeTrackingDataLocal;
                case TobiiXR_TrackingSpace.World:
                    return _eyeTrackingDataWorld;
            }

            throw new System.Exception("Unknown tracking space: " + trackingSpace);
        }

        /// <summary>
        /// Gets all possible <see cref="FocusedCandidate"/> with gaze focus. Only game 
        /// objects with a <see cref="IGazeFocusable"/> component can be focused 
        /// using gaze.
        /// </summary>
        /// <returns>A list of <see cref="FocusedCandidate"/> in descending order of probability.</returns>
        public static List<FocusedCandidate> FocusedObjects
        {
            get
            {
                if (Internal.G2OM == null) return new List<FocusedCandidate>();

                return Internal.G2OM.GazeFocusedObjects;
            }
        }

        private static bool IsRunning
        {
            get { return Internal.Provider != null; }
        }
        
        public static bool Start(TobiiXR_Settings settings = null)
        {
            if (IsRunning) Stop();
            
            if (!TobiiEula.IsEulaAccepted())
            {
                Debug.LogWarning(
                    "You need to accept Tobii Software Development License Agreement to use Tobii XR Unity SDK.");
            }

            // Create default settings if none were provided
            if (settings == null)
            {
                settings = new TobiiXR_Settings();
            }
            Internal.Settings = settings;
            
            // Check if a license was supplied
            string licenseKey = null;
            if (settings.LicenseAsset != null) // Prioritize asset
            {
                Debug.Log("Using license asset from settings");
                licenseKey = Encoding.Unicode.GetString(settings.LicenseAsset.bytes);
            }
            else if (!string.IsNullOrEmpty(settings.OcumenLicense)) // Second priority is license as text
            {
                Debug.Log("Using license string from settings");
                licenseKey = settings.OcumenLicense;
            }
            /*else // Lastly try to find a license in Resources
            {
                var licenseResource = Resources.Load("TobiiOcumenLicense") as TextAsset;
                if (licenseResource != null)
                {
                    Debug.Log("Using TobiiOcumenLicense from Resources");
                    licenseKey = Encoding.Unicode.GetString(licenseResource.bytes);
                }
            }*/
            
            // Setup eye tracking provider
            if (settings.AdvancedEnabled)
            {
                Debug.Log("Advanced eye tracking enabled so TobiiXR will use Tobii provider for eye tracking.");
                if (string.IsNullOrEmpty(licenseKey))
                {
                    throw new System.Exception("An Ocumen license is required to use the advanced API. Read more about Ocumen here: https://vr.tobii.com/sdk/solutions/tobii-ocumen/");
                }

                var provider = new TobiiProvider();
                if (!provider.InitializeWithLicense(licenseKey, true))
                {
                    Debug.LogError("Failed to connect to a supported eye tracker. TobiiXR will NOT be available.");
                    return false;
                }

                if (provider.HasValidOcumenLicense)
                {
                    Debug.Log("Ocumen license valid");
                    _advanced = new TobiiXRAdvanced(provider);
                }
                else
                {
                    Debug.LogError("Ocumen license INVALID. Advanced API will NOT be available.");
                }

                Internal.Provider = provider;
            }
            else if (!string.IsNullOrEmpty(licenseKey))
            {
                Debug.Log("An explicit license was provided so TobiiXR will use Tobii provider for eye tracking.");
                var provider = new TobiiProvider();
                if (!provider.InitializeWithLicense(licenseKey, false))
                {
                    Debug.LogError("Failed to connect to a supported eye tracker. TobiiXR will NOT be available.");
                    return false;
                }
                
                Internal.Provider = provider;
            }
            else
            {
                Internal.Provider = settings.EyeTrackingProvider;

                if (Internal.Provider == null)
                {
                    Internal.Provider = new NoseDirectionProvider();
                    Debug.LogWarning(string.Format("All configured providers failed. Using ({0}) as fallback.",
                        Internal.Provider.GetType().Name));
                }

                Debug.Log(string.Format("Starting TobiiXR with ({0}) as provider for eye tracking.",
                    Internal.Provider));
            }

            // Setup G2OM
            if (settings.G2OM != null)
            {
                Internal.G2OM = settings.G2OM;
            }
            else
            {
                Internal.G2OM = G2OM.Create(new G2OM_Description
                {
                    LayerMask = settings.LayerMask,
                    HowLongToKeepCandidatesInSeconds = settings.HowLongToKeepCandidatesInSeconds
                });
            }

            // Create GameObject with TobiiXR_Lifecycle to give us Unity events
            _updaterGameObject = new GameObject("TobiiXR Updater");
            var updater = _updaterGameObject.AddComponent<TobiiXR_Lifecycle>();
            updater.OnUpdateAction += Tick;
            updater.OnDisableAction += Internal.G2OM.Clear;
            updater.OnApplicationQuitAction += Stop;

            return true;
        }

        public static void Stop()
        {
            if (!IsRunning) return;

            Internal.G2OM.Destroy();
            Internal.Provider.Destroy();

            if (_updaterGameObject != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    Object.Destroy(_updaterGameObject.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(_updaterGameObject.gameObject);
                }
#else
            Object.Destroy(_updaterGameObject.gameObject);
#endif
            }


            _updaterGameObject = null;
            _advanced = null;
            Internal.G2OM = null;
            Internal.Provider = null;
        }

        private static void Tick()
        {
            Internal.Provider.Tick();
            Internal.Provider.GetEyeTrackingDataLocal(_eyeTrackingDataLocal);
            EyeTrackingDataHelper.CopyAndTransformGazeData(_eyeTrackingDataLocal, _eyeTrackingDataWorld,
                Internal.Provider.LocalToWorldMatrix);

            if (Internal.Filter != null && Internal.Filter.enabled)
            {
                var worldForward = Internal.Provider.LocalToWorldMatrix.MultiplyVector(Vector3.forward);
                Internal.Filter.Filter(_eyeTrackingDataLocal, Vector3.forward);
                Internal.Filter.Filter(_eyeTrackingDataWorld, worldForward);
            }

            var g2omData = CreateG2OMData(_eyeTrackingDataWorld);
            Internal.G2OM.Tick(g2omData);
        }

        private static G2OM_DeviceData CreateG2OMData(TobiiXR_EyeTrackingData data)
        {
            var t = Internal.Provider.LocalToWorldMatrix;
            return new G2OM_DeviceData
            {
                timestamp = data.Timestamp,
                gaze_ray_world_space = new G2OM_GazeRay
                {
                    is_valid = data.GazeRay.IsValid.ToByte(),
                    ray = G2OM_UnityExtensionMethods.CreateRay(data.GazeRay.Origin, data.GazeRay.Direction),
                },
                camera_up_direction_world_space = t.MultiplyVector(Vector3.up).AsG2OMVector3(),
                camera_right_direction_world_space = t.MultiplyVector(Vector3.right).AsG2OMVector3()
            };
        }

        /// <summary>
        /// For advanced and internal use only. Do not access this field before TobiiXR.Start has been called.
        /// Do not save a reference to the fields exposed by this class since TobiiXR will recreate them when restarted
        /// </summary>
        public static TobiiXRInternal Internal
        {
            get { return _internal; }
        }

        /// <summary>
        /// For advanced use only. TobiiXR needs to be initialized with a license before accessing this API.
        /// </summary>
        public static TobiiXRAdvanced Advanced
        {
            get
            {
                if (_advanced == null)
                {
                    Debug.LogError("An attempt was made to access TobiiXR Advanced without having initialized it");
                }

                return _advanced;
            }
        }

        public class TobiiXRInternal
        {
            public TobiiXR_Settings Settings { get; internal set; }

            public IEyeTrackingProvider Provider { get; set; }

            public G2OM G2OM { get; internal set; }

            /// <summary>
            /// Defaults to no filter. If set, both EyeTrackingData and FocusedObjects will apply this filter to gaze data before using it
            /// </summary>
            public EyeTrackingFilterBase Filter
            {
                get { return Settings == null ? null : Settings.EyeTrackingFilter; }
            }
        }
    }
}
