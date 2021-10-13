using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoObjectAnimation : MonoBehaviour {

    Vector3 startPos;
    public bool mx=true;
    public bool my=true;
    public bool mz=true;

    public Vector3 speed = new Vector3(1f,1f,1f);
    public Vector3 frequency = new Vector3(1f, 1f, 1f);

    public Color color=Color.red;
	// Use this for initialization
	void Start () {
        startPos = transform.position;
        GetComponent<Renderer>().material.color = color;
	}
	
	// Update is called once per frame
	void Update () {

        Vector3 dir=Vector3.zero;
        dir += (mx == true ? 1 : 0) * Mathf.Sin(Time.realtimeSinceStartup * Mathf.PI * frequency.x) * speed.x * transform.right;
        dir += (my == true ? 1 : 0) * Mathf.Sin(Time.realtimeSinceStartup * Mathf.PI * frequency.y) * speed.y * transform.up;
        dir += (mz == true ? 1 : 0) * Mathf.Sin(Time.realtimeSinceStartup * Mathf.PI * frequency.z) * speed.z * transform.forward;
        //transform.Translate(dir);

        transform.position = startPos + dir;

	}
}
