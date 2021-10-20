using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillCameraBackground : MonoBehaviour
{
    public GameObject quad;
    public Camera mainCam;

    float CW = 1f;
    float CH = 1f;
    float CAspect;

    float TW = 1f;
    float TH = 1f;
    float TAspect = 1f;
    float ScaleMag = 1f;

    Vector3 quadScale;

    [Range(0.0f, 1.0f)]
    public float Dist = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float fov = mainCam.fieldOfView;
        float nearClipplane = mainCam.nearClipPlane;
        float farClipplane = mainCam.farClipPlane;
        float targetDist = nearClipplane + (farClipplane-nearClipplane)*Dist;
        //float CAspect = mainCam.aspect;
        CAspect = (float)Screen.width / (float)Screen.height;

        CH = Mathf.Tan((fov / 2f) * Mathf.Deg2Rad) * targetDist * 2f;
        CW = CAspect*CH;

        ScaleMag = 1f;

        if (quad.GetComponent<Renderer>().material.mainTexture != null)
        {
            TW = quad.GetComponent<Renderer>().material.mainTexture.width;
            TH = quad.GetComponent<Renderer>().material.mainTexture.height;
        }
        TAspect = TW / TH;

        if(TAspect > CAspect)
        {
            ScaleMag = (TAspect / CAspect);
            quadScale = new Vector3(CW * ScaleMag, CH, 1f);
        }
        else
        {
            ScaleMag = (CAspect / TAspect);
            quadScale = new Vector3(CW, CH * ScaleMag, 1f);
        }

        quad.transform.position = mainCam.transform.position + mainCam.transform.forward * targetDist;
        quad.transform.localScale = quadScale;
    }
}
