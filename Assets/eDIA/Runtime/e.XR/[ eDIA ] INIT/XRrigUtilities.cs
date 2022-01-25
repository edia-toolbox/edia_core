using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace eDIA {

	public static class XRrigUtilities {

		// Holders for the main objects
		private static Transform xCam = null;
		private static Transform xCtrlR = null;
		private static Transform xCtrlL = null;

#region HMD and Controller objects

		private static int maxLoops = 100;

		/// <summary>Gets XRrig references from SystemManager or FindWithTag  </summary>
		public static async void GetXRrigReferencesAsync () {

			maxLoops = 100;

			if (xCam == null) {
				while (xCam == null && maxLoops > 0) {
					await Task.Delay (50);
					xCam = GetXRcam ();
					maxLoops++;
				}
			}

			maxLoops = 100;

			if (xCtrlR == null) {
				while (xCtrlR == null && maxLoops > 0) {
					await Task.Delay (50);
					xCtrlR = GetXRcontrollerRight ();
					maxLoops++;
				}
			}

			maxLoops = 100;

			if (xCtrlL == null) {
				while (xCtrlL == null && maxLoops > 0) {
					await Task.Delay (50);
					xCtrlL = GetXRcontrollerLeft ();
					maxLoops++;
				}
			}

			EventManager.TriggerEvent ("EvFoundXRrigReferences", null);
		}

		/// <summary>
		/// Gets the reference from the SystemManager, or tries to find the HMD camera by searching for it's tag.
		/// </summary>
		/// <returns>Camera object transform</returns>
		public static Transform GetXRcam () {

			if (xCam == null) {
				if (SystemManager.instance != null) {
					xCam = SystemManager.instance.XRrig_MainCamera;
				} else {
					try {
						xCam = GameObject.FindGameObjectWithTag ("XRcam").transform;
					} catch (System.Exception e) {
						Debug.LogError ("XRcam reference not found!'");
						return null;
					}
				}
			}

			return xCam;
		}

		/// <summary>Gets RightController transform from SystemManager or FindWithTag'RightController' </summary>
		/// <returns>Transform of the RightController or null if not found</returns>
		public static Transform GetXRcontrollerRight () {

			if (xCtrlR == null) {
				if (SystemManager.instance != null) {
					xCtrlR = SystemManager.instance.XRrig_RightController;
				} else {
					try {
						xCtrlR = GameObject.FindGameObjectWithTag ("RightController").transform;
					} catch (System.Exception e) {
						Debug.LogError ("RightController reference not found!'");
						return null;
					}
				}
			}

			return xCtrlR;
		}

		/// <summary>Gets LeftController transform from SystemManager or FindWithTag'LeftController' </summary>
		/// <returns>Transform of the LeftController or null if not found</returns>
		public static Transform GetXRcontrollerLeft () {

			if (xCtrlL == null) {
				if (SystemManager.instance != null) {
					xCtrlL = SystemManager.instance.XRrig_LeftController;
				} else {
					try {
						xCtrlL = GameObject.FindGameObjectWithTag ("LeftController").transform;
					} catch (System.Exception e) {
						Debug.LogError ("LeftController reference not found!'");
						return null;
					}
				}
			}

			return xCtrlL;
		}

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region XR HMD and Controller methods

		/// <summary>
		/// Setting the XRInteractorLine component 
		/// </summary>
		/// <param name="_onOff">True or false</param>
		public static void EnableXRInteractorLine (bool _onOff) {
			xCtrlR.GetComponent<XRInteractorLineVisual> ().enabled = _onOff;
			xCtrlL.GetComponent<XRInteractorLineVisual> ().enabled = _onOff;
		}

	}

#endregion // -------------------------------------------------------------------------------------------------------------------------------

}