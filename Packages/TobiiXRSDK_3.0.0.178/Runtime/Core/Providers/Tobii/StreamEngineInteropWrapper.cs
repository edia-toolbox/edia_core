using System;
using System.Collections.Generic;
using Tobii.StreamEngine;

namespace Tobii.XR
{
    public interface IStreamEngineInterop
    {
        tobii_error_t tobii_api_create(out IntPtr apiContext, tobii_custom_log_t logger);
        tobii_error_t tobii_api_destroy(IntPtr apiContext);
        tobii_error_t tobii_device_create(IntPtr api, string url, Interop.tobii_field_of_use_t field_of_use, out IntPtr device);
        tobii_error_t tobii_device_create_ex(IntPtr api, string url, Interop.tobii_field_of_use_t field_of_use, string[] license_keys, List<tobii_license_validation_result_t> license_results, out IntPtr device);
        tobii_error_t tobii_device_destroy(IntPtr deviceContext);
        tobii_error_t tobii_device_reconnect(IntPtr nativeContext);
        tobii_error_t tobii_enumerate_local_device_urls_internal(IntPtr apiContext, tobii_device_url_receiver_t receiverFunction, IntPtr userData);
        tobii_error_t tobii_get_device_info(IntPtr deviceContext, out tobii_device_info_t info);
    }
    
    internal class StreamEngineInteropWrapper : IStreamEngineInterop
    {
        public tobii_error_t tobii_api_create(out IntPtr apiContext, tobii_custom_log_t logger)
        {
            return Interop.tobii_api_create(out apiContext, logger);
        }

        public tobii_error_t tobii_api_destroy(IntPtr apiContext)
        {
            return Interop.tobii_api_destroy(apiContext);
        }

        public tobii_error_t tobii_device_create(IntPtr api, string url, Interop.tobii_field_of_use_t field_of_use, out IntPtr device)
        {
            return Interop.tobii_device_create(api, url, field_of_use, out device);
        }

        public tobii_error_t tobii_device_create_ex(IntPtr api, string url, Interop.tobii_field_of_use_t field_of_use, string[] license_keys, List<tobii_license_validation_result_t> license_results, out IntPtr device)
        {
            return Interop.tobii_device_create_ex(api, url, field_of_use, license_keys, license_results, out device);
        }

        public tobii_error_t tobii_device_destroy(IntPtr deviceContext)
        {
            return Interop.tobii_device_destroy(deviceContext);
        }

        public tobii_error_t tobii_device_reconnect(IntPtr nativeContext)
        {
            return Interop.tobii_device_reconnect(nativeContext);
        }

        public tobii_error_t tobii_enumerate_local_device_urls_internal(IntPtr apiContext, tobii_device_url_receiver_t receiverFunction, IntPtr userData)
        {
            return Interop.tobii_enumerate_local_device_urls_internal(apiContext, receiverFunction, userData);
        }

        public tobii_error_t tobii_get_device_info(IntPtr deviceContext, out tobii_device_info_t info)
        {
            return Interop.tobii_get_device_info(deviceContext, out info);
        }
    }
}