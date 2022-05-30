using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace eDIA {

	/// <summary>Base Panel functionality class</summary>
	public class ExperimenterPanel : MonoBehaviour {
		
		public List<Transform> children = new List<Transform>();
		Transform myParent;

		public virtual void Awake() {
			myParent = transform.parent;

			foreach (Transform tr in transform) children.Add(tr);
		}

		public void ShowPanel () {
			foreach (Transform tr in children) tr.gameObject.SetActive(true);
			transform.parent = myParent;
		}

		public void HidePanel () {
			foreach (Transform tr in children) tr.gameObject.SetActive(false);
			transform.parent = null;
		}

		public void ApplyTheme () {
			
		}


	}
}