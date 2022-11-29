using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using eDIA;



public class child : MonoBehaviour
{
    public GameObject cube;

    private void Awake() {
        EventManager.StartListening("fire", OnFire);
    }

    private void OnDestroy() {
        EventManager.StopListening("fire", OnFire);
    }

    private void OnFire(eParam obj)
	{
        aMethod (obj.GetBool());
	}

    public void aMethod (bool onOff) {
        Debug.Log("aMethod called");
        cube.SetActive(onOff);
    }
}
