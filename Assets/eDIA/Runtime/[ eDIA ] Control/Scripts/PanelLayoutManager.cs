using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA {

	public class PanelLayoutManager : MonoBehaviour {
		
		public List<Transform> currentPanelOrder = new List<Transform>();
		public Transform nonActivePanelHolder = null;
		
		void Awake() {
			foreach (Transform tr in transform) {
				tr.name = tr.GetSiblingIndex().ToString() + "_" + tr.name;
			}
		}

		public void UpdatePanelOrder () {

			currentPanelOrder.Clear();
			currentPanelOrder = transform.Cast<Transform>().ToList();
			currentPanelOrder.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });

			for (int i=0; i<currentPanelOrder.Count; ++i) {
				currentPanelOrder[i].SetSiblingIndex(i);
			}
		}
	}
}