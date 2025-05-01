using UnityEngine;

namespace StartersKit {
    public class CubeCatcher : MonoBehaviour {
        public Transform CubeInitionPosition;

        private void OnTriggerEnter(Collider other) {
            if (other.transform.parent.name is "Stimuli")
                other.transform.parent.position = CubeInitionPosition.position;
        }
    }

}