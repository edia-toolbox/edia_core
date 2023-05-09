using System.Collections;
using System.Collections.Generic;
using UnityEngine;

	[CreateAssetMenu(fileName = "XRDevicesSOBJ", menuName = "eDIA/XRDevicesDB")]
	public class XRDevicesSOBJ : ScriptableObject
	{
		[System.Serializable]
		public struct XRDeviceItem
		{
			public string name;
			public Sprite icon;

			//TODO complete with interesting data
		}

		[SerializeField]
		public List<XRDeviceItem> XRDevices;

	}

