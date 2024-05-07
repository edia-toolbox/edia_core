using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILslTimer {
    public double GetTime();
}

public interface ILslPusher {
    public void PushSample(float posX, float posY, float posZ,
        float rotX, float rotY, float rotZ,
        float pupilDiameter = 0f,
        float confidence = 0f,
        float timestampEt = 0f,
        double timestampLsl = 0);
}
