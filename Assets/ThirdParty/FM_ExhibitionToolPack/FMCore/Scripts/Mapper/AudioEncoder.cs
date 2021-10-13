using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//[RequireComponent(typeof(AudioListener))]
public class AudioEncoder : MonoBehaviour
{
    //----------------------------------------------
    AudioListener[] AudioListenerObject;
    private Queue<byte> AudioBytes = new Queue<byte>();

    //[Header("[Capture In-Game Sound]")]
    public bool StreamGameSound = true;
    public int OutputSampleRate = 48000;
    public int OutputChannels = 2;

    private object _asyncLockAudio = new object();
    //----------------------------------------------

    [Range(1f, 60f)]
    public float StreamFPS = 20f;
    float interval = 0.05f;

    public bool GZipMode = false;

    public UnityEventByteArray OnDataByteReadyEvent;

    //[Header("Pair Encoder & Decoder")]
    public int label = 2001;
    int dataID = 0;
    int maxID = 1024;
    int chunkSize = 8096; //32768;
    float next = 0f;
    bool stop = false;

    public int dataLength;

    // Use this for initialization
    void Start ()
    {
        Application.runInBackground = true;

        if (GetComponent<AudioListener>() == null) this.gameObject.AddComponent<AudioListener>();
        OutputSampleRate = AudioSettings.GetConfiguration().sampleRate;
        AudioListenerObject = FindObjectsOfType<AudioListener>();
        for (int i = 0; i < AudioListenerObject.Length; i++)
        {
            if (AudioListenerObject[i] != null) AudioListenerObject[i].enabled = (AudioListenerObject[i].gameObject == this.gameObject);
        }
        StartCoroutine(SenderCOR());
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (StreamGameSound)
        {
            OutputChannels = channels;
            if (AudioBytes.Count > 2048 * 10)
            {
                lock (_asyncLockAudio)
                {
                    while (AudioBytes.Count > 2048 * 10) AudioBytes.Dequeue();
                }
                return;
            }
            lock (_asyncLockAudio)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    byte[] byteData = BitConverter.GetBytes(data[i]);
                    foreach (byte _byte in byteData) AudioBytes.Enqueue(_byte);
                }
            }
        }
    }

    IEnumerator SenderCOR()
    {
        while (!stop)
        {
            if (Time.realtimeSinceStartup > next)
            {
                interval = 1f / StreamFPS;
                next = Time.realtimeSinceStartup + interval;
                EncodeBytes();
            }
            yield return null;
        }
    }

    void EncodeBytes()
    {
        //==================getting byte data==================
        byte[] dataByte;

        byte[] _samplerateByte = BitConverter.GetBytes(OutputSampleRate);
        byte[] _channelsByte = BitConverter.GetBytes(OutputChannels);
        lock (_asyncLockAudio)
        {
            dataByte = new byte[AudioBytes.Count + _samplerateByte.Length + _channelsByte.Length];

            Buffer.BlockCopy(_samplerateByte, 0, dataByte, 0, _samplerateByte.Length);
            Buffer.BlockCopy(_channelsByte, 0, dataByte, 4, _channelsByte.Length);
            Buffer.BlockCopy(AudioBytes.ToArray(), 0, dataByte, 8, AudioBytes.Count);
            AudioBytes.Clear();
        }

        if (GZipMode) dataByte = dataByte.FMZipBytes();
        //==================getting byte data==================

        dataLength = dataByte.Length;
        int _length = dataByte.Length;
        int _offset = 0;

        byte[] _meta_label = BitConverter.GetBytes(label);
        byte[] _meta_id = BitConverter.GetBytes(dataID);
        byte[] _meta_length = BitConverter.GetBytes(_length);
        
        int chunks = Mathf.FloorToInt(dataByte.Length / chunkSize);
        for (int i = 0; i <= chunks; i++)
        {
            byte[] _meta_offset = BitConverter.GetBytes(_offset);
            int SendByteLength = (i == chunks) ? (_length % chunkSize + 17) : (chunkSize + 17);
            byte[] SendByte = new byte[SendByteLength];
            Buffer.BlockCopy(_meta_label, 0, SendByte, 0, 4);
            Buffer.BlockCopy(_meta_id, 0, SendByte, 4, 4);
            Buffer.BlockCopy(_meta_length, 0, SendByte, 8, 4);
            Buffer.BlockCopy(_meta_offset, 0, SendByte, 12, 4);
            SendByte[16] = (byte)(GZipMode ? 1 : 0);

            Buffer.BlockCopy(dataByte, _offset, SendByte, 17, SendByte.Length - 17);
            OnDataByteReadyEvent.Invoke(SendByte);
            _offset += chunkSize;
        }

        dataID++;
        if (dataID > maxID) dataID = 0;
    }

    private void OnEnable()
    {
        if (Time.realtimeSinceStartup <= 3f) return;
        if (stop)
        {
            stop = false;
            StartCoroutine(SenderCOR());
        }

        if(AudioListenerObject != null)
        {
            for (int i = 0; i < AudioListenerObject.Length; i++)
            {
                if (AudioListenerObject[i] != null) AudioListenerObject[i].enabled = (AudioListenerObject[i].gameObject == this.gameObject);
            }
        }
    }
    private void OnDisable()
    {
        stop = true;
        StopCoroutine(SenderCOR());

        //reset listener
        for (int i = 0; i < AudioListenerObject.Length; i++)
        {
            if(AudioListenerObject[i] != null) AudioListenerObject[i].enabled = (AudioListenerObject[i].gameObject != this.gameObject);
        }
    }
}
