using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public enum MicDeviceMode { Default, TargetDevice }
public class MicEncoder : MonoBehaviour{
#if !UNITY_WEBGL || UNITY_EDITOR
    //----------------------------------------------
    AudioSource AudioMic;
    private Queue<byte> AudioBytes = new Queue<byte>();

    public MicDeviceMode DeviceMode = MicDeviceMode.Default;
    public string TargetDeviceName = "MacBook Pro Microphone";
    string CurrentDeviceName = null;

    [TextArea]
    public string DetectedDevices;

    //[Header("[Capture In-Game Sound]")]
    public bool StreamGameSound = true;
    public int OutputSampleRate = 11025;
    public int OutputChannels = 1;
    private object _asyncLockAudio = new object();

    int CurrentAudioTimeSample = 0;
    int LastAudioTimeSample = 0;
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
    void Start()
    {
        StartCoroutine(CaptureMic());
        StartCoroutine(SenderCOR());
    }

    IEnumerator CaptureMic()
    {
        if (AudioMic == null) AudioMic = GetComponent<AudioSource>();
        if (AudioMic == null) AudioMic = gameObject.AddComponent<AudioSource>();

        //Check Target Device
        DetectedDevices = "";
        string[] MicNames = Microphone.devices;
        foreach (string _name in MicNames) DetectedDevices += _name + "\n";
        if (DeviceMode == MicDeviceMode.TargetDevice)
        {
            bool IsCorrectName = false;
            for(int i = 0; i<MicNames.Length; i++)
            {
                if(MicNames[i] == TargetDeviceName)
                {
                    IsCorrectName = true;
                    break;
                }
            }
            if (!IsCorrectName) TargetDeviceName = null;
        }
        //Check Target Device

        CurrentDeviceName = DeviceMode == MicDeviceMode.Default ? null : TargetDeviceName;

        AudioMic.clip = Microphone.Start(CurrentDeviceName, true, 1, OutputSampleRate);
        AudioMic.loop = true;
        while (!(Microphone.GetPosition(CurrentDeviceName) > 0)) { }
        Debug.Log("Start Mic(pos): " + Microphone.GetPosition(CurrentDeviceName));
        AudioMic.Play();

        AudioMic.volume = 0f;

        OutputChannels = AudioMic.clip.channels;

        while (!stop)
        {
            AddMicData();
            yield return null;
        }
        yield return null;
    }


    void AddMicData()
    {
        LastAudioTimeSample = CurrentAudioTimeSample;
        //CurrentAudioTimeSample = AudioMic.timeSamples;
        CurrentAudioTimeSample = Microphone.GetPosition(CurrentDeviceName);

        if (CurrentAudioTimeSample != LastAudioTimeSample)
        {
            float[] samples = new float[AudioMic.clip.samples];
            AudioMic.clip.GetData(samples, 0);

            if (CurrentAudioTimeSample > LastAudioTimeSample)
            {
                lock (_asyncLockAudio)
                {
                    for (int i = LastAudioTimeSample; i < CurrentAudioTimeSample; i++)
                    {
                        byte[] byteData = BitConverter.GetBytes(samples[i]);
                        foreach (byte _byte in byteData) AudioBytes.Enqueue(_byte);
                    }
                }
            }
            else if (CurrentAudioTimeSample < LastAudioTimeSample)
            {
                lock (_asyncLockAudio)
                {
                    for (int i = LastAudioTimeSample; i < samples.Length; i++)
                    {
                        byte[] byteData = BitConverter.GetBytes(samples[i]);
                        foreach (byte _byte in byteData) AudioBytes.Enqueue(_byte);
                    }
                    for (int i = 0; i < CurrentAudioTimeSample; i++)
                    {
                        byte[] byteData = BitConverter.GetBytes(samples[i]);
                        foreach (byte _byte in byteData) AudioBytes.Enqueue(_byte);
                    }
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
        StartAll();
    }
    private void OnDisable()
    {
        StopAll();
    }

    void StartAll()
    {
        if (stop)
        {
            stop = false;
            StartCoroutine(SenderCOR());
            StartCoroutine(CaptureMic());
        }
    }
    void StopAll()
    {
        stop = true;
        StopCoroutine(SenderCOR());
        StopCoroutine(CaptureMic());

        AudioMic.Stop();
        Microphone.End(CurrentDeviceName);
    }
#endif
}
