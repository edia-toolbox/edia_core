using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace eDIA {

	/// <summary>Base Panel functionality class</summary>
	public class ExperimenterPanel : MonoBehaviour {
		
		Transform menuPanel;

		[HideInInspector]
		public List<Transform> children = new List<Transform>();


		public virtual void Awake() {

			menuPanel = transform.parent; // The main panel all subpanels are childs off

			foreach (Transform tr in transform) children.Add(tr); // get a list of all children of this subpanel
		}

		public void ShowPanel () {

			foreach (Transform tr in children) tr.gameObject.SetActive(true);
			
			transform.parent = menuPanel;

			menuPanel.GetComponent<PanelLayoutManager>().UpdatePanelOrder();
		}

		public void HidePanel () {
			foreach (Transform tr in children) tr.gameObject.SetActive(false);
			transform.parent = null;

			menuPanel.GetComponent<PanelLayoutManager>().UpdatePanelOrder();
		}

		public void ApplyTheme () {
			
		}


	}
}