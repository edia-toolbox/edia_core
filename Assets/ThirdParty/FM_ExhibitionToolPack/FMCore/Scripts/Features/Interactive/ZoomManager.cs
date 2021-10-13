using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomManager : MonoBehaviour
{
    public Camera cam;
    public float miniFov = 10f;
    public float maxFov = 60f;
    float fov;
    Touch[] fingers;
    float NDistStart = 0;
    float NDistNow = 0;
    float NDistDelta = 0;
    float fovStart;
    bool startZoom = false;

    // Start is called before the first frame update
    void Start()
    {
        if (cam == null) cam = Camera.main;

        fov = cam.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        GestureZoom();

        fov = Mathf.Clamp(fov, miniFov, maxFov);
        cam.fieldOfView = fov;

    }

    void GestureZoom()
    {
        if (Input.touchCount >= 2)
        {
            fingers = Input.touches;

            if (fingers[1].phase == TouchPhase.Began)
            {

            }
            else if (fingers[1].phase == TouchPhase.Moved)
            {

                if (!startZoom)
                {
                    startZoom = true;
                    fovStart = fov;
                    Vector2 pos1 = new Vector2((float)fingers[0].position.x / (float)Screen.width, (float)fingers[0].position.y / (float)Screen.height);
                    Vector2 pos2 = new Vector2((float)fingers[1].position.x / (float)Screen.width, (float)fingers[1].position.y / (float)Screen.height);
                    NDistStart = Vector2.Distance(pos1, pos2);
                }
                else
                {
                    Vector2 pos1 = new Vector2((float)fingers[0].position.x / (float)Screen.width, (float)fingers[0].position.y / (float)Screen.height);
                    Vector2 pos2 = new Vector2((float)fingers[1].position.x / (float)Screen.width, (float)fingers[1].position.y / (float)Screen.height);
                    NDistNow = Vector2.Distance(pos1, pos2);

                    NDistDelta = NDistNow - NDistStart;
                    fov = fovStart - NDistDelta * 30f;
                }
            }

        }
        else
        {
            startZoom = false;
        }
    }

    public void ZoomIn()
    {
        fov -= Time.deltaTime * 10f;
    }

    public void ZoomOut()
    {
        fov += Time.deltaTime * 10f;
    }
}
