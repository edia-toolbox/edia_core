using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkActionDemo : MonoBehaviour {

    public GameObject[] NetworkObjects;

    public Image demoImg;
    public Text demoText;

    public void Action_ProcessRpcMessage(string RpcMsg)
    {
        if (RpcMsg.Contains("Cmd"))
        {
            string StrCmd = RpcMsg.Substring(3, RpcMsg.Length - 3);

            if (StrCmd.Contains(","))
            {
                //has variables
                string[] StrGrp = StrCmd.Split(',');

                if (StrGrp[1].Contains("(bool)"))
                {
                    bool var1 = bool.Parse(StrGrp[1].Substring(6, StrGrp[1].Length - 6));
                    foreach (GameObject obj in NetworkObjects)
                    {
                        obj.SendMessage(StrGrp[0], var1, SendMessageOptions.DontRequireReceiver);
                    }
                }
                if (StrGrp[1].Contains("(int)"))
                {
                    int var1 = int.Parse(StrGrp[1].Substring(5, StrGrp[1].Length - 5));
                    foreach (GameObject obj in NetworkObjects)
                    {
                        obj.SendMessage(StrGrp[0], var1, SendMessageOptions.DontRequireReceiver);
                    }
                }
                if (StrGrp[1].Contains("(float)"))
                {
                    float var1 = float.Parse(StrGrp[1].Substring(7, StrGrp[1].Length - 7));
                    foreach (GameObject obj in NetworkObjects)
                    {
                        obj.SendMessage(StrGrp[0], var1, SendMessageOptions.DontRequireReceiver);
                    }
                }
                if (StrGrp[1].Contains("(string)"))
                {
                    string var1 = StrGrp[1].Substring(8, StrGrp[1].Length - 8);
                    foreach (GameObject obj in NetworkObjects)
                    {
                        obj.SendMessage(StrGrp[0], var1, SendMessageOptions.DontRequireReceiver);
                    }
                }
                if (StrGrp[1].Contains("(Vector3)"))
                {
                    string[] substring = StrGrp[1].Substring(9, StrGrp[1].Length - 9).Split(':');
                    Vector3 var1 = new Vector3(float.Parse(substring[0]), float.Parse(substring[1]), float.Parse(substring[2]));
                    foreach (GameObject obj in NetworkObjects)
                    {
                        obj.SendMessage(StrGrp[0], var1, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
            else
            {
                foreach (GameObject obj in NetworkObjects)
                {
                    obj.SendMessage(StrCmd, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    public void Rpc_SetInt(int _value)
    {
        NetworkActionClient.instance.Action_RpcSend("Action_SetInt,(int)" + _value);
    }
    public void Rpc_SetFloat(float _value)
    {
        NetworkActionClient.instance.Action_RpcSend("Action_SetFloat,(float)" + _value);
    }
    public void Rpc_SetBool(bool _value)
    {
        NetworkActionClient.instance.Action_RpcSend("Action_SetBool,(bool)" + _value);
    }
    public void Rpc_SetString(string _value)
    {
        NetworkActionClient.instance.Action_RpcSend("Action_SetString,(string)" + _value);
    }
    public void Rpc_SetVector3(Vector3 _value)
    {
        NetworkActionClient.instance.Action_RpcSend("Action_SetVector3,(Vector3)" + _value.x.ToString() + ":" + _value.y.ToString() + ":" + _value.z.ToString());
    }
    public void Rpc_SetVector3Random()
    {
        Rpc_SetVector3(new Vector3(Random.value, Random.value, Random.value));
    }

    public void Cmd_ServerSendByte()
    {
        if (NetworkActionServer.instance != null)
        {
            float rand = Random.value;
            NetworkActionServer.instance.Action_AddCmd(System.Text.Encoding.ASCII.GetBytes("Hello World: " + rand));
        }
    }
    public void Action_ProcessByteData(byte[] _data)
    {
        if (_data.Length > 100) return;
        demoText.text = "[Decode Bytes] " + System.Text.Encoding.ASCII.GetString(_data);
    }

    void Action_SetColorRed()
    {
        demoImg.color = Color.red;
    }

    void Action_SetColorGreen()
    {
        demoImg.color = Color.green;
    }
    void Action_SetInt(int _value)
    {
        demoText.text = "[Receive] demo int: " + _value;
    }
    void Action_SetFloat(float _value)
    {
        demoText.text = "[Receive] demo float: " + _value;
    }
    void Action_SetBool(bool _value)
    {
        demoText.text = "[Receive] demo bool: " + _value;
    }
    void Action_SetString(string _value)
    {
        demoText.text = "[Receive] demo string: " + _value;
    }
    void Action_SetVector3(Vector3 _value)
    {
        demoText.text = "[Receive] demo Vector3: " + "(" + _value.x + ", " + _value.y + ", " + _value.z + ")";
    }
}

