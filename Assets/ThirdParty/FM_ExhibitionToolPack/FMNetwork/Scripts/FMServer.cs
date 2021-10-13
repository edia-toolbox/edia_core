using System.Collections;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;
using System.Collections.Generic;

public class FMServer : MonoBehaviour
{
    [HideInInspector]
    public FMNetworkManager Manager;
    public static FMServer instance;
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    [HideInInspector]
    public int ServerListenPort = 3333;
    [HideInInspector]
    public int ClientListenPort = 3334;

    [Serializable]
    public class ConnectedClient
    {
        public string IP;
        public int Port;
        public int LastSeenTimeMS;
        public int LastSentTimeMS;

        public UdpClient Client;
        public IPEndPoint ClientEp;

        public void Send(byte[] _byte)
        {
            try
            {
                if (Client == null)
                {
                    Client = new UdpClient(IP, Port);
                    Client.Client.SendTimeout = 500;
                }
                Client.Send(_byte, _byte.Length);
                LastSentTimeMS = Environment.TickCount;
            }
            catch
            {
                //Debug.Log("clients: " + _byte.Length+ " "+ e);
                Close();
            }
        }
        public void Close()
        {
            if (Client != null) Client.Close(); Client = null;
        }
    }

    public void MulticastChecker()
    {
        UdpClient MulticastClient = new UdpClient();
        try
        {
            MulticastClient.Client.SendTimeout = 200;
            MulticastClient.EnableBroadcast = true;

            byte[] _byte = new byte[1];
            MulticastClient.Send(_byte, _byte.Length, new IPEndPoint(IPAddress.Broadcast, ClientListenPort));

            if (MulticastClient != null) MulticastClient.Close();
        }
        catch
        {
            if (MulticastClient != null) MulticastClient.Close();
        }
    }

    public bool IsConnected = false;
    public int ConnectionCount = 0;
    public List<ConnectedClient> ConnectedClients = new List<ConnectedClient>();
    public void Action_CheckClientStatus(string _ip)
    {
        bool IsExistedClient = false;
        for (int i = 0; i < ConnectedClients.Count; i++)
        {
            if (_ip == ConnectedClients[i].IP)
            {
                IsExistedClient = true;
                ConnectedClients[i].IP = _ip;
                ConnectedClients[i].LastSeenTimeMS = Environment.TickCount;
            }
        }

        if (!IsExistedClient)
        {
            //register new client
            ConnectedClient NewClient = new ConnectedClient();
            NewClient.IP = _ip;
            NewClient.Port = ClientListenPort;
            NewClient.LastSeenTimeMS = Environment.TickCount;

            NewClient.Send(new byte[1]);

            ConnectedClients.Add(NewClient);
        }
    }

    [HideInInspector]
    public int CurrentSeenTimeMS;

    private Queue<FMPacket> _appendQueueReceivedPacket = new Queue<FMPacket>();
    private object _asyncLockReceived = new object();

    private Queue<FMPacket> _appendQueueSendPacket = new Queue<FMPacket>();
    private object _asyncLock = new object();
    public void Action_AddPacket(byte[] _byteData, FMSendType _type)
    {
        byte[] _meta = new byte[2];
        _meta[0] = 0;//raw byte

        if (_type == FMSendType.All) _meta[1] = 0;//all clients
        if (_type == FMSendType.Server) _meta[1] = 1;//all clients
        if (_type == FMSendType.Others) _meta[1] = 2;//skip sender

        byte[] _sendByte = new byte[_byteData.Length + _meta.Length];
        Buffer.BlockCopy(_meta, 0, _sendByte, 0, _meta.Length);
        Buffer.BlockCopy(_byteData, 0, _sendByte, 2, _byteData.Length);

        //if (_appendQueueSendPacket.Count < 120)
        {
            FMPacket _packet = new FMPacket();
            _packet.SendByte = _sendByte;
            _packet.SendType = _type;
            lock (_asyncLock) _appendQueueSendPacket.Enqueue(_packet);
        }
    }
    public void Action_AddPacket(string _stringData, FMSendType _type)
    {
        byte[] _byteData = Encoding.ASCII.GetBytes(_stringData);

        byte[] _meta = new byte[2];
        _meta[0] = 1;//string data

        if (_type == FMSendType.All) _meta[1] = 0;//all clients
        if (_type == FMSendType.Server) _meta[1] = 1;//all clients
        if (_type == FMSendType.Others) _meta[1] = 2;//skip sender

        byte[] _sendByte = new byte[_byteData.Length + _meta.Length];
        Buffer.BlockCopy(_meta, 0, _sendByte, 0, _meta.Length);
        Buffer.BlockCopy(_byteData, 0, _sendByte, 2, _byteData.Length);

        //if (_appendQueueSendPacket.Count < 120)
        {
            FMPacket _packet = new FMPacket();
            _packet.SendByte = _sendByte;
            _packet.SendType = _type;
            lock (_asyncLock) _appendQueueSendPacket.Enqueue(_packet);
        }
    }

    public void Action_AddPacket(byte[] _byteData, string _targetIP)
    {
        byte[] _meta = new byte[2];
        _meta[0] = 0;//raw byte
        _meta[1] = 3;//target ip

        byte[] _sendByte = new byte[_byteData.Length + _meta.Length];
        Buffer.BlockCopy(_meta, 0, _sendByte, 0, _meta.Length);
        Buffer.BlockCopy(_byteData, 0, _sendByte, 2, _byteData.Length);

        //if (_appendQueueSendPacket.Count < 120)
        {
            FMPacket _packet = new FMPacket();
            _packet.SendByte = _sendByte;
            _packet.SendType = FMSendType.TargetIP;
            _packet.TargetIP = _targetIP;
            lock (_asyncLock) _appendQueueSendPacket.Enqueue(_packet);
        }
    }
    public void Action_AddPacket(string _stringData, string _targetIP)
    {
        byte[] _byteData = Encoding.ASCII.GetBytes(_stringData);

        byte[] _meta = new byte[2];
        _meta[0] = 1;//string data
        _meta[1] = 3;//target ip

        byte[] _sendByte = new byte[_byteData.Length + _meta.Length];
        Buffer.BlockCopy(_meta, 0, _sendByte, 0, _meta.Length);
        Buffer.BlockCopy(_byteData, 0, _sendByte, 2, _byteData.Length);

        //if (_appendQueueSendPacket.Count < 120)
        {
            FMPacket _packet = new FMPacket();
            _packet.SendByte = _sendByte;
            _packet.SendType = FMSendType.TargetIP;
            _packet.TargetIP = _targetIP;
            lock (_asyncLock) _appendQueueSendPacket.Enqueue(_packet);
        }
    }

    public void Action_AddPacket(FMPacket _packet)
    {
        //if (_appendQueueSendPacket.Count < 120)
        {
            lock (_asyncLock) _appendQueueSendPacket.Enqueue(_packet);
        }
    }

    public void Action_AddNetworkObjectPacket(byte[] _byteData, FMSendType _type)
    {
        byte[] _meta = new byte[2];
        _meta[0] = 2;//network object packet

        if (_type == FMSendType.All) _meta[1] = 0;//all clients
        if (_type == FMSendType.Server) _meta[1] = 1;//all clients
        if (_type == FMSendType.Others) _meta[1] = 2;//skip sender

        byte[] _sendByte = new byte[_byteData.Length + _meta.Length];
        Buffer.BlockCopy(_meta, 0, _sendByte, 0, _meta.Length);
        Buffer.BlockCopy(_byteData, 0, _sendByte, 2, _byteData.Length);

        if (_appendQueueSendPacket.Count < 120)
        {
            FMPacket _packet = new FMPacket();
            _packet.SendByte = _sendByte;
            _packet.SendType = _type;
            lock (_asyncLock) _appendQueueSendPacket.Enqueue(_packet);
        }
    }

    bool stop = false;
    // Start is called before the first frame update
    void Start()
    {
        StartAll();
    }

    public int CmdLength;
    private void Update()
    {
        IsConnected = (ConnectionCount > 0) ? true : false;
        CmdLength = _appendQueueSendPacket.Count;
    }

    public void Action_StartServer()
    {
        StartCoroutine(NetworkServerStart());
    }

    [Header("[Experimental] for supported devices only")]
    public bool UseAsyncListener = false;
    [Header("[Experimental] suggested for mobile")]
    public bool UseMainThreadSender = false;
    UdpClient Server;
    IPEndPoint ClientEp;
    IEnumerator NetworkServerStart()
    {
        stop = false;
        yield return new WaitForSeconds(0.5f);

        MulticastChecker();
        yield return new WaitForSeconds(0.5f);

        if (!UseAsyncListener)
        {
            //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv Server Receiver vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
            while (Loom.numThreads >= Loom.maxThreads) yield return null;
            Loom.RunAsync(() =>
            {
                while (!stop)
                {
                    try
                    {
                        if (Server == null)
                        {
                            Server = new UdpClient(ServerListenPort);
                            Server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                            Server.Client.EnableBroadcast = true;
                            ClientEp = new IPEndPoint(IPAddress.Any, ServerListenPort);
                        }

                        //=======================Queue Received Data=======================
                        byte[] ReceivedData = Server.Receive(ref ClientEp);
                        string ClientIP = ClientEp.Address.ToString();
                        if (ReceivedData.Length > 2)
                        {
                            FMPacket _packet = new FMPacket();
                            _packet.SendByte = ReceivedData;
                            if (ReceivedData[1] == 2)
                            {
                                //others, skip sender
                                _packet.SkipIP = ClientIP;
                            }

                            lock (_asyncLockReceived) _appendQueueReceivedPacket.Enqueue(_packet);
                        }
                        //=======================Queue Received Data=======================

                        //=======================Check is new client?=======================
                        Action_CheckClientStatus(ClientIP);
                        //=======================Check is new client?=======================

                    }
                    catch
                    {
                        //DebugLog("Server Socket exception: " + socketException);
                        if (Server != null) Server.Close(); Server = null;
                    }
                    //System.Threading.Thread.Sleep(1);
                }
            });
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Server Receiver ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }
        else
        {
            //Use Async Receiver solution
            //Experimental feature: try to reduce thread usage
            //Didn't work well on low-end devices during our testing
            if (Server == null)
            {
                Server = new UdpClient(ServerListenPort);
                Server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                ClientEp = new IPEndPoint(IPAddress.Any, ServerListenPort);

                Server.Client.ReceiveTimeout = 1000;
                Server.Client.SendTimeout = 200;
            }

            if (!stop)
            {
                try
                {
                    Server.BeginReceive(new AsyncCallback(UdpReceiveCallback), null);
                }
                catch
                {
                    //DebugLog("Socket exception: " + socketException);
                    if (Server != null) Server.Close(); Server = null;
                }
            }
        }

        if (!UseMainThreadSender)
        {
            //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv Server Sender vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
            while (Loom.numThreads >= Loom.maxThreads) yield return null;
            Loom.RunAsync(() =>
            {
                while (!stop)
                {
                    Sender();
                    System.Threading.Thread.Sleep(1);
                }
                System.Threading.Thread.Sleep(1);
            });
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Server Sender ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }
        else
        {
            StartCoroutine(MainThreadSender());
        }

        //processing
        while (!stop)
        {
            CurrentSeenTimeMS = Environment.TickCount;
            while (_appendQueueReceivedPacket.Count > 0)
            {
                FMPacket _packet = new FMPacket();
                lock (_asyncLockReceived) _packet = _appendQueueReceivedPacket.Dequeue();

                if (Manager != null)
                {
                    byte[] ReceivedData = _packet.SendByte;
                    if (ReceivedData.Length > 2)
                    {
                        byte[] _meta = new byte[] { ReceivedData[0], ReceivedData[1] };
                        if (_meta[1] == 3)
                        {
                            //Send to TargetIP, contains 4 bytes ip after meta data
                            _packet.TargetIP = new IPAddress(new byte[] { ReceivedData[2], ReceivedData[3], ReceivedData[4], ReceivedData[5] }).ToString();
                            byte[] _data = new byte[ReceivedData.Length - 6];
                            Buffer.BlockCopy(ReceivedData, 6, _data, 0, _data.Length);

                            if (_packet.TargetIP == Manager.ReadLocalIPAddress)
                            {
                                //process received data>> byte data: 0, string msg: 1
                                switch (_meta[0])
                                {
                                    case 0: Manager.OnReceivedByteDataEvent.Invoke(_data); break;
                                    case 1: Manager.OnReceivedStringDataEvent.Invoke(Encoding.ASCII.GetString(_data)); break;
                                }
                            }
                            else
                            {
                                //redirect the data to target IP
                                if (_packet.TargetIP != Manager.ReadLocalIPAddress)
                                {
                                    _packet.SendType = FMSendType.TargetIP;
                                    _packet.SendByte = new byte[_meta.Length + _data.Length];
                                    _packet.SendByte[0] = _meta[0]; _packet.SendByte[1] = _meta[1];
                                    Buffer.BlockCopy(_data, 0, _packet.SendByte, 2, _data.Length);
                                    Action_AddPacket(_packet);
                                }
                            }
                        }
                        else
                        {
                            byte[] _data = new byte[ReceivedData.Length - 2];
                            Buffer.BlockCopy(ReceivedData, 2, _data, 0, _data.Length);

                            //process received data>> byte data: 0, string msg: 1
                            switch (_meta[0])
                            {
                                case 0: Manager.OnReceivedByteDataEvent.Invoke(_data); break;
                                case 1: Manager.OnReceivedStringDataEvent.Invoke(Encoding.ASCII.GetString(_data)); break;
                            }

                            //check send type
                            switch (_meta[1])
                            {
                                //send to all, redirect msg to all other clients
                                case 0: Action_AddPacket(_packet); break;
                                //send to server only, do not need to do anything
                                case 1: break;
                                //skip sender
                                case 2: Action_AddPacket(_packet); break;
                            }
                        }
                        Manager.GetRawReceivedData.Invoke(ReceivedData);
                    }
                }
            }
            yield return null;
        }
        yield break;
    }

    void UdpReceiveCallback(IAsyncResult ar)
    {
        if (ar.IsCompleted)
        {
            //receive callback completed
            //=======================Queue Received Data=======================
            byte[] ReceivedData = Server.EndReceive(ar, ref ClientEp);
            string ClientIP = ClientEp.Address.ToString();
            if (ReceivedData.Length > 2)
            {
                FMPacket _packet = new FMPacket();
                _packet.SendByte = ReceivedData;

                //others, skip sender
                if (ReceivedData[1] == 2) _packet.SkipIP = ClientIP;
                lock (_asyncLockReceived) _appendQueueReceivedPacket.Enqueue(_packet);
            }
            //=======================Queue Received Data=======================

            //=======================check is new client?=======================
            Action_CheckClientStatus(ClientIP);
            //=======================check is new client?=======================
        }

        if (!stop)
        {
            try
            {
                Server.BeginReceive(new AsyncCallback(UdpReceiveCallback), null);
            }
            catch
            {
                //DebugLog("sth wrong with server receive async: " + socketException.ToString());
                if (Server != null) Server.Close(); Server = null;
            }
        }
    }

    IEnumerator MainThreadSender()
    {
        while (!stop)
        {
            Sender();
            yield return null;
            //yield return new WaitForSeconds(0.001f);
        }
    }

    void Sender()
    {
        for (int i = 0; i < ConnectedClients.Count; i++)
        {
            //if (CurrentSeenTimeMS - ConnectedClients[i].LastSeenTimeMS > 3000)
            //{
            //    //remove it if didn't receive any data from client for 3000 ms
            //    ConnectedClients[i].Close();
            //    ConnectedClients.Remove(ConnectedClients[i]);
            //}

            bool _active = false;
            if (CurrentSeenTimeMS < 0 && ConnectedClients[i].LastSeenTimeMS > 0)
            {
                _active = (Mathf.Abs(CurrentSeenTimeMS - int.MinValue) + (int.MaxValue - ConnectedClients[i].LastSeenTimeMS) < 3000) ? true : false;
            }
            else
            {
                _active = ((CurrentSeenTimeMS - ConnectedClients[i].LastSeenTimeMS) < 3000) ? true : false;
            }
            if (_active == false)
            {
                //remove it if didn't receive any data from client for 3000 ms
                ConnectedClients[i].Close();
                ConnectedClients.Remove(ConnectedClients[i]);
            }
        }
        ConnectionCount = ConnectedClients.Count;

        if (_appendQueueSendPacket.Count > 0)
        {
            //limit 30 packet sent in each frame, solved overhead issue on receiver
            int k = 0;
            //there are some commands in queue
            while (_appendQueueSendPacket.Count > 0 && k < 30)
            {
                k++;
                FMPacket _packet = new FMPacket();
                lock (_asyncLock) _packet = _appendQueueSendPacket.Dequeue();

                if (_packet.SendType != FMSendType.TargetIP)
                {
                    for (int i = 0; i < ConnectedClients.Count; i++)
                    {
                        if (ConnectedClients[i].IP != _packet.SkipIP) ConnectedClients[i].Send(_packet.SendByte);
                    }
                }
                else
                {
                    //sending to target ip only
                    for (int i = 0; i < ConnectedClients.Count; i++)
                    {
                        if (ConnectedClients[i].IP == _packet.TargetIP)
                        {
                            ConnectedClients[i].Send(_packet.SendByte);
                        }
                        else
                        {
                            if (CurrentSeenTimeMS - ConnectedClients[i].LastSentTimeMS > 1000) ConnectedClients[i].Send(new byte[1]);
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < ConnectedClients.Count; i++)
            {
                //send empty byte for checking, after 1000 ms
                if (CurrentSeenTimeMS - ConnectedClients[i].LastSentTimeMS > 1000) ConnectedClients[i].Send(new byte[1]);
            }
        }
    }

    public bool ShowLog = true;
    public void DebugLog(string _value) { if (ShowLog) Debug.Log(_value); }

    private void OnApplicationQuit() { StopAll(); }
    private void OnDisable() { StopAll(); }
    private void OnDestroy() { StopAll(); }
    private void OnEnable()
    {
        if (Time.timeSinceLevelLoad <= 3f) return;
        if (stop) StartAll();
    }

    void StartAll()
    {
        stop = false;
        Action_StartServer();
    }
    void StopAll()
    {
        stop = true;
        IsConnected = false;
        if (Server != null)
        {
            try
            {
                Server.Close();
            }
            catch (Exception e)
            {
                DebugLog(e.Message);
            }
            Server = null;
        }
        if (ConnectedClients.Count > 0)
        {
            foreach (ConnectedClient cc in ConnectedClients) cc.Close();
        }
        StopCoroutine(NetworkServerStart());
    }
}
