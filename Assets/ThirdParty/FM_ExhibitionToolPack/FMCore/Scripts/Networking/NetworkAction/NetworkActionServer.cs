using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

public class NetworkActionServer : MonoBehaviour {

    public static NetworkActionServer instance;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    private object _asyncLock = new object();
    private Queue<byte[]> _appendQueueCmd = new Queue<byte[]>();

    private object _asyncLockReceived = new object();
    private Queue<byte[]> _appendQueueReceived = new Queue<byte[]>();
    public void Action_AddCmd(string _cmd)
    {
        if (!isConnected) return;
        float startTime = Time.realtimeSinceStartup;

        byte[] SendTypeByte = Encoding.ASCII.GetBytes("Norm");
        byte[] CmdByte = Encoding.ASCII.GetBytes(_cmd);
        byte[] SendByte = new byte[SendTypeByte.Length + CmdByte.Length];
        Buffer.BlockCopy(SendTypeByte, 0, SendByte, 0, SendTypeByte.Length);
        Buffer.BlockCopy(CmdByte, 0, SendByte, SendTypeByte.Length, CmdByte.Length);

        if (_appendQueueCmd.Count < 30)
        {
            lock (_asyncLock)
            {
                _appendQueueCmd.Enqueue(SendByte);
            }
        }
        //print((Time.realtimeSinceStartup - startTime) * 1000);
        //Debug.Log("covert time: " + System.DateTime.Now);
    }
    public void Action_AddCmd(byte[] _byteData)
    {
        if (!isConnected) return;

        byte[] SendTypeByte = Encoding.ASCII.GetBytes("Byte");

        byte[] SendByte = new byte[SendTypeByte.Length + _byteData.Length];
        Buffer.BlockCopy(SendTypeByte, 0, SendByte, 0, SendTypeByte.Length);
        Buffer.BlockCopy(_byteData, 0, SendByte, SendTypeByte.Length, _byteData.Length);

        if (_appendQueueCmd.Count < 1000)
        {
            lock (_asyncLock) _appendQueueCmd.Enqueue(SendByte);
        }
    }

    public bool enableLog = false;

    [Header("[Network Settings]")]
    public int ServerListenPort = 2389;
    int ClientListenPort = 2390;
    public bool isConnected = false;
    public int ConnectionCount = 0;

    public UnityEventInt GetServerListenPort_Event;

    private TcpListener listener;


    bool stop = false;
    private List<TcpClient> clients = new List<TcpClient>();
    private List<NetworkStream> streams = new List<NetworkStream>();

    //This must be the same with SEND_COUNT on the client
    const int SEND_RECEIVE_COUNT = 4;
    List<int> dataLostTimers = new List<int>();


    //float next = 0f;

    //=============================udpServer============================
    //int frequency = 0;//default is 0, fastest and less traffic

    UdpClient Server;
    IPEndPoint ClientEp;
    object _UdpLock = new object();
    Queue<string> commands = new Queue<string>();

    [Header("[Experimental] for supported devices only")]
    public bool UseAsyncUdpReceiver = false;


    public UnityEventString OnReceivedStringDataEvent;
    public UnityEventByteArray OnReceivedByteDataEvent;

    IEnumerator NetworkServerStart()
    {
        stop = false;
        yield return new WaitForSeconds(1f);

        if (UseAsyncUdpReceiver)
        {
            Server = new UdpClient(ServerListenPort);
            Server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Server.Client.ReceiveTimeout = 1000;
            if (!stop)
            {
                try
                {
                    Server.BeginReceive(new AsyncCallback(UdpReceiveCallback), null);
                }
                catch (SocketException socketException)
                {
                    Debug.Log("Socket exception: " + socketException);
                }
            }
        }
        else
        {
            while (Loom.numThreads >= Loom.maxThreads)
            {
                yield return null;
            }

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


                        byte[] ClientRequestData = Server.Receive(ref ClientEp);

                        //Server.Close();

                        lock (_asyncLockReceived) _appendQueueReceived.Enqueue(ClientRequestData);
                    }
                    catch (SocketException socketException)
                    {
                        Debug.Log("Socket exception: " + socketException);
                        if (Server != null) Server.Close(); Server = null;
                    }
                    //System.Threading.Thread.Sleep(1);
                    //System.Threading.Thread.Sleep(frequency);
                }
                System.Threading.Thread.Sleep(1);
            });
        }

        while (!stop)
        {
            while (_appendQueueReceived.Count > 0)
            {
                byte[] ClientRequestData = new byte[1];
                lock (_asyncLockReceived) ClientRequestData = _appendQueueReceived.Dequeue();

                byte[] _type = new byte[] { ClientRequestData[0] };
                byte[] _meta = new byte[] { ClientRequestData[1], ClientRequestData[2], ClientRequestData[3] };
                byte[] _data = new byte[ClientRequestData.Length - _type.Length - _meta.Length];
                Buffer.BlockCopy(ClientRequestData, 4, _data, 0, _data.Length);
                //Debug.Log("123");
                string ClientRequest = Encoding.ASCII.GetString(_meta);
                if (_type[0] == 0)
                {
                    //byte data
                    if (ClientRequest.Contains("Rpc"))
                    {
                        lock (_UdpLock) Action_AddCmd(_data);

                        OnReceivedByteDataEvent.Invoke(_data);
                    }
                }
                else
                {
                    //string data
                    if (ClientRequest.Contains("Rpc"))
                    {
                        string _string = Encoding.ASCII.GetString(_data);
                        string command = "Cmd" + _string;
                        lock (_UdpLock) commands.Enqueue(command);

                        OnReceivedStringDataEvent.Invoke(_string);
                    }
                }
            }
            yield return null;
        }
    }

    void UdpReceiveCallback(IAsyncResult ar)
    {
        if (ar.IsCompleted)
        {
            IPEndPoint ClientEp = new IPEndPoint(IPAddress.Any, ServerListenPort);
            byte[] ClientRequestData = Server.EndReceive(ar, ref ClientEp);

            //Process codes
            lock (_asyncLockReceived) _appendQueueReceived.Enqueue(ClientRequestData);
        }

        //waiting for another message
        if (!stop)
        {
            try
            {
                Server.BeginReceive(new AsyncCallback(UdpReceiveCallback), null);
            }
            catch (SocketException socketException)
            {
                Debug.Log("Socket exception: " + socketException);
            }
        }
    }

    void StopServerListener()
    {
        if (Server != null)
        {
            Server.Close();
        }
        StopCoroutine(NetworkServerStart());
    }
    //=============================udpServer============================

    private void Start()
    {
        Application.runInBackground = true;

        ClientListenPort = ServerListenPort + 1;

        StartCoroutine(NetworkServerStart());

        StartCoroutine(initServer());
    }

    private void Update()
    {
        GetServerListenPort_Event.Invoke(ServerListenPort);
        isConnected = ConnectionCount > 0 ? true : false;

        while (commands.Count > 0)
        {
            //process received UDP request
            lock (_UdpLock)
            {
                Action_AddCmd(commands.Dequeue());
            }
        }
    }

    bool CreatedServer = false;
    [Header ("[Experimental] Tested on Mobile")]
    public bool UseAsyncTcpListener = true;

    [Header("[Experimental] suggested for mobile")]
    public bool UseMainThreadSender = false;
    IEnumerator initServer()
    {
        if (!CreatedServer)
        {
            CreatedServer = true;

            //create listener
            listener = new TcpListener(IPAddress.Any, ClientListenPort);
            listener.Start();
            listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            if (UseAsyncTcpListener)
            {
                //Use Async Listener, replaced multi-threading solution before v1.06
                listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), listener);
            }
            else
            {
                //create LOOM thread, only create on first time, otherwise we will crash max thread limit
                // Wait for client to connect in another Thread 
                Loom.RunAsync(() => {
                    while (!stop)
                    {
                        // Wait for client connection
                        clients.Add(listener.AcceptTcpClient());
                        clients[clients.Count - 1].NoDelay = true;
                        // We are connected
                        isConnected = true;

                        streams.Add(clients[clients.Count - 1].GetStream());
                        streams[streams.Count - 1].WriteTimeout = 500;

                        dataLostTimers.Add(0);
                        Loom.QueueOnMainThread(() => {
                            isConnected = true;
                        });
                        System.Threading.Thread.Sleep(1);
                    }
                });
            }

            //Start sending coroutine
            StartCoroutine(senderCOR());
        }
        yield return null;
    }

    void AcceptCallback(IAsyncResult ar)
    {
        // Get the listener that handles the client request.
        TcpListener _listener = (TcpListener)ar.AsyncState;
        TcpClient client = _listener.EndAcceptTcpClient(ar);
        client.NoDelay = true;

        isConnected = true;
        clients.Add(client);
        streams.Add(client.GetStream());
        dataLostTimers.Add(0);

        ConnectionCount = clients.Count;

        if (!stop)
        {
            listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), listener);
        }
    }

    IEnumerator senderCOR()
    {
        //Wait until client has connected
        while (!isConnected) yield return null;
        LOG("Connected!");

        bool readyToGetFrame = true;
        while (!stop)
        {
            if (clients.Count > 0)
            {
                readyToGetFrame = false;

                if (UseMainThreadSender)
                {
                    Sender();
                    readyToGetFrame = true;
                }
                else
                {
                    while (Loom.numThreads >= Loom.maxThreads) yield return null;
                    Loom.RunAsync(() =>
                    {
                        Sender();
                        readyToGetFrame = true;
                        //System.Threading.Thread.Sleep(1);
                    });
                }
            }

            float startCheckTime = Time.realtimeSinceStartup;
            //Wait until ready to get new frame
            while (!readyToGetFrame)
            {
                LOG("Waiting To get new frame");
                //System.Threading.Thread.Sleep(1);

                if ((Time.realtimeSinceStartup - startCheckTime) > 2f)
                {
                    for (int i = 0; i < clients.Count; i++)
                    {
                        if (!clients[i].Connected) RemoveClientConnection(i);
                    }
                    readyToGetFrame = true;
                }
                ConnectionCount = clients.Count;
                yield return null;
            }
            yield return null;
        }
        
    }

    void Sender()
    {
        byte[] dataBytesLength = new byte[SEND_RECEIVE_COUNT];
        byte[] dataBytes = new byte[1];
        //=================check: connection================
        for (int i = 0; i < clients.Count; i++)
        {
            try
            {
                if (clients[i].Available <= 0)
                {
                    dataLostTimers[i]++;
                    if (dataLostTimers[i] > 0)
                    {
                        RemoveClientConnection(i);
                        ConnectionCount = clients.Count;
                    }
                }
                else
                {
                    //received some data
                    dataLostTimers[i] = 0;
                }
                if (streams[i].CanWrite) streams[i].WriteTimeout = 200;
            }
            catch (SocketException socketException)
            {
                LOG("Socket exception: " + i + " >> " + socketException);
                RemoveClientConnection(i);
                ConnectionCount = clients.Count;
            }
        }
        //=================check: connection================

        if (_appendQueueCmd.Count > 0)
        {
            //there are some commands in queue
            while (_appendQueueCmd.Count > 0)
            {
                lock (_asyncLock) dataBytes = _appendQueueCmd.Dequeue();
                byteLengthToDataByteArray(dataBytes.Length, dataBytesLength);

                for (int i = 0; i < clients.Count; i++)
                {
                    StreamWrite(streams[i], dataBytes, dataBytesLength);
                }
                //System.Threading.Thread.Sleep(1);
            }
        }
        else
        {
            //there is no command, just sent one byte for data check
            dataBytes = new byte[1];
            byteLengthToDataByteArray(dataBytes.Length, dataBytesLength);

            for (int i = 0; i < clients.Count; i++)
            {
                StreamWrite(streams[i], dataBytes, dataBytesLength);
            }
        }

        ConnectionCount = clients.Count;
    }

    void byteLengthToDataByteArray(int byteLength, byte[] fullBytes)
    {
        //Clear old data
        Array.Clear(fullBytes, 0, fullBytes.Length);
        //Convert int to bytes
        byte[] bytesToSendCount = BitConverter.GetBytes(byteLength);
        //Copy result to fullBytes
        bytesToSendCount.CopyTo(fullBytes, 0);
    }

    void StreamWrite(NetworkStream _stream, byte[] _dataBytes, byte[] _dataBytesLength)
    {
        if (_stream.CanWrite)
        {
            try
            {
                _stream.Write(_dataBytesLength, 0, _dataBytesLength.Length);
            }
            catch (SocketException socketException)
            {
                LOG("Socket exception: " + socketException);
            }

            try
            {
                //Send the image bytes
                _stream.Write(_dataBytes, 0, _dataBytes.Length);

            }
            catch (SocketException socketException)
            {
                LOG("Socket exception: " + socketException);
            }
            _stream.Flush();
        }
        else
        {
            _stream.Flush();
            LOG("can't write?");
        }
    }
    void RemoveClientConnection(int _id)
    {
        try
        {
            streams[_id].Close();
            //streams[_id].Dispose();
            clients[_id].Close();
        }
        catch (SocketException socketException)
        {
            LOG("Socket exception: " + socketException);
        }

        streams.Remove(streams[_id]);
        clients.Remove(clients[_id]);
        dataLostTimers.Remove(dataLostTimers[_id]);
    }

    void LOG(string messsage)
    {
        if (enableLog)
        {
            Debug.Log(messsage);
        }
    }

    void StopAll()
    {
        stop = true;
        if (listener != null) listener.Stop();

        for (int i = 0; i < clients.Count; i++) RemoveClientConnection(i);

        StopCoroutine(senderCOR());
        StopServerListener();
        CreatedServer = false;
    }
    private void OnApplicationQuit()
    {
        StopAll();
    }


    void OnDisable()
    {
        StopAll();
    }

    private void OnDestroy()
    {
        StopAll();
    }

    void OnEnable()
    {
        if (Time.realtimeSinceStartup <= 3f || CreatedServer) return;

        stop = false;
        isConnected = false;

        if (clients != null) clients.Clear();

        StartCoroutine(NetworkServerStart());
        StartCoroutine(initServer());
    }
}
