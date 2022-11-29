using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using eDIA;

public class Caller : MonoBehaviour
{
    private void Start() {

        EventManager.TriggerEvent("fire", new eParam(false));
        receiver.Instance.Fire(true);
    }
}
