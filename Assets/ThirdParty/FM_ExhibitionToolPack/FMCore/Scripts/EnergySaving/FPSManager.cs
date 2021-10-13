using UnityEngine;
using UnityEngine.UI;

public class FPSManager : MonoBehaviour
{

    public Text fpsText;
    float deltaTime;

    public int targetFps = 30;

    [Tooltip("ignore targetFPS when vSyncCount > 0")]
    public int vSyncCount = 0;

    float UpdateTextTimer = 0f;
    float UpdateTextThreshold = 0.2f;
    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (QualitySettings.vSyncCount != vSyncCount) QualitySettings.vSyncCount = vSyncCount;
        if (Application.targetFrameRate != targetFps) Application.targetFrameRate = targetFps;
#endif

        if (fpsText == null) return;
        
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        //if (Time.frameCount % 5 == 0)
        //{
        //    float fps = 1.0f / deltaTime;
        //    fpsText.text = "FPS: " + Mathf.Ceil(fps).ToString();
        //}

        UpdateTextTimer += Time.deltaTime;
        if (UpdateTextTimer > UpdateTextThreshold)
        {
            UpdateTextTimer = 0f;
            float fps = 1.0f / deltaTime;
            fpsText.text = "FPS: " + Mathf.Ceil(fps).ToString();
        }
    }

    public void Action_SetFPS(int _fps)
    {
        targetFps = _fps;
    }
}