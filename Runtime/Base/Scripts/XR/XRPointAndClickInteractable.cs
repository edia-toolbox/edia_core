using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Edia.XR {
    [RequireComponent(typeof(XRSimpleInteractable))]
    [AddComponentMenu("EDIA/XR Interactable PointAndClick")]
    [EdiaHeader("Point and Click", "XR Interactable", "Fires 'Selected' event when the trigger is pressed while hovering over the interactable object.")]
    public class XRPointAndClickInteractable : MonoBehaviour {

        [Tooltip("Link up all inputactions.")]
        public List<InputActionReference> TriggerPressedInputActionReferences = new();

        /// <summary> Method to call on selected. </summary>
        public UnityEvent Selected;

        /// <summary> Method to call on hovered started. </summary>
        public UnityEvent HoveredEntered;

        /// <summary> Method to call on hovred ended.</summary>
        public UnityEvent HoveredExited;

        private bool _isHovering = false;

        /// <summary>Listening and invoking events on XR interaction. </summary>
        private bool _isEnabled = false;

        public bool IsEnabled {
            get => _isEnabled;
            set {
                _isEnabled = value;
                SetEnabled();
            }
        }

        private void SetEnabled() {
            switch (_isEnabled) {
                case true:
                    GetComponent<XRSimpleInteractable>().hoverEntered.AddListener(OnHoverEntered);
                    GetComponent<XRSimpleInteractable>().hoverExited.AddListener(OnHoverExited);
                    TriggerPressedInputActionReferences.ForEach(action => action.action.performed += TriggerPressedInputActionOnperformed);
                    break;
                case false:
                    GetComponent<XRSimpleInteractable>().hoverEntered.RemoveAllListeners();
                    TriggerPressedInputActionReferences.ForEach(action => action.action.performed -= TriggerPressedInputActionOnperformed);
                    break;
            }
        }

        private void OnEnable() {
            SetEnabled();
        }

        private void OnDisable() {
            SetEnabled();
        }

        void TriggerPressedInputActionOnperformed(InputAction.CallbackContext obj) {
            if (!_isHovering)
                return;

            Selected?.Invoke();
        }

        private void OnHoverEntered(HoverEnterEventArgs args) {
            _isHovering = true;
            HoveredEntered?.Invoke();
        }

        private void OnHoverExited(HoverExitEventArgs args) {
            _isHovering = false;
            HoveredExited?.Invoke();
        }
    }
}