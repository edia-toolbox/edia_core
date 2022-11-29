using System.Diagnostics.Tracing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using eDIA;
using System;

public class receiver : Singleton<receiver>
{
    public child c;

    public void Fire (bool onOff) {
        c.aMethod(onOff);
    }
}
