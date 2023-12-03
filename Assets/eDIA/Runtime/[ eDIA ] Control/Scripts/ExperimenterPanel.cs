using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace eDIA.Manager {

	/// <summary>Base Panel functionality</summary>
	public class ExperimenterPanel : MonoBehaviour {
		
		[HideInInspector]
		public List<Transform> children = new List<Transform>();
		[HideInInspector]
		public Transform myParent = null; // not used but meant for remote panel config

		public virtual void Awake() {
			myParent = transform.parent;
			GetComponent<Image>().enabled = false;
			foreach (Transform tr in transform) children.Add(tr); // get a list of all children of this subpanel
		}

		public void ShowPanel (bool onOff) {

			foreach (Transform tr in children) tr.gameObject.SetActive(onOff);
			
			GetComponent<Image>().enabled = onOff;
			ControlPanel.Instance.ShowPanel(transform, onOff);

		}

		[ContextMenu("HidePanel")]
		public void HidePanel () {
			ShowPanel(false);
		}

		[ContextMenu("ShowPanel")]
		public void ShowPanel () {
			ShowPanel(true);
		}

	}
}