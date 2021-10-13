#region License
/*
 * SocketIO.cs
 *
 * The MIT License
 *
 * Copyright (c) 2014 Fabio Panettieri
 * Copyright (c) 2019 Frozen Mist
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace FMSocketIO
{
    [System.Serializable]
    public class SocketOpenData
    {
        public string sid;
        public string[] upgrades;
        public int pingInterval;
        public int pingTimeout;
    }

    public class SocketIOComponent : MonoBehaviour
    {
        #region Public Properties
        public static SocketIOComponent instance;
        public string IP = "127.0.0.13";
        public int port = 3000;

        public bool sslEnabled = false;
        public FMSslProtocols sslProtocols = FMSslProtocols.Default;

        public bool portRequired = true;
        public bool socketIORequired = true;

        public bool DefaultQueryString = true;
        public string CustomisedQueryString = "?EIO=4&transport=websocket";

        [HideInInspector]
        public string url = "ws://127.0.0.1:4567/socket.io/?EIO=4&transport=websocket";
        public bool autoConnect = true;
        public int reconnectDelay = 5;
        public float ackExpirationTime = 1800f;
        public float pingInterval = 25f;
        public float pingTimeout = 60f;

        public WebSocket socket { get { return ws; } }
        public string sid { get; set; }
        public bool IsConnected { get { return connected; } }

        #endregion

        #region Private Properties

        private volatile bool connected;
        private volatile bool thPinging;
        private volatile bool thPong;
        private volatile bool wsConnected;

        public bool IsWebSocketConnected()
        {
            return IsWebSocketConnected(ws);
        }
        private bool IsWebSocketConnected(WebSocket _ws)
        {
            return _ws.ReadyState == WebSocketState.Open || _ws.ReadyState == WebSocketState.Closing;
        }

        private Thread socketThread;
        private Thread pingThread;
        private WebSocket ws;

        private Encoder encoder;
        private Decoder decoder;
        private Parser parser;

        private Dictionary<string, List<Action<SocketIOEvent>>> handlers;
        private List<Ack> ackList;

        private int packetId;

        private object eventQueueLock;
        private Queue<SocketIOEvent> eventQueue;

        private object ackQueueLock;
        private Queue<Packet> ackQueue;

        #endregion

        
        private object RawMessageQueueLock;
        private Queue<string> RawMessageQueue;

        #region Unity interface
        public bool Initialised = false;

        public bool DebugMode = true;
        private void DebugLog(string _value)
        {
            if (!DebugMode) return;
            Debug.Log("FMLog: " + _value);
        }

        public void Awake()
        {
            if (instance == null) instance = this;
        }

        public void Init()
        {
            encoder = new Encoder();
            decoder = new Decoder();
            parser = new Parser();

            packetId = 0;

            handlers = new Dictionary<string, List<Action<SocketIOEvent>>>();

            eventQueueLock = new object();
            eventQueue = new Queue<SocketIOEvent>();

            ackQueueLock = new object();
            ackQueue = new Queue<Packet>();
            ackList = new List<Ack>();

            RawMessageQueueLock = new object();
            RawMessageQueue = new Queue<string>();

            //url = "ws" + (sslEnabled ? "s" : "") + "://" + IP + ":" + port + "/socket.io/?EIO=4&transport=websocket";
            url = "ws" + (sslEnabled ? "s" : "") + "://" + IP;
            if(portRequired) url += ":" + port;
            if (socketIORequired)
            {
                url += "/socket.io/";
                url += DefaultQueryString ? "?EIO=4&transport=websocket" : CustomisedQueryString;
            }
            Debug.Log(url);

            ws = new WebSocket(url);
            ws.OnOpen += OnOpen;
            ws.OnMessage += OnMessage;
            ws.OnError += OnError;
            ws.OnClose += OnClose;

            if (sslEnabled)
            {
                switch (sslProtocols)
                {
                    case FMSslProtocols.Default: ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Default; break;
                    case FMSslProtocols.None: ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.None; break;
                    case FMSslProtocols.Ssl2: ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Ssl2; break;
                    case FMSslProtocols.Ssl3: ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Ssl3; break;
                    case FMSslProtocols.Tls: ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls; break;
#if UNITY_2019_1_OR_NEWER
                    case FMSslProtocols.Tls11: ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls11; break;
                    case FMSslProtocols.Tls12: ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12; break;
#endif
                    default: ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Default; break;
                }
            }

            sid = null;
            wsConnected = false;
            connected = false;

            Initialised = true;
        }

        public void Update()
        {
            if (Initialised == false) return;

            if (FMSocketIOManager.instance != null)
            {
                DebugMode = FMSocketIOManager.instance.DebugMode;
                lock (RawMessageQueueLock)
                {
                    while (RawMessageQueue.Count > 0) FMSocketIOManager.instance.OnReceivedRawMessageEvent.Invoke(RawMessageQueue.Dequeue());
                }
            }

            lock (eventQueueLock)
            {
                while (eventQueue.Count > 0) EmitEvent(eventQueue.Dequeue());
            }

            lock (ackQueueLock)
            {
                while (ackQueue.Count > 0) InvokeAck(ackQueue.Dequeue());
            }

            if (wsConnected != IsWebSocketConnected(ws))
            {
                wsConnected = IsWebSocketConnected(ws);
                EmitEvent(wsConnected ? "connect" : "disconnect");
            }

            // GC expired acks
            if (ackList.Count == 0) return;
            if (DateTime.Now.Subtract(ackList[0].time).TotalSeconds < ackExpirationTime) return;
            ackList.RemoveAt(0);
        }

        public void OnDestroy()
        {
            if (socketThread != null) socketThread.Abort();
            if (pingThread != null) pingThread.Abort();
        }

        public void OnApplicationQuit()
        {
            Close();
        }

        #endregion

        #region Public Interface

        public void Connect()
        {
            connected = true;

            socketThread = new Thread(RunSocketThread);
            socketThread.Start(ws);

            pingThread = new Thread(RunPingThread);
            pingThread.Start(ws);
        }

        public void Close()
        {
            EmitClose();
            connected = false;
        }

        public void On(string ev, Action<SocketIOEvent> callback)
        {
            if (!handlers.ContainsKey(ev)) handlers[ev] = new List<Action<SocketIOEvent>>();
            handlers[ev].Add(callback);
        }

        public void Off(string ev, Action<SocketIOEvent> callback)
        {
            if (!handlers.ContainsKey(ev)) return;

            List<Action<SocketIOEvent>> l = handlers[ev];
            if (!l.Contains(callback)) return;

            l.Remove(callback);
            if (l.Count == 0) handlers.Remove(ev);
        }

        public void EmitRaw(string ev)
        {
            EmitMessage(-1, ev);
        }

        public void Emit(string ev)
        {
            EmitMessage(-1, string.Format("[\"{0}\"]", ev));
        }

        public void Emit(string ev, Action<string> action)
        {
            EmitMessage(++packetId, string.Format("[\"{0}\"]", ev));
            ackList.Add(new Ack(packetId, action));
        }

        public void Emit(string ev, string data)
        {
            EmitMessage(-1, string.Format("[\"{0}\",{1}]", ev, data));
        }

        public void Emit(string ev, string data, Action<string> action)
        {
            EmitMessage(++packetId, string.Format("[\"{0}\",{1}]", ev, data));
            ackList.Add(new Ack(packetId, action));
        }

        #endregion

        #region Private Methods

        private void RunSocketThread(object obj)
        {
            WebSocket webSocket = (WebSocket)obj;
            while (connected)
            {
                if (IsWebSocketConnected(webSocket))
                {
                    Thread.Sleep(reconnectDelay);
                }
                else
                {
                    try
                    {
                        webSocket.Connect();
                    }
                    catch(Exception e)
                    {
                        DebugLog("Connection Execption: " + e.Message);
                    }
                    //Thread.Sleep(5000);
                }
            }
            webSocket.Close();
        }

        private void RunPingThread(object obj)
        {
            WebSocket webSocket = (WebSocket)obj;

            int timeoutMilis = Mathf.FloorToInt(pingTimeout * 1000);
            int intervalMilis = Mathf.FloorToInt(pingInterval * 1000);

            DateTime pingStart;

            while (connected)
            {
                if (!wsConnected)
                {
                    Thread.Sleep(reconnectDelay);
                }
                else
                {
                    thPinging = true;
                    thPong = false;

                    EmitPacket(new Packet(EnginePacketType.PING));
                    pingStart = DateTime.Now;

                    while (IsWebSocketConnected(webSocket) && thPinging && (DateTime.Now.Subtract(pingStart).TotalSeconds < timeoutMilis)) Thread.Sleep(200);
                    if (!thPong) webSocket.Close();
                    Thread.Sleep(intervalMilis);
                }
            }
        }

        private void EmitMessage(int id, string raw)
        {
            EmitPacket(new Packet(EnginePacketType.MESSAGE, SocketPacketType.EVENT, 0, "/", id, raw));
        }

        private void EmitClose()
        {
            EmitPacket(new Packet(EnginePacketType.MESSAGE, SocketPacketType.DISCONNECT, 0, "/", -1, JsonUtility.ToJson("")));
            EmitPacket(new Packet(EnginePacketType.CLOSE));
        }

        private void EmitPacket(Packet packet)
        {
            try { ws.Send(encoder.Encode(packet)); } catch { }
        }

        private void OnOpen(object sender, EventArgs e)
        {
            EmitEvent("open");
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            Packet packet = decoder.Decode(e);
            switch (packet.enginePacketType)
            {
                case EnginePacketType.OPEN: HandleOpen(packet); break;
                case EnginePacketType.CLOSE: EmitEvent("close"); break;
                case EnginePacketType.PING: HandlePing(); break;
                case EnginePacketType.PONG: HandlePong(); break;
                case EnginePacketType.MESSAGE: HandleMessage(packet); break;
            }
            
        }

        private void HandleOpen(Packet packet)
        {
            sid = JsonUtility.FromJson<SocketOpenData>(packet.json).sid;
            EmitEvent("open");
            DebugLog("open: " + sid);
            FMSocketIOManager.instance.Settings.socketID = sid;
        }

        private void HandlePing()
        {
            EmitPacket(new Packet(EnginePacketType.PONG));
        }

        private void HandlePong()
        {
            thPong = true;
            thPinging = false;
        }

        private void HandleMessage(Packet packet)
        {
            if (packet.json == null) return;
            if (packet.socketPacketType == SocketPacketType.ACK)
            {
                for (int i = 0; i < ackList.Count; i++)
                {
                    if (ackList[i].packetId != packet.id) continue;
                    lock (ackQueueLock) ackQueue.Enqueue(packet);
                    return;
                }
                DebugLog("[SocketIO] Ack received for invalid Action: " + packet.id);
            }

            if (packet.socketPacketType == SocketPacketType.EVENT)
            {
                if (FMSocketIOManager.instance != null)
                {
                    lock (RawMessageQueueLock) RawMessageQueue.Enqueue(packet.json);
                }
                SocketIOEvent e = parser.Parse(packet.json);
                lock (eventQueueLock) eventQueue.Enqueue(e);
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            EmitEvent("error");
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            EmitEvent("close");
        }

        private void EmitEvent(string type)
        {
            EmitEvent(new SocketIOEvent(type));
        }

        private void EmitEvent(SocketIOEvent ev)
        {
            if (!handlers.ContainsKey(ev.name)) return;
            foreach (Action<SocketIOEvent> handler in handlers[ev.name])
            {
                try
                {
                    handler(ev);
                }
                catch (Exception ex)
                {
                    DebugLog(ex.ToString());
                }
            }
        }

        private void InvokeAck(Packet packet)
        {
            Ack ack;
            for (int i = 0; i < ackList.Count; i++)
            {
                if (ackList[i].packetId != packet.id) continue;
                ack = ackList[i];
                ackList.RemoveAt(i);
                ack.Invoke(packet.json);
                return;
            }
        }

        #endregion
    }
}
