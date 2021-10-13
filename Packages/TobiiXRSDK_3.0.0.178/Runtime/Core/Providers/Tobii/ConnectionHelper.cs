using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Tobii.StreamEngine;

namespace Tobii.XR
{
    public static class ConnectionHelper
    {
        private static readonly tobii_device_url_receiver_t _deviceUrlReceiver = DeviceUrlReceiver; // Needed to prevent GC from removing callback
        private static readonly Stopwatch _stopwatch = new Stopwatch();
        
        public static bool TryConnect(IStreamEngineInterop interop, StreamEngineTracker_Description description, out StreamEngineContext context, tobii_custom_log_t customLog = null)
        {
            _stopwatch.Reset();
            _stopwatch.Start();

            context = null;
            IntPtr apiContext;
            if (CreateApiContext(interop, out apiContext, customLog) == false) return false;

            try
            {
                List<string> connectedDevices;
                if (GetAvailableTrackers(interop, apiContext, out connectedDevices) == false)
                {
                    DestroyApiContext(interop, apiContext);
                    return false;
                }

                IntPtr deviceContext;
                string hmdEyeTrackerUrl;
                if (GetFirstSupportedTracker(interop, apiContext, connectedDevices, description, out deviceContext, out hmdEyeTrackerUrl) == false)
                {
                    DestroyApiContext(interop, apiContext);
                    return false;
                }

                context = new StreamEngineContext(apiContext, deviceContext, hmdEyeTrackerUrl);
                _stopwatch.Stop();
                UnityEngine.Debug.Log(string.Format("Connected to SE tracker: {0} and it took {1}ms",
                    context.Url, _stopwatch.ElapsedMilliseconds));
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error connecting to eye tracker: " + e.ToString());
                return false;
            }
        }

        public static void Disconnect(IStreamEngineInterop interop, StreamEngineContext context)
        {
            if (context == null) return;

            DestroyDeviceContext(interop, context.Device);
            DestroyApiContext(interop, context.Api);
        }

        private static bool CreateDeviceContext(IStreamEngineInterop interop, string url, Interop.tobii_field_of_use_t fieldOfUse, IntPtr apiContext, string[] licenseKeys, out IntPtr deviceContext)
        {
            if (licenseKeys == null || licenseKeys.Length == 0)
            {
                return interop.tobii_device_create(apiContext, url, fieldOfUse, out deviceContext) == tobii_error_t.TOBII_ERROR_NO_ERROR;
            }

            var licenseResults = new List<tobii_license_validation_result_t>();
            var result = interop.tobii_device_create_ex(apiContext, url, fieldOfUse, licenseKeys, licenseResults, out deviceContext);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                UnityEngine.Debug.LogError(string.Format("Failed to create device context for {0}. {1}", url, result));
                return false;
            }

            for (int i = 0; i < licenseKeys.Length; i++)
            {
                var licenseResult = licenseResults[i];
                if (licenseResult == tobii_license_validation_result_t.TOBII_LICENSE_VALIDATION_RESULT_OK) continue;

                UnityEngine.Debug.LogError("License " + i + " failed. Return code " + licenseResult);
            }

            return true;
        }

        private static void DestroyDeviceContext(IStreamEngineInterop interop, IntPtr deviceContext)
        {
            if (deviceContext == IntPtr.Zero) return;

            var result = interop.tobii_device_destroy(deviceContext);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                UnityEngine.Debug.LogError(string.Format("Failed to destroy device context. Error {0}", result));
            }
        }

        private static bool CreateApiContext(IStreamEngineInterop interop, out IntPtr apiContext, tobii_custom_log_t customLog = null)
        {
            var result = interop.tobii_api_create(out apiContext, customLog);
            if (result == tobii_error_t.TOBII_ERROR_NO_ERROR) return true;

            UnityEngine.Debug.LogError("Failed to create api context. " + result);
            apiContext = IntPtr.Zero;
            return false;
        }

        private static void DestroyApiContext(IStreamEngineInterop interop, IntPtr apiContext)
        {
            if (apiContext == IntPtr.Zero) return;

            var result = interop.tobii_api_destroy(apiContext);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                UnityEngine.Debug.LogError(string.Format("Failed to destroy api context. Error {0}", result));
            }
        }

        [AOT.MonoPInvokeCallback(typeof(tobii_device_url_receiver_t))]
        private static void DeviceUrlReceiver(string url, IntPtr user_data)
        {
            GCHandle gch = GCHandle.FromIntPtr(user_data);
            var urls = (List<string>)gch.Target;
            urls.Add(url);
        }

        private static bool GetAvailableTrackers(IStreamEngineInterop interop, IntPtr apiContext, out List<string> connectedDevices)
        {
            connectedDevices = new List<string>();
            GCHandle gch = GCHandle.Alloc(connectedDevices);
            var result = interop.tobii_enumerate_local_device_urls_internal(apiContext, _deviceUrlReceiver, GCHandle.ToIntPtr(gch));
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                UnityEngine.Debug.LogError("Failed to enumerate connected devices. " + result);
                return false;
            }

            if (connectedDevices.Count >= 1) return true;

            UnityEngine.Debug.LogWarning("No connected eye trackers found.");
            return false;
        }

        private static bool GetFirstSupportedTracker(IStreamEngineInterop interop, IntPtr apiContext, IList<string> connectedDevices, StreamEngineTracker_Description description, out IntPtr deviceContext, out string deviceUrl)
        {
            deviceContext = IntPtr.Zero;
            deviceUrl = "";

            for (var i = 0; i < connectedDevices.Count; i++)
            {
                var connectedDeviceUrl = connectedDevices[i];
                if (CreateDeviceContext(interop, connectedDeviceUrl, Interop.tobii_field_of_use_t.TOBII_FIELD_OF_USE_STORE_OR_TRANSFER_FALSE, apiContext, description.License, out deviceContext) == false) continue;

                tobii_device_info_t info;
                var result = interop.tobii_get_device_info(deviceContext, out info);
                if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
                {
                    DestroyDeviceContext(interop, deviceContext);
                    UnityEngine.Debug.LogWarning("Failed to get device info. " + result);
                    continue;
                }

                var integrationType = info.integration_type.ToLowerInvariant();
                if (integrationType != description.SupportedIntegrationType)
                {
                    DestroyDeviceContext(interop, deviceContext);
                    continue;
                }

                deviceUrl = connectedDeviceUrl;
                return true;
            }

            UnityEngine.Debug.LogWarning(string.Format("Failed to find Tobii eye trackers of integration type {0}", description.SupportedIntegrationType));
            return false;
        }

        public static bool TryReconnect(IStreamEngineInterop interop, IntPtr deviceContext)
        {
            var result = interop.tobii_device_reconnect(deviceContext);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR) return false;

            UnityEngine.Debug.Log("Reconnected.");
            return true;
        }
    }

    
}
