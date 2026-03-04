using System.Collections.Generic;

namespace Edia.Data {

    /// <summary>
    /// Thread-safe dictionary used to store trial results.
    /// </summary>
    public class ResultsDictionary {

        private Dictionary<string, object> baseDict;
        private bool allowAdHocAdding;
        private readonly object lockObject = new object();

        public ResultsDictionary(IEnumerable<string> initialKeys, bool allowAdHocAdding) {
            baseDict = new Dictionary<string, object>();
            this.allowAdHocAdding = allowAdHocAdding;
            foreach (var key in initialKeys) {
                lock (lockObject) {
                    baseDict.Add(key, string.Empty);
                }
            }
        }

        public object this[string key] {
            get { return baseDict[key]; }
            set {
                lock (lockObject) {
                    if (allowAdHocAdding || baseDict.ContainsKey(key)) {
                        baseDict[key] = value;
                    }
                    else {
                        throw new KeyNotFoundException(string.Format("Custom header \"{0}\" does not exist!", key));
                    }
                }
            }
        }

        public Dictionary<string, object>.KeyCollection Keys {
            get { return baseDict.Keys; }
        }

        public bool ContainsKey(string key) {
            return baseDict.ContainsKey(key);
        }
    }
}
