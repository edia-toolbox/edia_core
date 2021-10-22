using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using eDIA;

public class UserMessageHandler : MonoBehaviour
{
    public TextMeshProUGUI msgField = null;

    void Start() {
        EventManager.StartListening("EvShowMessage", OnEvShowMessage);
    }

    void OnDestroy() {
        EventManager.StopListening("EvShowMessage", OnEvShowMessage);
    }    
    
    public void OnEvShowMessage (eParam e) {
        if (msgField == null) 
            return;

        msgField.text = e.GetString();
    }
}
