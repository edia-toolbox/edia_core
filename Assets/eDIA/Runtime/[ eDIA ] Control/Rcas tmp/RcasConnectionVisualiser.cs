using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RCAS;
using UnityEngine.UI;
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

namespace eDIA.Manager {

	public class RcasConnectionVisualiser : MonoBehaviour
	{

		Image connectionIcon = null;


		private void Start()
		{
			connectionIcon = GetComponent<Image>();
			
			RCAS_Peer.Instance.OnConnectionEstablished += Connected;
			RCAS_Peer.Instance.OnConnectionLost += Disconnected;

		}

		private void OnDestroy()
		{
			RCAS_Peer.Instance.OnConnectionEstablished -= Connected;
			RCAS_Peer.Instance.OnConnectionLost -= Disconnected;
		}

		private void Disconnected(IPEndPoint EP)
		{
			connectionIcon.color = Color.red;
		}

		private void Connected(IPEndPoint EP)
		{
			connectionIcon.color = Color.green;
		}


	}
	
}
