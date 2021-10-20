using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffScreenTrigger : MonoBehaviour
{
    WorldToScreenSpace[] WSSs;
    // Start is called before the first frame update
    void Start()
    {
        WSSs = transform.parent.GetComponentsInChildren<WorldToScreenSpace>(true);
        foreach (WorldToScreenSpace WSS in WSSs)
        {
            WSS.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Action_offscreen()
    {
        foreach (WorldToScreenSpace WSS in WSSs)
        {
            WSS.InvokeOffScreen();
        }
    }
}
