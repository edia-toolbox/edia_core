using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace eDIA {

	/// <summary>Base Panel functionality class</summary>
	public class ExperimenterPanel : MonoBehaviour {
		
		Transform myParent;
		// public List<Transform> preSetSibblingList = new List<Transform>();
		// public List<Transform> currentSibblingList = new List<Transform>();

		[HideInInspector]
		public List<Transform> children = new List<Transform>();

		public int myOriginalIndex = -1;


		public virtual void Awake() {

			myParent = transform.parent; // The main panel all subpanels are childs off
			// foreach (Transform tr in transform.parent) preSetSibblingList.Add(tr); // get a list of all panel siblings in parent

			foreach (Transform tr in transform) children.Add(tr); // get a list of all children of this subpanel
		}

		public void ShowPanel () {

			foreach (Transform tr in children) tr.gameObject.SetActive(true);
			
			transform.parent = myParent;

			myParent.GetComponent<PanelLayoutManager>().UpdatePanelOrder(transform, myOriginalIndex);

			// int myOriginalIndex = preSetSibblingList.FindIndex(x => x == transform);
			
			// Debug.Log("myIndex: " + myIndex);

			// if (myOriginalIndex > -1) { // Exists
			// 	if (myOriginalIndex == 0) { // first in list
			// 		transform.SetAsFirstSibling();
			// 	}
			// 	else {
			// 		// Debug.Log("zoek zoek");

			// 		currentSibblingList.Clear();
			// 		foreach (Transform tr in transform.parent) currentSibblingList.Add(tr);

			// 		for (int i=currentSibblingList.Count-1;i>=0;i--) {
						
			// 			int checkIndex = preSetSibblingList.FindIndex(x => x == currentSibblingList[i]);
			// 			// Debug.Log("Index: " + i + " checkindex: " + checkIndex);

			// 			if (checkIndex == -1)
			// 				continue;
			// 			else {
			// 				if (checkIndex > myOriginalIndex) {
			// 					transform.SetSiblingIndex(i);
			// 					// Debug.Log("SetSiblingIndex: " + i);
			// 				}
			// 			}
			// 		}
			// 	}
			// }
			// else { // New so just add to the end
			// 	transform.SetAsLastSibling();
			// }

		}

		public void HidePanel () {
			foreach (Transform tr in children) tr.gameObject.SetActive(false);
			transform.parent = null;
		}

		public void ApplyTheme () {
			
		}


	}
}