using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LargeFileEncoder : MonoBehaviour {

    public bool Sending = false;

    [Tooltip("smaller number: take longer time to send \nTry to reduce the number if the file cannot be sent successfully")]
    [Range(1,60)]
    public int AddDelayEveryPacket = 4;

    public UnityEventByteArray OnDataByteReadyEvent;

    [Header("Pair Encoder & Decoder")]
    public int label = 8001;
    int dataID = 0;
    int maxID = 1024;
    int chunkSize = 8096; //32768;

    public int dataLength;

    void Start()
    {
        Application.runInBackground = true;
    }

    public void Action_SendLargeByte(byte[] _data)
    {
        StartCoroutine(SenderCOR(_data));
    }

    IEnumerator SenderCOR(byte[] dataByte)
    {
        yield return null;
        if (!Sending)
        {
            Sending = true;

            dataLength = dataByte.Length;
            int _length = dataByte.Length;
            int _offset = 0;

            byte[] _meta_label = BitConverter.GetBytes(label);
            byte[] _meta_id = BitConverter.GetBytes(dataID);
            byte[] _meta_length = BitConverter.GetBytes(_length);

            int chunks = Mathf.FloorToInt(dataByte.Length / chunkSize);
            for (int i = 0; i <= chunks; i++)
            {
                int SendByteLength = (i == chunks) ? (_length % chunkSize + 16) : (chunkSize + 16);
                byte[] _meta_offset = BitConverter.GetBytes(_offset);
                byte[] SendByte = new byte[SendByteLength];

                Buffer.BlockCopy(_meta_label, 0, SendByte, 0, 4);
                Buffer.BlockCopy(_meta_id, 0, SendByte, 4, 4);
                Buffer.BlockCopy(_meta_length, 0, SendByte, 8, 4);

                Buffer.BlockCopy(_meta_offset, 0, SendByte, 12, 4);

                Buffer.BlockCopy(dataByte, _offset, SendByte, 16, SendByte.Length - 16);
                OnDataByteReadyEvent.Invoke(SendByte);
                _offset += chunkSize;
            }

            dataID++;
            if (dataID > maxID) dataID = 0;

            Sending = false;
        }
    }

    void OnDisable()
    {
        StopAll();
    }
    void OnApplicationQuit()
    {
        StopAll();
    }
    void OnDestroy()
    {
        StopAll();
    }

    void StopAll()
    {
        StopAllCoroutines();
    }
}
