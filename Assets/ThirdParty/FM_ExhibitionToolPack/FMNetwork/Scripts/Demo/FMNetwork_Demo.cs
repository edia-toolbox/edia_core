using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UI;

public class FMNetwork_Demo : MonoBehaviour {

    public Text ServerText;
    public Text ClientText;

    public void Action_ProcessStringData(string _string)
    {
        if(FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
        {
            if (ServerText != null) ServerText.text = "Server Received : " + _string;
        }
        else
        {
            if (ClientText != null) ClientText.text = "Client Received : " + _string;
        }
    }

    public void Action_ProcessByteData(byte[] _byte)
    {
        if (FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
        {
            if (ServerText != null) ServerText.text = "Server Received byte[] : " + _byte.Length;
        }
        else
        {
            if (ClientText != null) ClientText.text = "Client Received byte[]: " + _byte.Length;
        }
    }

    public void Action_ShowRawByteLength(byte[] _byte)
    {
        Debug.Log("get byte length: " + _byte.Length);
    }

    // Use this for initialization
    void Start () {
        if (Box1 != null) StartPosBox1 = Box1.transform.position;
        if (Box2 != null) StartPosBox2 = Box2.transform.position;
	}

    public GameObject Box1;
    public GameObject Box2;
    public Vector3 StartPosBox1;
    public Vector3 StartPosBox2;
    public Toggle toggle;

    public LargeFileEncoder LFE;
    public Text LFE_text;

    [Space]
    [Header("[New] Send To Target IP")]
    public string TargetIP = "127.0.0.1";
    public void Action_SetTargetIP(string _targetIP) { TargetIP = _targetIP; }

    public void Action_SendRandomLargeFile()
    {
        LFE.Action_SendLargeByte(new byte[(int)(1024f * 1024f * ((int)Random.Range(2, 4)))]);
    }
    public void Action_DemoReceivedLargeFile(byte[] _data)
    {
        //LFE_text.text = (float)_data.Length/(float)(1024*1024) + " MB";
    }

    private void Update()
    {
        if (FMNetworkManager.instance.NetworkType == FMNetworkType.Server)
        {
            if (toggle.isOn)
            {
                if (Box1 != null && Box2 != null)
                {
                    Box1.transform.Rotate(new Vector3(0f, Time.deltaTime * 15f, 0f));
                    Box1.transform.position = new Vector3(Mathf.Sin(Time.realtimeSinceStartup), 0f, 0f) + StartPosBox1;
                    Box1.transform.localScale = new Vector3(1f, 1f, 1f) * (1f + 0.5f * Mathf.Sin(Time.realtimeSinceStartup * 3f));

                    Box2.transform.Rotate(new Vector3(0f, Time.deltaTime * 30f, 0f));
                    Box2.transform.position = new Vector3(Mathf.Sin(Time.realtimeSinceStartup * 0.5f), 0f, -1f) + StartPosBox2;
                    Box2.transform.localScale = new Vector3(1f, 1f, 1f) * (1f + 0.5f * Mathf.Sin(Time.realtimeSinceStartup * 2f));
                }
            }
        }
    }

    public void Action_SendByteToAll(int _value)
    {
        FMNetworkManager.instance.SendToAll(new byte[_value]);
    }
    public void Action_SendByteToServer(int _value)
    {
        FMNetworkManager.instance.SendToServer(new byte[_value]);
    }
    public void Action_SendByteToOthers(int _value)
    {
        FMNetworkManager.instance.SendToOthers(new byte[_value]);
    }
    public void Action_SendByteToTarget(int _value)
    {
        FMNetworkManager.instance.SendToTarget(new byte[_value], TargetIP);
    }

    public void Action_SendTextToAll(string _value)
    {
        FMNetworkManager.instance.SendToAll(_value);
    }
    public void Action_SendTextToServer(string _value)
    {
        FMNetworkManager.instance.SendToServer(_value);
    }
    public void Action_SendTextToOthers(string _value)
    {
        FMNetworkManager.instance.SendToOthers(_value);
    }
    public void Action_SendTextToTarget(string _value)
    {
        FMNetworkManager.instance.SendToTarget(_value, TargetIP);
    }

    public void Action_SendRandomTextToAll()
    {
        string _value = Random.value.ToString();
        FMNetworkManager.instance.SendToAll(_value);
    }
    public void Action_SendRandomTextToServer()
    {
        string _value = Random.value.ToString();
        FMNetworkManager.instance.SendToServer(_value);
    }
    public void Action_SendRandomTextToOthers()
    {
        string _value = Random.value.ToString();
        FMNetworkManager.instance.SendToOthers(_value);
    }
    public void Action_SendRandomTextToTarget()
    {
        string _value = Random.value.ToString();
        FMNetworkManager.instance.SendToTarget(_value, TargetIP);
    }
}
