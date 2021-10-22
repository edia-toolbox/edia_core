using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA {
        
    public class XRrigInitialiser : MonoBehaviour
    {
        public GameObject XRrigPrefab;

        void Awake() {
            if (SystemManager.instance == null) {
                Instantiate(XRrigPrefab, new Vector3(0,0,0), Quaternion.identity);
            }
            
            SystemManager.instance.MovePlayarea(this.transform);
            SystemManager.instance.AddToLog("Positioned playarea");
        }
    }
}