using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

using System.Text;

public class NetworkActionClient : MonoBehaviour
{
    public static NetworkActionClient instance;
    private void Awake()
    {
        if (instance == null)instance = this;
    }
    public bool enableLog = false;
    public string IP = "192.168.1.165";
    public void Action_SetIP(string _value)
    {
        IP = _value;
    }
    public int ServerListenPort = 2389;
    int ClientListenPort = 2390;
    public void Action_SetServerListenPort(int _port)
    {
        ServerListenPort = _port;
        ClientListenPort = _port + 1;
    }

    TcpClient client;
    NetworkStream stream;
    bool stop = false;

    //This must be the-same with SEND_COUNT on the server
    const int SEND_RECEIVE_COUNT = 4;
    int dataSize = 0;
    byte[] dataBytesCount;

    //float lastSeenTime = 0f;
    //[HideInInspector]
    public int CurrentSeenTimeMS;
    public int LastReceivedTimeMS;
    public bool isConnected = false;
    public int TimeoutThresholdMS = 5000;
    int GetCurrentMS() { return Environment.TickCount; }

    public bool autoReconnect = true;
    //float timeoutThreshold = 5f;

    bool isReachedServer = false;
    int checkTimer = 0;


    private object _asyncLockReceived = new object();
    private Queue<byte[]> _appendQueueReceived = new Queue<byte[]>();

    public UnityEvent OnConnectedEvent;
    public UnityEvent OnDisconnectedEvent;

    public UnityEventString OnReceivedStringDataEvent;
    public UnityEventByteArray OnReceivedByteDataEvent;
    public void Action_ReceivedDataDebug(string _value)
    {
        Debug.Log(_value);
    }

    //=========================Client Send================================
    UdpClient udpClient;
    //private Queue<string> _appendQueueCmd = new Queue<string>();
    private Queue<byte[]> _appendQueueCmd = new Queue<byte[]>();

    public void Action_RpcSend(string _cmd)
    {
        if (!isConnected) return;
        //limit the queue length
        if (_appendQueueCmd.Count > 100) return;
        //add cmd to queue

        byte[] _type = new byte[] { 1 };
        byte[] _meta = Encoding.ASCII.GetBytes("Rpc");
        byte[] _data = Encoding.ASCII.GetBytes(_cmd);
        byte[] _byte = new byte[_type.Length + _meta.Length + _data.Length];

        int _offset = 0;
        Buffer.BlockCopy(_type, 0, _byte, 0, _type.Length);
        _offset += _type.Length;
        Buffer.BlockCopy(_meta, 0, _byte, _offset, _meta.Length);
        _offset += _meta.Length;
        Buffer.BlockCopy(_data, 0, _byte, _offset, _data.Length);

        lock (_asyncSend) _appendQueueCmd.Enqueue(_byte);
        //check cmd list
        //Action_CheckCmdList();
    }
    public void Action_RpcSend(byte[] _cmd)
    {
        if (!isConnected) return;
        //limit the queue length
        if (_appendQueueCmd.Count > 1000) return;
        //add cmd to queue
        byte[] _type = new byte[] { 0 };
        byte[] _meta = Encoding.ASCII.GetBytes("Rpc");
        byte[] _data = _cmd;
        byte[] _byte = new byte[_type.Length + _meta.Length + _data.Length];

        int _offset = 0;
        Buffer.BlockCopy(_type, 0, _byte, 0, _type.Length);
        _offset += _type.Length;
        Buffer.BlockCopy(_meta, 0, _byte, _offset, _meta.Length);
        _offset += _meta.Length;
        Buffer.BlockCopy(_cmd, 0, _byte, _offset, _data.Length);

        lock (_asyncSend) _appendQueueCmd.Enqueue(_byte);

        //check cmd list
        //Action_CheckCmdList();
    }

    [HideInInspector]
    public bool readySend = true;
    object _asyncSend = new object();

    void Start()
    {
        Application.runInBackground = true;
        ClientListenPort = ServerListenPort + 1;
        LastReceivedTimeMS = int.MinValue;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isConnected)
        {
            OnDisconnectedEvent.Invoke();
        }
        else
        {
            OnConnectedEvent.Invoke();
        }

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        CheckPause ();
#endif

        //checkTimer = Time.realtimeSinceStartup - lastSeenTime;
        CurrentSeenTimeMS = GetCurrentMS();
        //checkTimer = CurrentSeenTimeMS - LastReceivedTimeMS;

        if (CurrentSeenTimeMS < 0 && LastReceivedTimeMS > 0)
        {
            //cycle
            checkTimer = Mathf.Abs(CurrentSeenTimeMS - int.MinValue) + Mathf.Abs(int.MaxValue - LastReceivedTimeMS);
        }
        else
        {
            checkTimer = Mathf.Abs(CurrentSeenTimeMS - LastReceivedTimeMS);
        }

        if (checkTimer > TimeoutThresholdMS)
        {
            isConnected = false;
            if (autoReconnect)
            {
                if (triggerQuitApp == false)
                {
                    triggerQuitApp = true;
                    StartCoroutine(RelaunchApp());
                }
            }
        }

        if (CurrentSeenTimeMS > LastReceivedTimeMS)
        {
            //int _min = int.MinValue + 5;
            //int _re = _min;
            //Debug.Log(">>>" + _re + " : " + _min);
        }
    }

    //Converts the byte array to the data size and returns the result
    int DataByteArrayToByteLength(byte[] dataBytesLength)
    {
        int byteLength = BitConverter.ToInt32(dataBytesLength, 0);
        return byteLength;
    }

    /////////////////////////////////////////////////////Read Data SIZE from Server///////////////////////////////////////////////////
    private int ReadDataByteSize(int size)
    {
        bool disconnected = false;
        if (dataBytesCount == null)
        {
            dataBytesCount = new byte[size];
        }
        else
        {
            if (dataBytesCount.Length > size) dataBytesCount = new byte[size];
        }

        int total = 0;
        do
        {
            int read = stream.Read(dataBytesCount, total, size - total);
            if (read == 0)
            {
                disconnected = true;
                break;
            }
            total += read;
        } while (total != size);

        int byteLength = -1;
        if (!disconnected) byteLength = DataByteArrayToByteLength(dataBytesCount);

        return byteLength;
    }

    //read data byte data from server
    private void ReadDataByteArray(int size)
    {
        bool disconnected = false;
        byte[] dataBytes = new byte[size];

        int total = 0;
        do
        {
            var read = stream.Read(dataBytes, total, size - total);
            //Debug.LogFormat("Client recieved {0} bytes", total);
            if (read == 0)
            {
                disconnected = true;
                break;
            }
            total += read;
        } while (total != size);

        bool readyToReadAgain = false;
        if (!disconnected)
        {
            lock (_asyncLockReceived) _appendQueueReceived.Enqueue(dataBytes);
            readyToReadAgain = true;
        }

        //Wait until old Data is processed
        while (!readyToReadAgain)
        {
            System.Threading.Thread.Sleep(1);
        }
    }

    void ProcessReceivedData(byte[] receivedDataBytes)
    {
        if (receivedDataBytes.Length < 4) return;
        if (EnergySavingManager.instance != null)
        {
            //save energy, not process
            if (EnergySavingManager.instance.isSleeping) return;
        }

        byte[] DataTypeByte = new byte[4];
        Buffer.BlockCopy(receivedDataBytes, 0, DataTypeByte, 0, DataTypeByte.Length);
        string DataType = Encoding.ASCII.GetString(DataTypeByte);

        //Debug.Log("client type: " + DataType);

        byte[] DataByte = new byte[receivedDataBytes.Length - DataTypeByte.Length];
        Buffer.BlockCopy(receivedDataBytes, DataTypeByte.Length, DataByte, 0, DataByte.Length);

        if (DataType == "Norm")
        {
            string DataMessage = Encoding.ASCII.GetString(DataByte);
            OnReceivedStringDataEvent.Invoke(DataMessage);
            //Debug.Log("client received: " + DataMessage);
        }
        if(DataType == "Byte")
        {
            //Debug.Log("client received byte: " + BitConverter.ToSingle(DataByte, 0 * sizeof(float)));
            OnReceivedByteDataEvent.Invoke(DataByte);
        }

    }


    bool triggerQuitApp = false;
    IEnumerator RelaunchApp()
    {
        LOGWARNING("trigger reconnection");
        stop = true;
        while (Loom.numThreads >= Loom.maxThreads) yield return null;

        Action_ConnectServer();
        yield return new WaitForSeconds(2);

        if (isReachedServer)
        {
            LOGWARNING("reached server, wait longer");
            yield return new WaitForSeconds(3);
        }

        triggerQuitApp = false;
        //notes:
        //stop previous reciever
        //if "reached server", give more time to check if we can receive data from server. then finished the task.
        //isConnected will be true if we received data recently. if not, we will trigger the re-connection task again.
    }

    public void Action_Disconnect()
    {
        stop = true;
        if (client != null)
        {
            try
            {
                client.Close();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        isConnected = false;
        StopCoroutine(ConnectServer());
    }

    public void Action_ConnectServer()
    {
        StartCoroutine(ConnectServer());
    }
    IEnumerator ConnectServer()
    {
        LastReceivedTimeMS = GetCurrentMS();

        LOGWARNING("try reconnecting");
        isReachedServer = false;
        if (client != null)
        {
            try
            {
                client.Close();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        yield return new WaitForSeconds(0.5f);
        client = new TcpClient();
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        stop = false;

        yield return new WaitForSeconds(0.1f);

        //Connect to server from another Thread
        Loom.RunAsync(() => {
            // if using the IPAD
            //client.Connect(IPAddress.Parse(IP), ClientListenPort);
            client.BeginConnect(IPAddress.Parse(IP), ClientListenPort, ClientEndConnect, null);
            while (!client.Connected)
            {
                System.Threading.Thread.Sleep(1);
            }

            if (client.Connected)
            {
                //----------------Sent CheckData----------------
                if (stream != null)
                {
                    stream.Close();
                }
                stream = client.GetStream();
                stream.ReadTimeout = 1000;
                stream.WriteTimeout = 200;

                LOGWARNING("Connected!");

                Loom.QueueOnMainThread(() =>
                {
                    isReachedServer = true;
                });

                if (client.GetStream() != null)
                {
                    byte[] sentByte = new byte[1];
                    client.GetStream().Write(sentByte, 0, sentByte.Length);
                    client.GetStream().Flush();
                }
                //----------------Sent CheckData----------------

                //----------------DataReceiver----------------
                while (!stop)
                {
                    stream.Flush();
                    //Read Data Count
                    dataSize = ReadDataByteSize(SEND_RECEIVE_COUNT);
                    LOGWARNING("Received Data byte Length: " + dataSize);

                    LastReceivedTimeMS = GetCurrentMS();
                    isConnected = true;

                    //Read Data Bytes and Process them
                    ReadDataByteArray(dataSize);

                    //System.Threading.Thread.Sleep(1);
                }
                //----------------DataReceiver----------------
            }
        });

        StartCoroutine(MainThreadSender());

        while (!stop)
        {
            while (_appendQueueReceived.Count > 0)
            {
                byte[] _data = new byte[1];
                lock (_asyncLockReceived) _data = _appendQueueReceived.Dequeue();

                ProcessReceivedData(_data);
            }
            yield return null;
        }
    }
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
            if (udpClient == null)
            {
                udpClient = new UdpClient();
                udpClient.Client.SendTimeout = 200;
                //udpClient.EnableBroadcast = true;
            }
            if (_appendQueueCmd.Count > 0)
            {
                while (_appendQueueCmd.Count > 0)
                {
                    byte[] _data = new byte[1];
                    lock (_asyncSend) _data = _appendQueueCmd.Dequeue();
                    udpClient.Send(_data, _data.Length, new IPEndPoint(IPAddress.Parse(IP), ServerListenPort));
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("error!: " + socketException.ToString());
            if (udpClient != null) udpClient.Close(); udpClient = null;
        }
    }

    void ClientEndConnect(IAsyncResult result)
    {
        client.EndConnect(result);
    }


    void LOGWARNING(string message)
    {
        if (enableLog) Debug.LogWarning(message);
    }

    void OnApplicationQuit()
    {
        LOGWARNING("OnApplicationQuit");
        stop = true;
        if (client != null) client.Close();
    }
    private void OnDestroy()
    {
        Action_Disconnect();
    }

    bool isPaused_old = false;
    bool isPaused = false;

    void CheckPause()
    {
        if (isPaused == true && isPaused_old == false)
        {
            LOGWARNING("pause");
            Action_Disconnect();
        }
        if (isPaused == false && isPaused_old == true)
        {
            LOGWARNING("running");
            if (triggerQuitApp == false)
            {
                triggerQuitApp = true;
                StartCoroutine(RelaunchApp());
            }
        }
        isPaused_old = isPaused;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        isPaused = !hasFocus;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        isPaused = pauseStatus;
    }

    private void OnDisable()
    {
        Action_Disconnect();
    }


    private void OnEnable()
    {
        if (Time.realtimeSinceStartup <= 3f) return;
    }
}

