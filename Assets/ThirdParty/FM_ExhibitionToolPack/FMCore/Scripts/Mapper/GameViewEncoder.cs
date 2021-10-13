using System.Collections;
using UnityEngine;
using System;

public enum GameViewCaptureMode { RenderCam, MainCam, FullScreen, Desktop }
public enum GameViewResize { Full, Half, Quarter, OneEighth }
public enum GameViewCubemapSample
{
    High = 2048,
    Medium = 1024,
    Low = 512,
    Minimum = 256
}

public class GameViewEncoder : MonoBehaviour
{
    public GameViewCaptureMode CaptureMode = GameViewCaptureMode.RenderCam;
    private GameViewCaptureMode _CaptureMode = GameViewCaptureMode.RenderCam;
    public GameViewResize Resize = GameViewResize.Quarter;

    public Camera MainCam;
    public Camera RenderCam;

    public Vector2 Resolution = new Vector2(512, 512);
    private Vector2 _Resolution = new Vector2(512, 512);
    public bool MatchScreenAspect = true;

    public bool FastMode = false;
    public bool AsyncMode = false;

    public bool GZipMode = false;
    public bool PanoramaMode = false;

    [Range(10, 100)]
    public int Quality = 40;

    [Range(0f, 60f)]
    public float StreamFPS = 20f;
    float interval = 0.05f;

    bool NeedUpdateTexture = false;
    bool EncodingTexture = false;

    public Texture2D CapturedTexture;
    RenderTexture rt;
    public RenderTexture rt_cube;
    [HideInInspector] public Material MatPano; //has to be public, otherwise the shader will be  missing

    public GameViewCubemapSample CubemapResolution = GameViewCubemapSample.Medium;

    Texture2D Screenshot;
    ColorSpace ColorSpace;

#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX)
    Texture2D DesktopTexture;
    Material FMDesktopMat;
#endif
    public Vector2 FMDesktopResolution = Vector2.zero;
    public bool FMDesktopFlipX = false;
    public bool FMDesktopFlipY = false;
    [Range(0.00001f, 2f)]
    public float FMDesktopRangeX = 1f;
    [Range(0.00001f, 2f)]
    public float FMDesktopRangeY = 1f;
    [Range(-0.5f, 0.5f)]
    public float FMDesktopOffsetX = 0f;
    [Range(-0.5f, 0.5f)]
    public float FMDesktopOffsetY = 0f;
    [Range(0, 8)]
    public int FMDesktopMonitorID = 0;
    public int FMDesktopMonitorCount = 0;

    public UnityEventByteArray OnDataByteReadyEvent;

    //[Header("Pair Encoder & Decoder")]
    public int label = 1001;
    int dataID = 0;
    int maxID = 1024;
    int chunkSize = 8096; //32768
    float next = 0f;
    bool stop = false;
    byte[] dataByte;

    public int dataLength;

    void CaptureModeUpdate()
    {
#if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN
        if (CaptureMode == GameViewCaptureMode.Desktop) CaptureMode = GameViewCaptureMode.FullScreen;
#endif
        if (_CaptureMode != CaptureMode)
        {
            _CaptureMode = CaptureMode;
            if (rt != null) Destroy(rt);
            if (CapturedTexture != null) Destroy(CapturedTexture);
        }
    }

    //init when added component, or reset component
    void Reset()
    {
        MatPano = new Material(Shader.Find("Hidden/FMCubemapToEquirect"));
    }

    private void Start()
    {
        Application.runInBackground = true;
        ColorSpace = QualitySettings.activeColorSpace;

        CaptureModeUpdate();
        StartCoroutine(SenderCOR());
    }

    private void Update()
    {
        Resolution.x = Mathf.RoundToInt(Resolution.x);
        Resolution.y = Mathf.RoundToInt(Resolution.y);
        _Resolution = Resolution;

        CaptureModeUpdate();

        switch (_CaptureMode)
        {
            case GameViewCaptureMode.MainCam:
                if (MainCam == null) MainCam = this.GetComponent<Camera>();
                _Resolution = new Vector2(Screen.width, Screen.height);
                _Resolution /= Mathf.Pow(2, (int)Resize);
                break;
            case GameViewCaptureMode.RenderCam:
                if (MatchScreenAspect)
                {
                    if (Screen.width > Screen.height) _Resolution.y = _Resolution.x / (float)(Screen.width) * (float)(Screen.height);
                    if (Screen.width < Screen.height) _Resolution.x = _Resolution.y / (float)(Screen.height) * (float)(Screen.width);
                }
                break;
            case GameViewCaptureMode.FullScreen:
                _Resolution = new Vector2(Screen.width, Screen.height);
                _Resolution /= Mathf.Pow(2, (int)Resize);
                break;
            case GameViewCaptureMode.Desktop:
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX)
                if (DesktopTexture != null)
                {
                    if (MatchScreenAspect)
                    {
                        if (FMDesktopRangeX == 0) FMDesktopRangeX = 0.00001f;
                        if (FMDesktopRangeY == 0) FMDesktopRangeY = 0.00001f;
                        float TargetRatio = ((float)DesktopTexture.width * FMDesktopRangeX) / ((float)DesktopTexture.height * FMDesktopRangeY);
                        float RenderRatio = _Resolution.x / _Resolution.y;
                        if(TargetRatio > RenderRatio) _Resolution.y = _Resolution.x / TargetRatio;
                        if(TargetRatio < RenderRatio) _Resolution.x = _Resolution.y * TargetRatio;
                    }
                }
#endif
                break;
        }

        if (_CaptureMode != GameViewCaptureMode.RenderCam)
        {
            if (RenderCam != null)
            {
                if (RenderCam.targetTexture != null) RenderCam.targetTexture = null;
            }
        }
    }

    void CheckResolution()
    {
        _Resolution.x = Mathf.RoundToInt(_Resolution.x);
        _Resolution.y = Mathf.RoundToInt(_Resolution.y);
        if (_Resolution.x == 0) _Resolution.x = 1;
        if (_Resolution.y == 0) _Resolution.y = 1;

        bool IsLinear = (ColorSpace == ColorSpace.Linear) && (CaptureMode == GameViewCaptureMode.FullScreen);

        if (PanoramaMode)
        {
            if (rt_cube == null)
            {
                rt_cube = new RenderTexture((int)CubemapResolution, (int)CubemapResolution, 0, RenderTextureFormat.ARGB32, IsLinear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
            }
            else
            {
                if (rt_cube.width != (int)CubemapResolution || rt_cube.height != (int)CubemapResolution || rt_cube.sRGB != IsLinear)
                {
                    Destroy(rt);
                    new RenderTexture((int)CubemapResolution, (int)CubemapResolution, 0, RenderTextureFormat.ARGB32, IsLinear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
                }
            }
            rt_cube.antiAliasing = 1;
            rt_cube.filterMode = FilterMode.Bilinear;
            rt_cube.anisoLevel = 0;
            rt_cube.dimension = UnityEngine.Rendering.TextureDimension.Cube;
            rt_cube.autoGenerateMips = false;
        }


        if (rt == null)
        {
            rt = new RenderTexture(Mathf.RoundToInt(_Resolution.x), Mathf.RoundToInt(_Resolution.y), 16, RenderTextureFormat.ARGB32, IsLinear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
        }
        else
        {
            if (rt.width != Mathf.RoundToInt(_Resolution.x) || rt.height != Mathf.RoundToInt(_Resolution.y) || rt.sRGB != IsLinear)
            {
                Destroy(rt);
                rt = new RenderTexture(Mathf.RoundToInt(_Resolution.x), Mathf.RoundToInt(_Resolution.y), 16, RenderTextureFormat.ARGB32, IsLinear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
            }
        }

        if (CapturedTexture == null)
        {
            CapturedTexture = new Texture2D(Mathf.RoundToInt(_Resolution.x), Mathf.RoundToInt(_Resolution.y), TextureFormat.RGB24, false, IsLinear);
        }
        else
        {
            if (CapturedTexture.width != Mathf.RoundToInt(_Resolution.x) || CapturedTexture.height != Mathf.RoundToInt(_Resolution.y))
            {
                Destroy(CapturedTexture);
                CapturedTexture = new Texture2D(Mathf.RoundToInt(_Resolution.x), Mathf.RoundToInt(_Resolution.y), TextureFormat.RGB24, false, IsLinear);
            }
        }
        //Debug.Log(ColorSpace + "RT: " + IsLinear_RT + " : " + IsLinear_Texture);
    }

    IEnumerator ProcessCapturedTexture()
    {
        if (PanoramaMode) Graphics.Blit(rt_cube, rt, MatPano);

        //render texture to texture2d
        CapturedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        CapturedTexture.Apply();

        //encode to byte
        StartCoroutine(EncodeBytes());
        yield break;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_CaptureMode == GameViewCaptureMode.MainCam)
        {
            if (NeedUpdateTexture && !EncodingTexture)
            {
                NeedUpdateTexture = false;
                CheckResolution();
                Graphics.Blit(source, rt);

                StartCoroutine(ProcessCapturedTexture());
            }
        }
        Graphics.Blit(source, destination);
    }

    IEnumerator RenderTextureRefresh()
    {
        if (NeedUpdateTexture && !EncodingTexture)
        {
            NeedUpdateTexture = false;
            EncodingTexture = true;

            yield return new WaitForEndOfFrame();
            CheckResolution();

            if (_CaptureMode == GameViewCaptureMode.RenderCam)
            {
                if (RenderCam != null)
                {
                    if (PanoramaMode)
                    {
                        RenderCam.targetTexture = rt_cube;
                        RenderCam.RenderToCubemap(rt_cube);

                        // Backup the currently set RenderTexture
                        RenderTexture previous = RenderTexture.active;
                        Shader.SetGlobalFloat("FORWARD", RenderCam.transform.eulerAngles.y * 0.01745f);

                        // Set the current RenderTexture to the temporary one we created
                        RenderTexture.active = rt_cube;

                        //RenderTexture to Texture2D
                        StartCoroutine(ProcessCapturedTexture());

                        // Reset the active RenderTexture
                        RenderTexture.active = previous;
                    }
                    else
                    {
                        RenderCam.targetTexture = rt;
                        RenderCam.Render();

                        // Backup the currently set RenderTexture
                        RenderTexture previous = RenderTexture.active;

                        // Set the current RenderTexture to the temporary one we created
                        RenderTexture.active = rt;

                        //RenderTexture to Texture2D
                        StartCoroutine(ProcessCapturedTexture());

                        // Reset the active RenderTexture
                        RenderTexture.active = previous;
                    }
                }
                else
                {
                    EncodingTexture = false;
                }
            }

            if (_CaptureMode == GameViewCaptureMode.FullScreen)
            {
                if (Resize == GameViewResize.Full)
                {
                    // cleanup
                    if (CapturedTexture != null) Destroy(CapturedTexture);
                    CapturedTexture = ScreenCapture.CaptureScreenshotAsTexture();
                    StartCoroutine(EncodeBytes());
                }
                else
                {
                    // cleanup
                    if (Screenshot != null) Destroy(Screenshot);
                    Screenshot = ScreenCapture.CaptureScreenshotAsTexture();
                    Graphics.Blit(Screenshot, rt);

                    //RenderTexture to Texture2D
                    StartCoroutine(ProcessCapturedTexture());
                }
            }

            if (_CaptureMode == GameViewCaptureMode.Desktop)
            {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX)
                FMDesktopMonitorCount = FMDesktop.Manager.monitorCount;
                if (FMDesktopMonitorID >= (FMDesktopMonitorCount - 1)) FMDesktopMonitorID = FMDesktopMonitorCount - 1;
                if (FMDesktopMonitorID < 0) FMDesktopMonitorID = 0;

                if (FMDesktop.Manager.GetMonitor(FMDesktopMonitorID) != null)
                {
                    if (FMDesktopMat == null) FMDesktopMat = new Material(Shader.Find("Hidden/FMDesktopMask"));

                    FMDesktop.Manager.GetMonitor(FMDesktopMonitorID).shouldBeUpdated = true;
                    DesktopTexture = FMDesktop.Manager.GetMonitor(FMDesktopMonitorID).texture;

                    FMDesktopMat.SetFloat("_FlipX", FMDesktopFlipX ? 0f : 1f);
                    FMDesktopMat.SetFloat("_FlipY", FMDesktopFlipY ? 0f : 1f);

                    FMDesktopMat.SetFloat("_RangeX", FMDesktopRangeX);
                    FMDesktopMat.SetFloat("_RangeY", FMDesktopRangeY);

                    FMDesktopMat.SetFloat("_OffsetX", FMDesktopOffsetX);
                    FMDesktopMat.SetFloat("_OffsetY", FMDesktopOffsetY);

                    Graphics.Blit(DesktopTexture, rt, FMDesktopMat);

                    //RenderTexture to Texture2D
                    StartCoroutine(ProcessCapturedTexture());
                }
                else
                {
                    EncodingTexture = false;
                }
#else
                EncodingTexture = false;
#endif
            }
        }
    }

    public void Action_UpdateTexture()
    {
        RequestTextureUpdate();
    }

    void RequestTextureUpdate()
    {
        if (!EncodingTexture)
        {
            NeedUpdateTexture = true;
            if (_CaptureMode != GameViewCaptureMode.MainCam) StartCoroutine(RenderTextureRefresh());
        }
    }

    IEnumerator SenderCOR()
    {
        while (!stop)
        {
            if (Time.realtimeSinceStartup > next)
            {
                if (StreamFPS > 0)
                {
                    interval = 1f / StreamFPS;
                    next = Time.realtimeSinceStartup + interval;

                    RequestTextureUpdate();
                }

                yield return null;
            }
            yield return null;
        }
    }

    IEnumerator EncodeBytes()
    {
        if (CapturedTexture != null)
        {
            yield return null;
            //==================getting byte data==================
#if UNITY_IOS && !UNITY_EDITOR
            FastMode = true;
#endif

#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_WIN || UNITY_IOS || UNITY_ANDROID || WINDOWS_UWP
            if (FastMode)
            {
                //try AsyncMode, on supported platform
                if (AsyncMode && Loom.numThreads < Loom.maxThreads)
                {
                    //has spare thread
                    bool AsyncEncoding = true;
                    byte[] RawTextureData = CapturedTexture.GetRawTextureData();
                    int _width = CapturedTexture.width;
                    int _height = CapturedTexture.height;

                    Loom.RunAsync(() =>
                    {
                        dataByte = RawTextureData.FMRawTextureDataToJPG(_width, _height, Quality);
                        AsyncEncoding = false;
                    });
                    while (AsyncEncoding) yield return null;
                }
                else
                {
                    //need yield return, in order to fix random error "coroutine->IsInList()"
                    yield return dataByte = CapturedTexture.FMEncodeToJPG(Quality);
                }
            }
            else
            {
                dataByte = CapturedTexture.EncodeToJPG(Quality);
            }
#else
            dataByte = CapturedTexture.EncodeToJPG(Quality);
#endif

            if (GZipMode) dataByte = dataByte.FMZipBytes();

            dataLength = dataByte.Length;
            //==================getting byte data==================
            int _length = dataByte.Length;
            int _offset = 0;

            byte[] _meta_label = BitConverter.GetBytes(label);
            byte[] _meta_id = BitConverter.GetBytes(dataID);
            byte[] _meta_length = BitConverter.GetBytes(_length);

            int chunks = Mathf.FloorToInt(dataByte.Length / chunkSize);
            for (int i = 0; i <= chunks; i++)
            {
                int SendByteLength = (i == chunks) ? (_length % chunkSize + 17) : (chunkSize + 17);
                byte[] _meta_offset = BitConverter.GetBytes(_offset);
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

        EncodingTexture = false;
        yield break;
    }

    void OnEnable() { StartAll(); }
    void OnDisable() { StopAll(); }
    void OnApplicationQuit() { StopAll(); }
    void OnDestroy() { StopAll(); }

    void StopAll()
    {
        stop = true;
        StopAllCoroutines();
    }
    void StartAll()
    {
        if (Time.realtimeSinceStartup < 3f) return;
        stop = false;
        StartCoroutine(SenderCOR());

        NeedUpdateTexture = false;
        EncodingTexture = false;
    }
}
