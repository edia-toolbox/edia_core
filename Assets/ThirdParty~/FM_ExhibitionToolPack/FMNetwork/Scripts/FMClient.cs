using System.Collections;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;
using System.Collections.Generic;

public class FMClient : MonoBehaviour
{
    //[HideInInspector]
    public FMNetworkManager Manager;
    public static FMClient instance;
    private void Awake() { if (instance == null) instance = this; }

    [HideInInspector]
    public int ServerListenPort = 3333;
    [HideInInspector]
    public int ClientListenPort = 3334;

    //[HideInInspector]
    public string ServerIP = "0,0,0,0";
    [HideInInspector]
    public string ClientIP = "0,0,0,0";

    public bool IsConnected = false;
    public bool FoundServer = false;
    public bool AutoNetworkDiscovery = true;

    [HideInInspector]
    public int CurrentSeenTimeMS;
    public int LastSentTimeMS;
    public int LastReceivedTimeMS;


    [Header("[Experimental] suggested for mobile")]
    public bool UseMainThreadSender = false;


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

        //if (_appendQueueSendPacket.Count < 60)
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
        _meta[0] = 1;//raw byte

        if (_type == FMSendType.All) _meta[1] = 0;//all clients
        if (_type == FMSendType.Server) _meta[1] = 1;//all clients
        if (_type == FMSendType.Others) _meta[1] = 2;//skip sender

        byte[] _sendByte = new byte[_byteData.Length + _meta.Length];
        Buffer.BlockCopy(_meta, 0, _sendByte, 0, _meta.Length);
        Buffer.BlockCopy(_byteData, 0, _sendByte, 2, _byteData.Length);

        //if (_appendQueueSendPacket.Count < 60)
        {
            FMPacket _packet = new FMPacket();
            _packet.SendByte = _sendByte;
            _packet.SendType = _type;
            lock (_asyncLock) _appendQueueSendPacket.Enqueue(_packet);
        }
    }

    public void Action_AddPacket(byte[] _byteData, string _targetIP)
    {
        if (ServerIP == _targetIP)
        {
            Action_AddPacket(_byteData, FMSendType.Server);
            return;
        }
        //Send To Target IP
        byte[] _meta = new byte[2];
        _meta[0] = 0;//raw byte
        _meta[1] = 3;//target ip
        byte[] _ip = IPAddress.Parse(_targetIP).GetAddressBytes();
        byte[] _sendByte = new byte[_byteData.Length + _meta.Length + _ip.Length];
        Buffer.BlockCopy(_meta, 0, _sendByte, 0, _meta.Length);
        Buffer.BlockCopy(_ip, 0, _sendByte, 2, _ip.Length);
        Buffer.BlockCopy(_byteData, 0, _sendByte, 6, _byteData.Length);

        //if (_appendQueueSendPacket.Count < 60)
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
        if (ServerIP == _targetIP)
        {
            Action_AddPacket(_stringData, FMSendType.Server);
            return;
        }
        byte[] _byteData = Encoding.ASCII.GetBytes(_stringData);

        byte[] _meta = new byte[2];
        _meta[0] = 1;//raw byte
        _meta[1] = 3;//target ip
        byte[] _ip = IPAddress.Parse(_targetIP).GetAddressBytes();
        byte[] _sendByte = new byte[_byteData.Length + _meta.Length + _ip.Length];
        Buffer.BlockCopy(_meta, 0, _sendByte, 0, _meta.Length);
        Buffer.BlockCopy(_ip, 0, _sendByte, 2, _ip.Length);
        Buffer.BlockCopy(_byteData, 0, _sendByte, 6, _byteData.Length);

        //if (_appendQueueSendPacket.Count < 60)
        {
            FMPacket _packet = new FMPacket();
            _packet.SendByte = _sendByte;
            _packet.SendType = FMSendType.TargetIP;
            _packet.TargetIP = _targetIP;
            lock (_asyncLock) _appendQueueSendPacket.Enqueue(_packet);
        }
    }

    bool stop = false;
    // Start is called before the first frame update
    void Start()
    {
        StartAll();
    }
    private void Update()
    {
        if (CurrentSeenTimeMS < 0 && LastReceivedTimeMS > 0)
        {
            IsConnected = (Mathf.Abs(CurrentSeenTimeMS - int.MinValue) + (int.MaxValue - LastReceivedTimeMS) < 3000) ? true : false;
        }
        else
        {
            IsConnected = ((CurrentSeenTimeMS - LastReceivedTimeMS) < 3000) ? true : false;
        }
    }

    public void Action_StartClient()
    {
        StartCoroutine(NetworkClientStart());
    }

    UdpClient Client;
    UdpClient ClientListener;
    IPEndPoint ServerEp;
    IEnumerator NetworkClientStart()
    {
        LastSentTimeMS = Environment.TickCount;
        LastReceivedTimeMS = Environment.TickCount;

        stop = false;
        yield return new WaitForSeconds(0.5f);

        if (UseMainThreadSender)
        {
            StartCoroutine(MainThreadSender());
        }
        else
        {
            //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv Client Sender vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
            while (Loom.numThreads >= Loom.maxThreads) yield return null;
            Loom.RunAsync(() =>
            {
                //client request
                while (!stop)
                {
                    Sender();
                    System.Threading.Thread.Sleep(FoundServer ? 1 : 200);
                }
                System.Threading.Thread.Sleep(1);
            });
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Client Sender ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }

        //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv Client Receiver vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
        while (Loom.numThreads >= Loom.maxThreads) yield return null;
        Loom.RunAsync(() =>
        {
            while (!stop)
            {
                try
                {
                    if (ClientListener == null)
                    {
                        ClientListener = new UdpClient(ClientListenPort);
                        ClientListener.Client.ReceiveTimeout = 2000;
                        //ClientListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        ServerEp = new IPEndPoint(IPAddress.Any, ClientListenPort);
                    }

                    byte[] ReceivedData = ClientListener.Receive(ref ServerEp);
                    LastReceivedTimeMS = Environment.TickCount;

                    //=======================Decode Data=======================
                    if (!FoundServer)
                    {
                        ServerIP = ServerEp.Address.ToString();
                        FoundServer = true;
                    }

                    if (ReceivedData.Length > 2)
                    {
                        FMPacket _packet = new FMPacket();
                        _packet.SendByte = ReceivedData;

                        lock (_asyncLockReceived) _appendQueueReceivedPacket.Enqueue(_packet);
                    }
                    //=======================Decode Data=======================
                }
                catch
                {
                    //DebugLog("Client Receiver Timeout: " + socketException.ToString());
                    if (ClientListener != null) ClientListener.Close(); ClientListener = null;
                }
                //System.Threading.Thread.Sleep(1);
            }
            System.Threading.Thread.Sleep(1);
        });
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Client Receiver ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        while (!stop)
        {
            CurrentSeenTimeMS = Environment.TickCount;
            while (_appendQueueReceivedPacket.Count > 0)
            {
                ReceivedCount = _appendQueueReceivedPacket.Count;
                FMPacket _packet = new FMPacket();
                lock (_asyncLockReceived) _packet = _appendQueueReceivedPacket.Dequeue();

                if (Manager != null)
                {
                    byte[] ReceivedData = _packet.SendByte;
                    if (ReceivedData.Length > 2)
                    {
                        byte[] _meta = new byte[] { ReceivedData[0], ReceivedData[1] };
                        byte[] _data = new byte[ReceivedData.Length - 2];
                        Buffer.BlockCopy(ReceivedData, 2, _data, 0, _data.Length);

                        //process received data>> byte data: 0, string msg: 1, network object data: 2
                        switch (_meta[0])
                        {
                            case 0: Manager.OnReceivedByteDataEvent.Invoke(_data); break;
                            case 1: Manager.OnReceivedStringDataEvent.Invoke(Encoding.ASCII.GetString(_data)); break;
                            case 2: Manager.Action_SyncNetworkObjectTransform(_data); break;
                        }
                        Manager.GetRawReceivedData.Invoke(ReceivedData);
                    }
                }
            }
            yield return null;
        }
        yield break;
    }
    public int ReceivedCount = 0;

    IEnumerator MainThreadSender()
    {
        //client request
        while (!stop)
        {
            Sender();
            yield return null;
            //yield return new WaitForSeconds(FoundServer?0.001f:0.2f);
        }
    }

    void Sender()
    {
        try
        {
            if (Client == null)
            {
                Client = new UdpClient();
                Client.Client.SendTimeout = 200;
                Client.EnableBroadcast = true;
            }

            byte[] RequestData = new byte[1];
            if (FoundServer == false && AutoNetworkDiscovery)
            {
                if (CurrentSeenTimeMS - LastSentTimeMS > 2000)
                {
                    //broadcast
                    Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, ServerListenPort));
                    LastSentTimeMS = Environment.TickCount;
                }
            }
            else
            {
                //send to server ip only
                if (_appendQueueSendPacket.Count > 0)
                {
                    //limit 30 packet sent in each frame, solved overhead issue on receiver
                    int k = 0;
                    while (_appendQueueSendPacket.Count > 0 && k < 30)
                    {
                        k++;
                        FMPacket _packet = new FMPacket();
                        lock (_asyncLock) _packet = _appendQueueSendPacket.Dequeue();
                        Client.Send(_packet.SendByte, _packet.SendByte.Length, new IPEndPoint(IPAddress.Parse(ServerIP), ServerListenPort));
                        LastSentTimeMS = Environment.TickCount;
                    }
                }
                else
                {
                    if (CurrentSeenTimeMS - LastSentTimeMS > 2000)
                    {
                        //check connection: minimum 2000ms
                        Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Parse(ServerIP), ServerListenPort));
                        LastSentTimeMS = Environment.TickCount;
                    }
                }
            }
        }
        catch
        {
            //DebugLog("client sender timeout: " + socketException.ToString());
            if (Client != null) Client.Close(); Client = null;
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
        Action_StartClient();
    }
    void StopAll()
    {
        stop = true;
        IsConnected = false;
        FoundServer = false;

        if (Client != null)
        {
            try
            {
                Client.Close();
            }
            catch (Exception e)
            {
                DebugLog(e.Message);
            }
            Client = null;
        }

        StopCoroutine(NetworkClientStart());
    }
}
