using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkActions_Debug : MonoBehaviour {

    public Text text;

    public void Action_TextUpdate(string _value)
    {
        text.text = _value;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
