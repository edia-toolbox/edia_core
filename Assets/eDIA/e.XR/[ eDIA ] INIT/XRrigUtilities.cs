using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace eDIA {

    /// <summary>
    /// Project wide utilities relating to XR rig
    /// </summary>
    public static class XRrigUtilities
    {
        private static Transform xCam = null;
        private static Transform xCtrlR = null;
        private static Transform xCtrlL = null;

        private static int maxLoops = 100;

        /// <summary>Gets XRrig references from SystemManager or FindWithTag  </summary>
        public static async void GetXRrigReferencesAsync () {
            
            maxLoops = 100;
            
            if (xCam == null) {
                while( xCam == null && maxLoops > 0 ) {
                    await Task.Delay(50);
                    xCam = GetXRcam();
                    maxLoops--;
                }
            }

            maxLoops = 100;

            if (xCtrlR == null) {
                while( xCtrlR == null && maxLoops > 0) {
                    await Task.Delay(50);
                    xCtrlR = GetXRcontrollerRight();
                    maxLoops--;
                }
            }

            maxLoops = 100;

            if (xCtrlL == null) {
                while( xCtrlL == null && maxLoops > 0) {
                    await Task.Delay(50);
                    xCtrlL = GetXRcontrollerLeft();
                    maxLoops--;
                }
            }

            EventManager.TriggerEvent("EvFoundXRrigReferences", null);
        }

        /// <summary>Searches for the correct <c>Camera</c> reference</summary>
        /// <returns>Transform of the camera gameobject</returns>
        public static Transform GetXRcam () {

            if (xCam == null) {
                if (SystemManager.instance != null) {
                    xCam = SystemManager.instance.XRrig_MainCamera;
                } else {
                    try {
                        xCam = GameObject.FindGameObjectWithTag("XRcam").transform;
                    } catch (System.Exception e) {
                        Debug.LogError("XRcam reference not found!'");
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
                        xCtrlR = GameObject.FindGameObjectWithTag("RightController").transform;
                    } catch (System.Exception e) {
                        Debug.LogError("RightController reference not found!'");
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
                        xCtrlL = GameObject.FindGameObjectWithTag("LeftController").transform;
                    } catch (System.Exception e) {
                        Debug.LogError("LeftController reference not found!'");
                        return null;
                    }
                }
            }

            return xCtrlL;
        }


    }
}
