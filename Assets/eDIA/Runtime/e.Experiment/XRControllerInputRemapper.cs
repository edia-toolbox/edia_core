using System.Collections;
using System.Collections.Generic;
using eDIA;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;


namespace eDIA {

	/// <summary>In order to be flexible for each taskblock, the remapping of a controller key to a method is a separate script</summary>
	[System.Serializable]
	public class XRControllerInputRemapper : MonoBehaviour {

		// TODO Allow multiple input actions to one ID
		// TODO Input remapping should take systems 'allowed interaction' into considiration

		[System.Serializable]
		public class ControllerInputRemap {
			public string id;
			public InputActionReference inputActionSubmit;
			public UnityEvent<InputAction.CallbackContext> methodToCall;
		} 

		public List<ControllerInputRemap> Redirectors = new List<ControllerInputRemap>();

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
			
			if (onOff) Redirectors[index].inputActionSubmit.action.performed += Redirectors[index].methodToCall.Invoke;
			else Redirectors[index].inputActionSubmit.action.performed -= Redirectors[index].methodToCall.Invoke;
		}

	}
}