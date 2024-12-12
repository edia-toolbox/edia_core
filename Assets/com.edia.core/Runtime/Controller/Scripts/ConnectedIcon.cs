using Edia.Controller;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Edia
{
    public class ConnectedIcon : MonoBehaviour
    {
        Image connectedImage;
        public bool isConnected = false;
        Color _unConnectedColor;
        Color disconnected = Color.red;
        public Color ConnectedColor = Color.green;

        private void Awake()
        {
            connectedImage = GetComponent<Image>();
            _unConnectedColor = connectedImage.color;

            if (ControlPanel.Instance.controlMode is ControlMode.Local)
                HideMe();
            else
            {
                EventManager.StartListening(Edia.Events.ControlPanel.EvConnectionEstablished, OnEvConnectionEstablished);
            }
        }

        private void HideMe()
        {
            this.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            EventManager.StopListening(Edia.Events.ControlPanel.EvConnectionEstablished, OnEvConnectionEstablished);
        }

        private void OnEvConnectionEstablished(eParam param)
        {
            this.Add2Console("OnEvConnectionEstablished");
            isConnected = !isConnected;
            connectedImage.color = isConnected ? ConnectedColor : _unConnectedColor;
        }
    }
}