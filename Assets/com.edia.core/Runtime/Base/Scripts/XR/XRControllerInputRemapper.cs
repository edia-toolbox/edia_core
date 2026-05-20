using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace Edia.XR {

	/// <summary>In order to be flexible for each Xblock, the remapping of a controller key to a method is a separate script</summary>
	[System.Serializable]
	[AddComponentMenu("EDIA/XR Remap Controller Input")]
	public class XRControllerInputRemapper : MonoBehaviour {

		// TODO Allow multiple input actions to one ID
		// TODO Input remapping should take systems 'allowed interaction' into considiration

		[System.Serializable]
		public class ControllerInputRemap {
			public string id;
			public bool isEnabled = false;
			public InputActionReference inputActionSubmit;
			public UnityEvent<InputAction.CallbackContext> methodToCall;

			internal bool isSubscribed = false;
		}

		public List<ControllerInputRemap> Redirectors = new List<ControllerInputRemap>();

		private void OnEnable() {
			foreach (ControllerInputRemap r in Redirectors) {
				if (r.isEnabled) {
					Subscribe(r);
				}
			}
		}

		private void OnDisable() {
			foreach (ControllerInputRemap r in Redirectors) {
				Unsubscribe(r);
			}
		}

		public List<string> GetControllerRemappings () {
			
			List<string> result = new List<string>();
			foreach (ControllerInputRemap r in Redirectors) {
				result.Add(r.id);
			}
			return result;
		}

		/// <summary>Enables predefined controller inputaction to custom unity event</summary>
		/// <param name="id">indentifier</param>
		/// <param name="onOff">active</param>
		public void EnableRemapping (string id, bool onOff) {
			int index = Redirectors.FindIndex(x => x.id == id);
			if (index < 0) return;

			Redirectors[index].isEnabled = onOff;

			if (!isActiveAndEnabled) return;

			if (onOff) Subscribe(Redirectors[index]);
			else Unsubscribe(Redirectors[index]);
		}

		private void Subscribe(ControllerInputRemap r) {
			if (r.isSubscribed || r.inputActionSubmit == null || r.inputActionSubmit.action == null) return;
			
			r.inputActionSubmit.action.Enable();
			r.inputActionSubmit.action.performed += r.methodToCall.Invoke;
			r.isSubscribed = true;
		}

		private void Unsubscribe(ControllerInputRemap r) {
			if (!r.isSubscribed || r.inputActionSubmit == null || r.inputActionSubmit.action == null) return;

			r.inputActionSubmit.action.performed -= r.methodToCall.Invoke;
			// Note: We don't disable the action here as other components might be using the same InputActionReference
			r.isSubscribed = false;
		}
	}
}