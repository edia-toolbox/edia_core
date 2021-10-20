using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionDebugText : MonoBehaviour
{
    public Text DebugText;

    public NetworkDiscovery ND;
    public NetworkActionClient NC;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(DebugText == null) return;

        string debugStr = "";
        debugStr += "num thread: " + Loom.numThreads + " / " + Loom.maxThreads + "\n";
        if (ND != null)
        {
            debugStr += "Network Discovery\n";
            debugStr += "-server IP: " + ND.ServerIP + "\n";
            debugStr += "-client IP: " + ND.ClientIP + "\n";
            debugStr += "-server received: " + ND.ServerStr + "\n";
            debugStr += "-client received: " + ND.ClientStr + "\n";
        }

        if (NC != null)
        {
            debugStr += "Network TCP Client\n";
            debugStr += "-connected: " + NC.isConnected + "\n";
            debugStr += "-Ip: " + NC.IP + "\n";
        }

        DebugText.text = debugStr;
    }
}
