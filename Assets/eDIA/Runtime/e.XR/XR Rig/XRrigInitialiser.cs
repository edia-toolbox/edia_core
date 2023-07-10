using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA {
        
    /// <summary> Injects eDIA XRrig prefab in the current loaded scene at Awake and positions the playarea on this gameobjects transform. </summary>
    public class XRrigInitialiser : MonoBehaviour
    {
        public GameObject XRrigPrefab;

        void Awake() {
            if (XRManager.Instance == null) {
                Instantiate(XRrigPrefab, new Vector3(0,0,0), Quaternion.identity);
            }
            
            XRManager.Instance.MovePlayarea(this.transform);
            XRManager.Instance.AddToLog("Positioned playarea");
        }
    }
}