using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class AudioDecoder : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;
		DeviceSampleRate = AudioSettings.GetConfiguration().sampleRate;
    }

    bool ReadyToGetFrame = true;

    //[Header ("Pair Encoder & Decoder")]
    public int label = 2001;
    int dataID = 0;
    //int maxID = 1024;
    int dataLength = 0;
    int receivedLength = 0;

    byte[] dataByte;
    public bool GZipMode = false;

    public void Action_ProcessData(byte[] _byteData)
    {
        if (!enabled) return;
        if (_byteData.Length <= 8) return;

        int _label = BitConverter.ToInt32(_byteData, 0);
        if (_label != label) return;

        int _dataID = BitConverter.ToInt32(_byteData, 4);
        //if (_dataID < dataID) return;

        if (_dataID != dataID) receivedLength = 0;
        dataID = _dataID;
        dataLength = BitConverter.ToInt32(_byteData, 8);
        int _offset = BitConverter.ToInt32(_byteData, 12);

        GZipMode = _byteData[16] == 1;

        if (receivedLength == 0) dataByte = new byte[dataLength];
        receivedLength += _byteData.Length - 17;
        Buffer.BlockCopy(_byteData, 17, dataByte, _offset, _byteData.Length - 17);

        if (ReadyToGetFrame)
        {
            if (receivedLength == dataLength) StartCoroutine(ProcessAudioData(dataByte));
        }
    }

    //[Header("[Audio Info]")]
    public int SourceChannels = 1;
    public double SourceSampleRate = 48000;
    public double DeviceSampleRate = 48000;

    private Queue<float> ABufferQueue = new Queue<float>();
    object _asyncLock = new object();

    IEnumerator ProcessAudioData(byte[] receivedAudioBytes)
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            ReadyToGetFrame = false;
            if (GZipMode) receivedAudioBytes = receivedAudioBytes.FMUnzipBytes();

            if (receivedAudioBytes.Length >= 8 + 1024)
            {
                byte[] _sampleRateByte = new byte[4];
                byte[] _channelsByte = new byte[4];
                byte[] _audioByte = new byte[1];
                lock (_asyncLock)
                {
                    _audioByte = new byte[receivedAudioBytes.Length - 8];
                    Buffer.BlockCopy(receivedAudioBytes, 0, _sampleRateByte, 0, _sampleRateByte.Length);
                    Buffer.BlockCopy(receivedAudioBytes, 4, _channelsByte, 0, _channelsByte.Length);
                    Buffer.BlockCopy(receivedAudioBytes, 8, _audioByte, 0, _audioByte.Length);
                }

                SourceSampleRate = BitConverter.ToInt32(_sampleRateByte, 0);
                SourceChannels = BitConverter.ToInt32(_channelsByte, 0);

                float[] ABuffer = ToFloatArray(_audioByte);

                for (int i = 0; i < ABuffer.Length; i++)
                {
                    ABufferQueue.Enqueue(ABuffer[i]);
                }

                CreateClip();
            }
            ReadyToGetFrame = true;
        }
        yield return null;
    }

    int position = 0;
    int samplerate = 44100;
    int channel = 2;

    AudioClip myClip;
    AudioSource Audio;
    void CreateClip()
    {
        if (samplerate != (int)SourceSampleRate || channel != SourceChannels)
        {
            samplerate = (int)SourceSampleRate;
            channel = SourceChannels;

            if (Audio != null) Audio.Stop();
            if (myClip != null) DestroyImmediate(myClip);

            myClip = AudioClip.Create("StreamingAudio", samplerate * SourceChannels, SourceChannels, samplerate, true, OnAudioRead, OnAudioSetPosition);
            Audio = GetComponent<AudioSource>();
            Audio.clip = myClip;
            Audio.loop = true;
            Audio.Play();
        }

    }

    void OnAudioRead(float[] data)
    {
        int count = 0;
        while (count < data.Length)
        {
            if (ABufferQueue.Count > 0)
            {
                lock (_asyncLock) data[count] = ABufferQueue.Dequeue();
            }
            else
            {
                data[count] = 0f;
            }

            position++;
            count++;
        }
    }

    void OnAudioSetPosition(int newPosition)
    {
        position = newPosition;
    }

    public float[] ToFloatArray(byte[] byteArray)
    {
        int len = byteArray.Length / 4;
        float[] floatArray = new float[len];
        for (int i = 0; i < byteArray.Length; i += 4)
        {
            floatArray[i / 4] = BitConverter.ToSingle(byteArray, i);
        }
        return floatArray;
    }
}


