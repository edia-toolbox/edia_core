using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eDIA {

	public class PanelLayoutManager : MonoBehaviour {
		
		public List<Transform> originalPanelOrder = new List<Transform>();
		public List<Transform> currentPanelOrder = new List<Transform>();

		void Awake() {
			foreach (Transform tr in transform) {
				originalPanelOrder.Add(tr); // get a list of all children of this subpanel
				tr.GetComponent<ExperimenterPanel>().myOriginalIndex = tr.GetSiblingIndex();
			}
		}

		public void UpdatePanelOrder (Transform child, int childsOriginalIndex) {
			if (childsOriginalIndex == -1)
				return;

			int newIndex = -1;

			currentPanelOrder.Clear();
			currentPanelOrder = transform.Cast<Transform>().ToList();

			for (int i=0;i<currentPanelOrder.Count;i++) {
				newIndex = currentPanelOrder[i].GetComponent<ExperimenterPanel>().myOriginalIndex > childsOriginalIndex ? i : newIndex;
			}

			child.SetSiblingIndex(newIndex);
		}

	}
}