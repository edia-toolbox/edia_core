using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Events;

public class WorldToScreenSpace : MonoBehaviour
{
    public Transform reference;
    public RectTransform[] targetRect;

    [Header("When target on screen")]
    public UnityEvent OnScreenEvent;
    [Header("When target off screen")]
    public UnityEvent OffScreenEvent;

    // Start is called before the first frame update
    void Start()
    {
        OffScreenEvent.Invoke();
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 SPos = Camera.main.WorldToScreenPoint(reference.position);
        //targetRect.position = SPos;
        foreach(RectTransform _rect in targetRect)
        {
            _rect.position = SPos;
        }


        if (SPos.z < 0 || SPos.x < -(float)Screen.width/2f || SPos.x > (float)Screen.width *3f/ 2f || SPos.y < -(float)Screen.height/2f || SPos.y > (float)Screen.height*3f/2f)
        //if (SPos.z < 0 )
        {
            //targetRect.gameObject.SetActive(false);
            OffScreenEvent.Invoke();
        }
        else
        {
            //targetRect.gameObject.SetActive(true);
            OnScreenEvent.Invoke();
        }

        //print(SPos + " : " + reference.position);

    }

    public void InvokeOnScreen()
    {
        OnScreenEvent.Invoke();
    }
    public void InvokeOffScreen()
    {
        OffScreenEvent.Invoke();
    }

    void OnDisable()
    {
        OffScreenEvent.Invoke();
    }

    void OnEnable()
    {

    }

    public void DebugTest(string _text)
    {
        print("Trigger Debug: " + _text);
    }

}
