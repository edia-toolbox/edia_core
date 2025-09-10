using UnityEngine;
using UnityEngine.UI;

namespace Edia.UI {

    /// <summary>
    /// A component that toggles the active state of GameObjects in response to
    /// the value change of a UI toggle.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class ToggleImageToggler : MonoBehaviour {
        [Tooltip("Graphic object representing the toggle on position.")]
        [SerializeField]
        private Image ToggleOnObject;

        [Tooltip("Graphic object representing the toggle off position.")]
        [SerializeField]
        private Image ToggleOffObject;

        [SerializeField] private bool   UseSelectedColorForOn = true;
        private                  Toggle toggle;
        
        [SerializeField] private bool   ToggleTargetGraphic = true;
        
        void Awake() {
            toggle = GetComponent<Toggle>();
            if (ToggleOnObject != null) {
                ToggleOnObject.color = UseSelectedColorForOn ? toggle.colors.selectedColor : ToggleOnObject.color;
            }
        }

        void OnEnable() {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        void OnDisable() {
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        void OnToggleValueChanged(bool isOn) {
            ToggleOnObject.gameObject.SetActive(isOn);
            ToggleOffObject.gameObject.SetActive(!isOn);

            if (!ToggleTargetGraphic) return;
            toggle.targetGraphic = isOn ? ToggleOnObject : ToggleOffObject;
        }
    }
}