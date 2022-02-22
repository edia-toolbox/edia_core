using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace eDIA {

	/// <summary>Base Panel functionality class</summary>
	[RequireComponent(typeof(VerticalLayoutGroup))]
	public class ExperimenterPanel : MonoBehaviour {
		
		public List<Transform> children = new List<Transform>();

		public virtual void Awake() {
			foreach (Transform tr in transform) children.Add(tr);
		}

		public void ShowPanel () {
			foreach (Transform tr in children) tr.gameObject.SetActive(true);
			GetComponent<VerticalLayoutGroup>().enabled = true;
		}

		public void HidePanel () {
			foreach (Transform tr in children) tr.gameObject.SetActive(false);
			GetComponent<VerticalLayoutGroup>().enabled = false;
		}

		public void ApplyTheme () {
			
		}


	}
}