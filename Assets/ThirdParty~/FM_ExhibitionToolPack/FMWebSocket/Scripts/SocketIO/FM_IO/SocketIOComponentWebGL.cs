using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using FMSocketIO;
using WebSocketSharp;
using WebSocketSharp.Net;

using System.Runtime.InteropServices;

[System.Serializable]
public class EventJson
{
    public string socketEvent;
    public string eventData;
}

public class SocketIOComponentWebGL : MonoBehaviour
{
    public static SocketIOComponentWebGL instance;
    public string sid;

    // Use this for initialization
    void Awake() { if (instance == null) instance = this; }
    public string IP = "127.0.0.1";
    public int port = 3000;
    public bool sslEnabled = false;

    public bool portRequired = true;
    public bool socketIORequired = true;

    public bool DefaultQueryString = true;
    public string CustomisedQueryString = "?EIO=4&transport=websocket";

    public bool DebugMode = true;
    private void DebugLog(string _value)
    {
        if (!DebugMode) return;
        Debug.Log("FMLog: " + _value);
    }

    int packetId;
    Dictionary<string, List<Action<SocketIOEvent>>> eventHandlers;
    List<Ack> ackList;

    private bool _WebSocketConnected = false;
    public bool IsWebSocketConnected() { return _WebSocketConnected; }

    bool Ready = false;
    public bool IsReady() { return Ready; }

    private void Update()
    {

        if (FMSocketIOManager.instance != null) DebugMode = FMSocketIOManager.instance.DebugMode;

#if UNITY_WEBGL
        if (Ready)
        {
            if (!socketIORequired)
            {
                REG_IsWebSocketConnected(gameObject.name);
            }
            else
            {
                IsSocketIOConnected(gameObject.name);
            }
        }
#endif
    }

#if UNITY_WEBGL
    //>>> SocketIO >>>
    [DllImport("__Internal")]
	private static extern void WebSocketAddSocketIO(string _src);
	[DllImport("__Internal")]
	private static extern void WebSocketAddEventListeners(string _gameobject);
	[DllImport("__Internal")]
	private static extern void WebSocketConnect(string _src, string _gameobject);
    [DllImport("__Internal")]
    private static extern void WebSocketClose();
    [DllImport("__Internal")]
    private static extern void WebSocketEmitEvent(string _e);
    [DllImport("__Internal")]
    private static extern void WebSocketEmitData(string _e, string _data);
    [DllImport("__Internal")]
    private static extern void WebSocketEmitEventAction(string _e, string _packetId, string _gameobject);
    [DllImport("__Internal")]
    private static extern void WebSocketEmitDataAction(string _e, string _data, string _packetId, string _gameobject);
    [DllImport("__Internal")]
    private static extern void WebSocketOn(string _e);
    //>>> SocketIO >>>

    //>>> Check connection >>>
    [DllImport("__Internal")]
    private static extern void IsSocketIOConnected(string _gameobject);
    [DllImport("__Internal")]
    private static extern void REG_IsWebSocketConnected(string _gameobject);
    //>>> Check connection >>>

    //>>> Echo Server Test >>>
    [DllImport("__Internal")]
    private static extern void REG_WebSocketAddEventListeners(string _src, string _gameobject);
    [DllImport("__Internal")]
    private static extern void REG_Send(string _src);
    [DllImport("__Internal")]
    private static extern void REG_Close();
    //>>> Echo Server Test >>>
#endif

    public void Init()
    {
        if (!socketIORequired) return;

        eventHandlers = new Dictionary<string, List<Action<SocketIOEvent>>>();
        ackList = new List<Ack>();
        AddSocketIO();
        AddEventListeners();
    }

    private void OnConnected(SocketIOEvent e) { DebugLog("[Event] SocketIO connected"); }

    void AddSocketIO()
    {
#if UNITY_WEBGL
        string src = "http" + (sslEnabled ? "s" : "") + "://" + IP;
        //if (portRequired) src += (!sslEnabled && port != 0 ? ":" + port.ToString() : "");
        if (portRequired) src += (port != 0 ? ":" + port.ToString() : "");
        src += "/socket.io/socket.io.js/";
        WebSocketAddSocketIO(src);
#endif
    }
    void AddEventListeners()
    {
#if UNITY_WEBGL
        WebSocketAddEventListeners(gameObject.name);
#endif
    }

    public void Connect()
    {
        DebugLog(">>> start connecting");
#if UNITY_WEBGL
        if (!socketIORequired)
        {
            string src = "ws" + (sslEnabled ? "s" : "") + "://" + IP;
            //if (portRequired) src += (!sslEnabled && port != 0 ? ":" + port.ToString() : "");
            if (portRequired) src += (port != 0 ? ":" + port.ToString() : "");
            REG_WebSocketAddEventListeners(src, gameObject.name);
        }
        else
        {
            string src = "http" + (sslEnabled ? "s" : "") + "://" + IP;
            //if (portRequired) src += (!sslEnabled && port != 0 ? ":" + port.ToString() : "") + "/";
            if (portRequired) src += (port != 0 ? ":" + port.ToString() : "") + "/";
            if (!DefaultQueryString) src += CustomisedQueryString;
            WebSocketConnect(src, gameObject.name);
        }
#endif
    }
    public void Close()
    {
#if UNITY_WEBGL
        if (!socketIORequired)
        {
            REG_Close();
        }
        else
        {
            WebSocketClose();
        }
#endif
        Ready = false;
    }


    public void Emit(string e)
    {
#if UNITY_WEBGL
        if (!socketIORequired)
        {
            REG_Send(e);
        }
        else
        {
            WebSocketEmitEvent(e);
        }
#endif
    }

    public void Emit(string e, string data)
    {
#if UNITY_WEBGL
        if (!socketIORequired)
        {
            REG_Send(e + ":" + data);
        }
        else
        {
            WebSocketEmitData(e, string.Format("{0}", data));
        }
#endif
    }

    public void Emit(string e, Action<string> action)
    {
        if (!socketIORequired) return;
#if UNITY_WEBGL
        packetId++;
        WebSocketEmitEventAction(e, packetId.ToString(), gameObject.name);
        ackList.Add(new Ack(packetId, action));
#endif
    }

    public void Emit(string e, string data, Action<string> action)
    {
        if (!socketIORequired) return;
#if UNITY_WEBGL
        packetId++;
        WebSocketEmitDataAction(e, data, packetId.ToString(), gameObject.name);
        ackList.Add(new Ack(packetId, action));
#endif
    }

    public void On(string e, Action<SocketIOEvent> callback)
    {
        if (!socketIORequired) return;
#if UNITY_WEBGL
        if (!eventHandlers.ContainsKey(e)) eventHandlers[e] = new List<Action<SocketIOEvent>>();
        eventHandlers[e].Add(callback);
        WebSocketOn(e);
#endif
    }

    public void Off(string e, Action<SocketIOEvent> callback)
    {
        if (!eventHandlers.ContainsKey(e)) return;
        List<Action<SocketIOEvent>> _eventHandlers = eventHandlers[e];
        if (!_eventHandlers.Contains(callback)) return;
        _eventHandlers.Remove(callback);
        if (_eventHandlers.Count == 0) eventHandlers.Remove(e);
    }

    public void InvokeAck(string ackJson)
    {
        Ack ack;
        Ack ackData = JsonUtility.FromJson<Ack>(ackJson);
        for (int i = 0; i < ackList.Count; i++)
        {
            if (ackList[i].packetId == ackData.packetId)
            {
                ack = ackList[i];
                ackList.RemoveAt(i);
                ack.Invoke(ackJson);
                return;
            }
        }
    }

    public void OnOpen()
    {
        Ready = true;
        DebugLog(">>> UNITY: ON OPEN");
    }

    public void SetSocketID(string socketID)
    {
        sid = socketID;
        DebugLog("socket id !: " + socketID);
        FMSocketIOManager.instance.Settings.socketID = sid;
    }

    public void InvokeEventCallback(string eventJson)
    {
        //DebugLog("getting event!");
        EventJson eventData = JsonUtility.FromJson<EventJson>(eventJson);
        if (!eventHandlers.ContainsKey(eventData.socketEvent)) return;
        for (int i = 0; i < eventHandlers[eventData.socketEvent].Count; i++)
        {
            SocketIOEvent socketEvent = new SocketIOEvent(eventData.socketEvent, eventData.eventData);
            eventHandlers[eventData.socketEvent][i](socketEvent);
        }
    }


    public void RegOnOpen()
    {
        Ready = true;
        //DebugLog(">>> UNITY: ON OPEN");
    }
    public void RegOnClose()
    {
        Ready = false;
        _WebSocketConnected = false;
        //DebugLog(">>> UNITY: ON Close");
    }

    public void RegOnMessage(string _msg)
    {
        if (FMSocketIOManager.instance != null) FMSocketIOManager.instance.OnReceivedRawMessageEvent.Invoke(_msg);
        //DebugLog(">>> UNITY: (MESSAGE) " + _msg);
    }
    public void RegOnError(string _msg)
    {
        //DebugLog(">>> UNITY: (Error) " + _msg);
    }

    public void RegWebSocketConnected()
    {
        _WebSocketConnected = true;
        //DebugLog(">>>>>>>>>>>>>>>> UNITY: (ReadyState) Connected! ");
    }
    public void RegWebSocketDisconnected()
    {
        _WebSocketConnected = false;
        //DebugLog(">>>>>>>>>>>>>>>> UNITY: (ReadyState) Not Connected! ");
    }
}
