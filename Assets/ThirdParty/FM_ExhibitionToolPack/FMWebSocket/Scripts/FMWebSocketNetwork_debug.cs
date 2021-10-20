using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMSocketIO;

public class FMWebSocketNetwork_debug : MonoBehaviour {

    public Text debugText;

    public void Action_SendStringAll(string _string)
    {
        FMSocketIOManager.instance.SendToAll(_string);
    }
    public void Action_SendStringServer(string _string)
    {
        FMSocketIOManager.instance.SendToServer(_string);
    }

    public void Action_SendStringOthers(string _string)
    {
        FMSocketIOManager.instance.SendToOthers(_string);
    }

    public void Action_SendByteAll()
    {
        FMSocketIOManager.instance.SendToAll(new byte[3]);
    }
    public void Action_SendByteServer()
    {
        FMSocketIOManager.instance.SendToServer(new byte[4]);
    }
    public void Action_SendByteOthers()
    {
        FMSocketIOManager.instance.SendToOthers(new byte[5]);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        debugText.text = "";
        if(FMSocketIOManager.instance != null) debugText.text += "[connected: "+FMSocketIOManager.instance.Ready+"]";
        debugText.text += _received;

    }

    string _received = "";
    public void Action_OnReceivedData(string _string)
    {
        //debugText.text = "received: " + _string;
        _received = "received: " + _string;
    }
    public void Action_OnReceivedData(byte[] _byte)
    {
        //debugText.text = "received(byte): " + _byte.Length;
        _received = "received(byte): " + _byte.Length;
    }
}
