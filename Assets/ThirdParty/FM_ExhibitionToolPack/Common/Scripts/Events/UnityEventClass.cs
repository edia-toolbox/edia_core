using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class UnityEventFloat: UnityEvent<float> { }
[System.Serializable]
public class UnityEventInt : UnityEvent<int> { }
[System.Serializable]
public class UnityEventBool : UnityEvent<bool> { }
[System.Serializable]
public class UnityEventString : UnityEvent<string> { }

[System.Serializable]
public class UnityEventByteArray: UnityEvent<byte[]> { }
[System.Serializable]
public class UnityEventTexture2D : UnityEvent<Texture2D> { }

public class UnityEventClass : MonoBehaviour
{

}
