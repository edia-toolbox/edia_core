using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RCAS;
using System;
using System.Net;

// Ms C# Coding Conventions
//
// * PascalCasing
// class, record, or struct, enums
// public members of types, such as fields, properties, events, methods, and local functions
//
// * camelCasing
// private or internal fields, and prefix them with _
//
// Use implicit typing for local variables when the type of the variable is obvious from 
// the right side of the assignment, or when the precise type is not important.
// var var1 = "This is clearly a string.";
// var var2 = 27;
//

namespace eDIA.Manager
{

	public class PanelConsole : MonoBehaviour
	{

		[HideInInspector]
		public GameObject holder = null;
		public Image connectionIcon = null;
		public TextMeshProUGUI textfield;


		private void Start()
		{
			RCAS_Peer.Instance.OnReceivedPairingOffer += PairingOfferReceived;
			RCAS_Peer.Instance.OnConnectionEstablished += Connected;
			RCAS_Peer.Instance.OnConnectionLost += Disconnected;
		}

		private void Disconnected(IPEndPoint EP)
		{
			connectionIcon.color = Color.red;
		}

		private void Connected(IPEndPoint EP)
		{
			connectionIcon.color = Color.green;
		}

		private void PairingOfferReceived(string IPAddress, int Port, string DeviceInfo)
		{
			connectionIcon.color = Color.yellow;
		}

		private void OnDestroy()
		{
			RCAS_Peer.Instance.OnReceivedPairingOffer -= PairingOfferReceived;
			RCAS_Peer.Instance.OnConnectionEstablished -= Connected;
			RCAS_Peer.Instance.OnConnectionLost -= Disconnected;
		}


		public void Add2Console(string msg)
		{
			textfield.text = msg + "\n" + textfield.text;
		}

		public void ShowConsole(bool onOff)
		{
			// textfield.gameObject.SetActive(onOff);
			holder.SetActive(onOff);
		}

		[ContextMenu("ShowConsole")]
		public void ShowConsole()
		{
			ShowConsole(true);
		}

		[ContextMenu("HideConsole")]
		public void HideConsole()
		{
			ShowConsole(false);
		}

		public void Toggle()
		{
			ShowConsole(!holder.gameObject.activeSelf);
		}

	}

}
