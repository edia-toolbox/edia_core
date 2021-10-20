using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;
using System.Text;

using UnityEngine.UI;
using System.Net.NetworkInformation;

using UnityEngine.Events;
public class NetworkDiscovery : MonoBehaviour
{
    public enum NetworkType { Server, Client }
    public NetworkType NT;

    [HideInInspector]
    public string ServerStr;
    [HideInInspector]
    public string ClientStr;

    public int port = 2222;

    public string ServerIP = "0,0,0,0";
    public string ClientIP = "0,0,0,0";

    [Header("[milliseconds (MS)]")]
    public int frequency = 100;

    public string LocalIPAddress()
    {
        string localIP = "0.0.0.0";
        //IPHostEntry host;
        //host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            //if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            //commented above condition, as it may not work on Android, found issues on Google Pixel Phones
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (ip.IsDnsEligible)
                        {
                            try
                            {
                                if (ip.AddressValidLifetime / 2 != int.MaxValue)
                                {
                                    localIP = ip.Address.ToString();
                                    break;
                                }
                                else
                                {
                                    //if didn't find any yet, this is the only one
                                    if (localIP == "0.0.0.0") localIP = ip.Address.ToString();
                                }
                            }
                            catch
                            {
                                localIP = ip.Address.ToString();
                                //Debug.Log("LocalIPAddress(): " + e.ToString());
                                break;
                            }
                        }
                    }
                }
            }
        }
        return localIP;
    }

    public int StreamingPort;
    public void SetStreamingPort(int _port)
    {
        StreamingPort = _port;
    }

    public bool isStreaming = false;
    public void Action_SetIsStreaming(bool _value)
    {
        isStreaming = _value;

        //if it's streaming, we do not need to receive broadcast ip.
        if (isStreaming)
        {
            if (Client != null)
            {
                try
                {
                    Client.Close();
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
            isListening = false;
        }
    }
    public bool isListening = false;

    [Header("[CLIENT] will invoke when found server")]
    public UnityEventInt GetStreamingServerPort_Event;
    [Header("[CLIENT] will invoke when found server")]
    public UnityEventString GetServerIP_Event;

    [Header("[SERVER] will invoke when found client")]
    public UnityEventString GetClientIP_Event;


    [Header("[Client] suggested \"true\" when less than 3 threads")]
    public bool StopAfterFoundIP = false;
    bool stop = false;
    // Start is called before the first frame update
    void Start()
    {
        StartAll();
    }

    private void Update()
    {
        if (NT == NetworkType.Client)
        {
            if (!isStreaming && !isListening)
            {
                //if not streaming, try to receive the broadcast ip again.
                Action_StartClient();
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StopAll();
        }
    }

    public void Action_StartServer()
    {
        StartCoroutine(NetworkServerStart());

    }
    public void Action_StartClient()
    {
        StartCoroutine(NetworkClientStart());
    }

    [Header ("[Experimental] for supported devices only")]
    public bool UseAsyncServer = false;
    UdpClient Server;
    IEnumerator NetworkServerStart()
    {
        stop = false;
        yield return new WaitForSeconds(2.5f);

        if (!UseAsyncServer)
        {
            while (Loom.numThreads >= Loom.maxThreads)
            {
                yield return null;
            }

            Loom.RunAsync(() =>
            {
                while (!stop)
                {
                    byte[] ResponseData = Encoding.ASCII.GetBytes(StreamingPort.ToString());
                    try
                    {
                        Server = new UdpClient(port);
                        Server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        IPEndPoint ClientEp = new IPEndPoint(IPAddress.Any, port);

                        byte[] ClientRequestData = Server.Receive(ref ClientEp);
                        string ClientRequest = "CMD" + Encoding.ASCII.GetString(ClientRequestData);

                        Loom.QueueOnMainThread(() =>
                        {
                            ClientIP = ClientEp.Address.ToString();
                            GetClientIP_Event.Invoke(ClientIP);
                            ServerStr = "from client " + ClientEp.Address + " : " + ClientRequest;
                        });

                        Server.Client.EnableBroadcast = true;
                        Server.Send(ResponseData, ResponseData.Length, ClientEp);
                        Server.Close();
                    }
                    catch (SocketException socketException)
                    {
                        DebugLog("Socket exception: " + socketException);
                    }

                    System.Threading.Thread.Sleep(frequency);
                }
            });
        }
        else
        {
            //Experimental feature: try to reduce thread usage
            //Didn't work well on low-end devices during our testing
            Server = new UdpClient(port);
            Server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Server.Client.ReceiveTimeout = 1000;
            Server.Client.SendTimeout = 200;
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

        yield break;
    }

    void UdpReceiveCallback(IAsyncResult ar)
    {
        if (ar.IsCompleted)
        {
            //receive callback completed
            IPEndPoint ClientEp = new IPEndPoint(IPAddress.Any, port);
            byte[] ClientRequestData = Server.EndReceive(ar, ref ClientEp);

            //Process codes
            string ClientRequest = "CMD" + Encoding.ASCII.GetString(ClientRequestData);

            ClientIP = ClientEp.Address.ToString();
            GetClientIP_Event.Invoke(ClientIP);

            ServerStr = "from client " + ClientEp.Address + " : " + ClientRequest;

            byte[] ResponseData = Encoding.ASCII.GetBytes(StreamingPort.ToString());

            Server.Client.EnableBroadcast = true;

            //respond to client
            try
            {
                Server.Send(ResponseData, ResponseData.Length, ClientEp);
            }
            catch (SocketException socketException)
            {
                DebugLog("Socket exception: " + socketException);
            }
        }

        if (!stop)
        {
            try
            {
                Server.BeginReceive(new AsyncCallback(UdpReceiveCallback), null);
            }
            catch (SocketException socketException)
            {
                DebugLog("network discovery: can't find any server: " + socketException.ToString());
            }
        }
    }

    UdpClient Client;
    IEnumerator NetworkClientStart()
    {
        stop = false;
        isListening = true;
        yield return new WaitForSeconds(2.5f);
        string LastSeenTime = System.DateTime.Now.ToString("mm:ss");
        while (Loom.numThreads >= Loom.maxThreads)
        {
            yield return new WaitForEndOfFrame();
        }
        Loom.RunAsync(() =>
        {
            while (!isStreaming && !stop)
            {
                try
                {
                    Client = new UdpClient();
                    Client.Client.SendTimeout = 200;
                    Client.Client.ReceiveTimeout = 500;
                    //Client.Client.ReceiveTimeout = 5000;

                    byte[] RequestData = Encoding.ASCII.GetBytes("last: " + LastSeenTime);
                    IPEndPoint ServerEp = new IPEndPoint(IPAddress.Any, port);
                    //IPEndPoint ServerEp = new IPEndPoint(IPAddress.Any, 0);

                    Client.EnableBroadcast = true;
                    Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, port));

                    byte[] ServerResponseData = Client.Receive(ref ServerEp);
                    string ServerResponse = Encoding.ASCII.GetString(ServerResponseData);

                    Client.Close();

                    if (ServerResponse.Length > 0)
                    {
                        Loom.QueueOnMainThread(() =>
                        {
                            LastSeenTime = System.DateTime.Now.ToString("mm:ss");
                            if (StopAfterFoundIP)
                            {
                                stop = true;
                            }

                            //get and assign the port
                            StreamingPort = int.Parse(ServerResponse);
                            GetStreamingServerPort_Event.Invoke(StreamingPort);

                            //get and assign server ip
                            ServerIP = ServerEp.Address.ToString();
                            GetServerIP_Event.Invoke(ServerIP);

                            ClientStr = "from server: " + ServerResponse + " : " + ServerIP;
                        });
                    }
                }
                catch (SocketException socketException)
                {
                    DebugLog("network discovery: can't find any server: " + socketException.ToString());
                }

                System.Threading.Thread.Sleep(frequency);
            }
            System.Threading.Thread.Sleep(1);
        });

        yield break;
    }

    public bool ShowLog = true;
    public void DebugLog(string _value)
    {
        if (ShowLog)
        {
            Debug.Log(_value);
        }
    }

    private void OnApplicationQuit()
    {
        StopAll();
    }
    private void OnDisable()
    {
        StopAll();
    }
    private void OnDestroy()
    {
        StopAll();
    }
    private void OnEnable()
    {
        if (Time.frameCount <= 0)
        {
            return;
        }
        if (stop)
        {
            StartAll();
        }

    }

    void StartAll()
    {
        isStreaming = false;
        isListening = false;
        stop = false;
        if (NT == NetworkType.Server)
        {
            ServerIP = LocalIPAddress();
            Action_StartServer();
        }
        if (NT == NetworkType.Client)
        {
            ClientIP = LocalIPAddress();
            Action_StartClient();
        }
    }
    void StopAll()
    {
        stop = true;
        if (Client != null)
        {
            try
            {
                Client.Close();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        if (Server != null)
        {
            try
            {
                Server.Close();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        if (NT == NetworkType.Server)
        {
            StopCoroutine(NetworkServerStart());
        }
        if (NT == NetworkType.Client)
        {
            StopCoroutine(NetworkClientStart());
        }
    }
}
