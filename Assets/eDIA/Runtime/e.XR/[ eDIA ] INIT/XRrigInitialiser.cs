using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA {
        
    /// <summary> Injects eDIA XRrig prefab in the current loaded scene at Awake and positions the playarea on this gameobjects transform. </summary>
    public class XRrigInitialiser : MonoBehaviour
    {
        public GameObject XRrigPrefab;

        void Awake() {
            if (XRrigManager.instance == null) {
                Instantiate(XRrigPrefab, new Vector3(0,0,0), Quaternion.identity);
            }
            
            XRrigManager.instance.MovePlayarea(this.transform);
            XRrigManager.instance.AddToLog("Positioned playarea");
        }
    }
}