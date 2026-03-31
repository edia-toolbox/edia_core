using TMPro;
using UnityEngine;

namespace Edia.Controller {
    public class KeyValueInfo : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI keyField;
        [SerializeField] private TextMeshProUGUI valueField;

        public void Set(string key, string value) {
            keyField.text   = key;
            valueField.text = value;
        }

        public void SetValue(string value) {
            valueField.text = value;
        }
    }
}