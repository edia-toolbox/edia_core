using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace eDIA {

	/// <summary>Base Panel functionality class</summary>
	[RequireComponent(typeof(LayoutElement))]
	public class ExperimenterPanel : MonoBehaviour {
		
		public void ShowPanel () {
			transform.GetChild(0).gameObject.SetActive(true);
			GetComponent<LayoutElement>().ignoreLayout = false;
		}

		public void HidePanel () {
			transform.GetChild(0).gameObject.SetActive(false);
			GetComponent<LayoutElement>().ignoreLayout = true;
		}


	}
}