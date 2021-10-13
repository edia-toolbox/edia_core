using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class DemoScreenshot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveScreenshot();
        }
#endif
    }

    int order = 0;
    void SaveScreenshot()
    {
        string path = Directory.GetParent(Application.dataPath).ToString()+"/Screenshots/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string SavePath = path + SceneManager.GetActiveScene().name + order + ".png";
        ScreenCapture.CaptureScreenshot(SavePath);
        order++;
        print(SavePath);
    }
}
