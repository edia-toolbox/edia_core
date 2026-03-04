using System;
using System.Linq;
using System.Collections.Generic;

namespace Edia.Data {

    /// <summary>
    /// Cascading settings system. Wraps a Dictionary.
    /// Settings requests cascade upwards through parent settings when a key is not found locally.
    /// </summary>
    public class Settings {

        public static Settings empty { get { return new Settings(new Dictionary<string, object>()); } }

        Settings parentSettings;

        public Dictionary<string, object> baseDict { get; private set; }

        public Dictionary<string, object>.KeyCollection Keys { get { return baseDict.Keys; } }

        public Settings(Dictionary<string, object> dict) {
            baseDict = dict ?? new Dictionary<string, object>();
        }

        public Settings() {
            baseDict = new Dictionary<string, object>();
        }

        public void UpdateWithDict(Dictionary<string, object> dict) {
            dict.ToList().ForEach(x => baseDict[x.Key] = x.Value);
        }

        /// <summary>
        /// Sets the parent settings object, which is accessed when a setting is not found in this dictionary.
        /// </summary>
        public void SetParent(Settings parent) {
            parentSettings = parent;
        }

        // --- Scalar getters ---
        public bool GetBool(string key) { return Convert.ToBoolean(Get(key)); }
        public int GetInt(string key) { return Convert.ToInt32(Get(key)); }
        public float GetFloat(string key) { return Convert.ToSingle(Get(key)); }
        public long GetLong(string key) { return Convert.ToInt64(Get(key)); }
        public double GetDouble(string key) { return Convert.ToDouble(Get(key)); }
        public string GetString(string key) { return Convert.ToString(Get(key)); }
        public Dictionary<string, object> GetDict(string key) { return (Dictionary<string, object>)Get(key); }
        public object GetObject(string key) { return Get(key); }

        // --- List getters ---
        public List<bool> GetBoolList(string key) {
            try { return GetObjectList(key).Select(v => Convert.ToBoolean(v)).ToList(); }
            catch (InvalidCastException) { return (List<bool>)Get(key); }
        }

        public List<int> GetIntList(string key) {
            try { return GetObjectList(key).Select(v => Convert.ToInt32(v)).ToList(); }
            catch (InvalidCastException) { return (List<int>)Get(key); }
        }

        public List<float> GetFloatList(string key) {
            try { return GetObjectList(key).Select(v => Convert.ToSingle(v)).ToList(); }
            catch (InvalidCastException) { return (List<float>)Get(key); }
        }

        public List<long> GetLongList(string key) {
            try { return GetObjectList(key).Select(v => Convert.ToInt64(v)).ToList(); }
            catch (InvalidCastException) { return (List<long>)Get(key); }
        }

        public List<double> GetDoubleList(string key) {
            try { return GetObjectList(key).Select(v => Convert.ToDouble(v)).ToList(); }
            catch (InvalidCastException) { return (List<double>)Get(key); }
        }

        public List<string> GetStringList(string key) {
            try { return GetObjectList(key).Select(v => Convert.ToString(v)).ToList(); }
            catch (InvalidCastException) { return (List<string>)Get(key); }
        }

        public List<Dictionary<string, object>> GetDictList(string key) {
            try { return GetObjectList(key).Select(v => (Dictionary<string, object>)v).ToList(); }
            catch (InvalidCastException) { return (List<Dictionary<string, object>>)Get(key); }
        }

        public List<object> GetObjectList(string key) {
            return (List<object>)Get(key);
        }

        // --- Scalar getters with default ---
        public bool GetBool(string key, bool valueIfNotFound) { return ContainsKey(key) ? GetBool(key) : valueIfNotFound; }
        public int GetInt(string key, int valueIfNotFound) { return ContainsKey(key) ? GetInt(key) : valueIfNotFound; }
        public float GetFloat(string key, float valueIfNotFound) { return ContainsKey(key) ? GetFloat(key) : valueIfNotFound; }
        public long GetLong(string key, long valueIfNotFound) { return ContainsKey(key) ? GetLong(key) : valueIfNotFound; }
        public double GetDouble(string key, double valueIfNotFound) { return ContainsKey(key) ? GetDouble(key) : valueIfNotFound; }
        public string GetString(string key, string valueIfNotFound) { return ContainsKey(key) ? GetString(key) : valueIfNotFound; }
        public Dictionary<string, object> GetDict(string key, Dictionary<string, object> valueIfNotFound) { return ContainsKey(key) ? GetDict(key) : valueIfNotFound; }
        public object GetObject(string key, object valueIfNotFound) { return ContainsKey(key) ? GetObject(key) : valueIfNotFound; }

        // --- List getters with default ---
        public List<bool> GetBoolList(string key, List<bool> valueIfNotFound) { return ContainsKey(key) ? GetBoolList(key) : valueIfNotFound; }
        public List<int> GetIntList(string key, List<int> valueIfNotFound) { return ContainsKey(key) ? GetIntList(key) : valueIfNotFound; }
        public List<float> GetFloatList(string key, List<float> valueIfNotFound) { return ContainsKey(key) ? GetFloatList(key) : valueIfNotFound; }
        public List<long> GetLongList(string key, List<long> valueIfNotFound) { return ContainsKey(key) ? GetLongList(key) : valueIfNotFound; }
        public List<double> GetDoubleList(string key, List<double> valueIfNotFound) { return ContainsKey(key) ? GetDoubleList(key) : valueIfNotFound; }
        public List<string> GetStringList(string key, List<string> valueIfNotFound) { return ContainsKey(key) ? GetStringList(key) : valueIfNotFound; }
        public List<Dictionary<string, object>> GetDictList(string key, List<Dictionary<string, object>> valueIfNotFound) { return ContainsKey(key) ? GetDictList(key) : valueIfNotFound; }
        public List<object> GetObjectList(string key, List<object> valueIfNotFound) { return ContainsKey(key) ? GetObjectList(key) : valueIfNotFound; }

        public bool ContainsKey(string key) {
            if (baseDict.ContainsKey(key))
                return true;
            if (parentSettings != null)
                return parentSettings.ContainsKey(key);
            return false;
        }

        public void SetValue(string key, object value) { Set(key, value); }

        protected object Get(string key) {
            try {
                return baseDict[key];
            }
            catch (KeyNotFoundException) {
                if (parentSettings != null)
                    return parentSettings.Get(key);
                throw new KeyNotFoundException(
                    string.Format("The key \"{0}\" was not found in the settings hierarchy.", key)
                );
            }
        }

        protected void Set(string key, object value) {
            baseDict[key] = value;
        }
    }
}
