using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebcamManager : MonoBehaviour
{

    WebCamDevice[] devices;
    WebCamTexture[] webCams;
    Texture[] textures;
    public int TargetCamID=0;
    public bool useFrontCam = true;
    public void Action_useFrontCam(bool _value)
    {
        useFrontCam = _value;
        while (!canRestart) return;
        StartCoroutine(RestartWebcam());
    }
    bool canRestart = true;
    IEnumerator RestartWebcam()
    {
        canRestart = false;
        Action_StopWebcam();
        yield return initAndWaitForWebCamTexture();
        canRestart = true;
        yield return null;
    }

    WebCamTexture webCamTexture;

    //Texture texture;
    public Material[] materials;
    public GameObject[] targetMeshObjects;
    public GameObject BackgroundQuad;

    int timeoutFrameCount = 300;

    public Vector2 requestResolution=new Vector2(1280,720);
    public Vector2 textureResolution;
    //bool isFlipped;

    bool isInitWaiting = false;

    float TextureRatio = 1f;
    float ScreenRatio = 1f;

    Quaternion baseRotation;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(initAndWaitForWebCamTexture());
        if (BackgroundQuad != null) baseRotation = BackgroundQuad.transform.localRotation;
    }

    void Update()
    {
        if (BackgroundQuad != null && webCamTexture != null) CalculateBackgroundQuad();
    }

    public void OnApplicationQuit()
    {
        Action_StopWebcam();
    }

    void CalculateBackgroundQuad()
    {
        Camera cam = Camera.main;
        ScreenRatio = (float)Screen.width / (float)Screen.height;

        BackgroundQuad.transform.SetParent(cam.transform);
        BackgroundQuad.transform.localPosition = new Vector3(0f, 0f, cam.farClipPlane / 2f);

        float videoRotationAngle = webCamTexture.videoRotationAngle;

        BackgroundQuad.transform.localRotation = baseRotation * Quaternion.AngleAxis(webCamTexture.videoRotationAngle, Vector3.forward);

        float distance = cam.farClipPlane / 2f;
        float frustumHeight = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        BackgroundQuad.transform.localPosition = new Vector3(0f, 0f, distance);
        Vector3 QuadScale = new Vector3(1f, frustumHeight, 1f);

        //adjust the scaling for portrait Mode & Landscape Mode
        if (videoRotationAngle == 0 || videoRotationAngle == 180)
        {
            //landscape mode
            TextureRatio = (float)(webCamTexture.width) / (float)(webCamTexture.height);
            if (ScreenRatio > TextureRatio)
            {
                float SH = ScreenRatio / TextureRatio;
                float TW = TextureRatio * frustumHeight * SH;
                float TH = frustumHeight * (webCamTexture.videoVerticallyMirrored ? -1 : 1) * SH;
                QuadScale = new Vector3(TW, TH, 1f);
            }
            else
            {
                float TW = TextureRatio * frustumHeight;
                QuadScale = new Vector3(TW, frustumHeight * (webCamTexture.videoVerticallyMirrored ? -1 : 1), 1f);
            }
        }
        else
        {
            //portrait mode
            TextureRatio = (float)(webCamTexture.height) / (float)(webCamTexture.width);
            if (ScreenRatio > TextureRatio)
            {
                float SH = ScreenRatio / TextureRatio;
                float TW = frustumHeight * -1f * SH;
                float TH = TW * (webCamTexture.videoVerticallyMirrored ? 1 : -1) * SH;
                QuadScale = new Vector3(TW, TH, 1f);
            }
            else
            {
                float TW = TextureRatio * frustumHeight;
                QuadScale = new Vector3(frustumHeight * -1f, TW * (webCamTexture.videoVerticallyMirrored ? 1 : -1), 1f);
            }
        }
        BackgroundQuad.transform.localScale = QuadScale;
    }

    public void Action_StopWebcam()
    {
        //if (!enabled || !gameObject.activeSelf) return;
        if (webCams != null)
        {
            for (int i = 0; i < webCams.Length; i++)
            {
                if (webCams[i].isPlaying) webCams[i].Stop();
                Destroy(webCams[i]);
                Destroy(textures[i]);
            }
        }
        webCams = null;
        devices = null;
        textures = null;

        Destroy(webCamTexture);
        webCamTexture = null;
    }

    public void Action_StartWebcam()
    {
        if (!enabled || !gameObject.activeSelf) return;
        if (!isInitWaiting) StartCoroutine(initAndWaitForWebCamTexture());
    }
    private void OnEnable()
    {
        if (Time.realtimeSinceStartup < 3f) return;
        Action_StartWebcam();
    }
    private void OnDisable()
    {
        Action_StopWebcam();
    }

    IEnumerator initAndWaitForWebCamTexture()
    {
        isInitWaiting = true;

        devices = WebCamTexture.devices;
        webCams = new WebCamTexture[devices.Length];
        textures = new Texture[devices.Length];

        for (int i = 0; i < devices.Length; i++)
        {
            webCams[i] = new WebCamTexture();
            webCams[i] = new WebCamTexture(devices[i].name, Mathf.RoundToInt(requestResolution.x), Mathf.RoundToInt(requestResolution.y), 30);
            textures[i] = webCams[i];
            textures[i].wrapMode = TextureWrapMode.Repeat;

            if (useFrontCam)
            {
                if (devices[i].isFrontFacing) TargetCamID = i;
            }
            else
            {
                if (!devices[i].isFrontFacing) TargetCamID = i;
            }
        }
        if (TargetCamID > devices.Length - 1) TargetCamID = devices.Length - 1;

        //texture = webCams[TargetCamID];
        webCams[TargetCamID].requestedFPS = 30;
        webCams[TargetCamID].Play();

        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].mainTexture = textures[TargetCamID];
        }
        if (BackgroundQuad != null) BackgroundQuad.GetComponent<Renderer>().material.mainTexture = textures[TargetCamID];

        foreach (GameObject obj in targetMeshObjects)
        {
            if(obj.GetComponent<Renderer>() != null) obj.GetComponent<Renderer>().material.mainTexture = textures[TargetCamID];
            if(obj.GetComponent<RawImage>() != null) obj.GetComponent<RawImage>().texture = textures[TargetCamID];
        }

        if (textures.Length > 0)
        {
            webCamTexture = webCams[TargetCamID];
            int initFrameCount = 0;
            //bool isTimeout = false;

            while (webCamTexture.width <= 16)
            {
                if (initFrameCount > timeoutFrameCount)
                {
                    //isTimeout = true;
                    break;
                }
                else
                {
                    initFrameCount++;
                }
                yield return new WaitForEndOfFrame();
            }

            textureResolution = new Vector2(webCamTexture.width, webCamTexture.height);

            //isFlipped = webCamTexture.videoVerticallyMirrored;

            isInitWaiting = false;
        }
        yield return null;
    }
}
