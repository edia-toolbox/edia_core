using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMNetwork_DemoAnimation : MonoBehaviour {

    public GameObject Box1;
    public GameObject Box2;
    public Vector3 StartPosBox1;
    public Vector3 StartPosBox2;

    // Use this for initialization
    void Start()
    {
        if (Box1 != null) StartPosBox1 = Box1.transform.position;
        if (Box2 != null) StartPosBox2 = Box2.transform.position;
    }
    private void Update()
    {
        if (Box1 != null && Box2 != null)
        {
            Box1.transform.Rotate(new Vector3(0f, Time.deltaTime * 15f, 0f));
            Box1.transform.position = new Vector3(Mathf.Sin(Time.realtimeSinceStartup), 0f, 0f) + StartPosBox1;
            Box1.transform.localScale = new Vector3(1f, 1f, 1f) * (1f + 0.5f * Mathf.Sin(Time.realtimeSinceStartup * 3f));

            Box2.transform.Rotate(new Vector3(0f, Time.deltaTime * 30f, 0f));
            Box2.transform.position = new Vector3(Mathf.Sin(Time.realtimeSinceStartup * 0.5f), 0f, -1f) + StartPosBox2;
            Box2.transform.localScale = new Vector3(1f, 1f, 1f) * (1f + 0.5f * Mathf.Sin(Time.realtimeSinceStartup * 2f));
        }
    }
}
