using System;
using UnityEngine;

namespace Tobii.XR
{
    public enum FeatureGroup
    {
        Consumer,
        Config,
        Professional,
    }
    
    public class LicenseParser
    {
        public string Licensee { get; private set; }
        public DateTime? ValidTo { get; private set; }
        public bool EyeImages { get; private set; }
        
        public bool LicenseIsParsed { get; private set; }
        
        public FeatureGroup FeatureGroup { get; private set; }

        public LicenseParser(string license)
        {
            var t = license.Substring(0, license.LastIndexOf("}", StringComparison.Ordinal) + 1);

            try
            {
                var json = JsonUtility.FromJson<LicenseJson>(t);
                Licensee = json.licenseKey.header.licensee;

                if (json.licenseKey.conditions.dateValid != null)
                {
                    var validTo = json.licenseKey.conditions.dateValid.to;
                    if (!string.IsNullOrEmpty(validTo))
                    {
                        ValidTo = DateTime.Parse(validTo);
                    }
                }

                switch (json.licenseKey.enables.featureGroup)
                {
                    case "consumer":
                        FeatureGroup = FeatureGroup.Consumer;
                        break;
                    case "config":
                        FeatureGroup = FeatureGroup.Config;
                        break;
                    case "professional":
                        FeatureGroup = FeatureGroup.Professional;
                        break;
                }

                // Check features
                if (json.licenseKey.enables.features != null)
                {
                    var hasLimitedImageStream = false;
                    var hasDiagnosticsImageStream = false;
                    foreach (var feature in json.licenseKey.enables.features)
                    {
                        switch (feature)
                        {
                            case "wearableLimitedImage":
                                hasLimitedImageStream = true;
                                break;
                            case "wearableDiagnosticsImage":
                                hasDiagnosticsImageStream = true;
                                break;
                        }
                    }

                    EyeImages = hasLimitedImageStream && hasDiagnosticsImageStream;
                }
                LicenseIsParsed = true;
            }
            catch (Exception exception)
            {
                Debug.Log("Unable to parse license: " + exception);
                LicenseIsParsed = false;
            }
        }
    

        #region License JSON

#pragma warning disable 0649 //  Field is never assigned to, and will always have its default value null

        [Serializable]
        private class LicenseJson
        {
            public LicenseKey licenseKey;
        }

        [Serializable]
        private class Header
        {
            public string id;
            public string licensee;
            public string version;
            public string created;
        }

        [Serializable]
        private class DateValid
        {
            public string from;
            public string to;
        }

        [Serializable]
        private class Process
        {
            public string[] names;
        }

        [Serializable]
        private class Conditions
        {
            public DateValid dateValid;
            public Process process;
        }

        [Serializable]
        private class Enables
        {
            public string featureGroup;
            public string[] features;
        }

        [Serializable]
        private class LicenseKey
        {
            public Header header;
            public Conditions conditions;
            public Enables enables;
        }

#pragma warning restore

        #endregion
    }

}