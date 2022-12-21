using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using RCAS;

namespace eDIA.Manager
{

	/// <summary>Panel for setting up config files, for now choosing them from pre-set versions</summary>
	/// 
	public class PanelPairing : ExperimenterPanel
	{
		public TextMeshProUGUI Output_Info;

		private string ip = "";
		private int port = 0;

		public void BtnTSubmitPressed()
		{
			RCAS_Peer.Instance.ConnectTo(ip, port);
		}

		private void Start()
		{
			RCAS_Peer.Instance.OnReceivedPairingOffer += PairingOfferReceived;
			RCAS_Peer.Instance.OnConnectionEstablished += Connected;
			RCAS_Peer.Instance.OnConnectionLost += Disconnected;
		}

		private void OnDestroy()
		{
			RCAS_Peer.Instance.OnReceivedPairingOffer -= PairingOfferReceived;
			RCAS_Peer.Instance.OnConnectionEstablished -= Connected;
			RCAS_Peer.Instance.OnConnectionLost -= Disconnected;
		}

		void PairingOfferReceived(string ip_address, int port, string deviceInfo)
		{
			if (RCAS_Peer.Instance.isConnected) return;

			Output_Info.text = $"{deviceInfo}";

			ip = ip_address;
			this.port = port;

			ShowPanel();
		}

		void Disconnected(System.Net.EndPoint EP)
		{
			HidePanel();
		}

		void Connected(System.Net.EndPoint EP)
		{
			HidePanel();
		}

	}
}