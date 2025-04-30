using UnityEngine;

public class HeaderTest : MonoBehaviour
{
    [Header("Header Test")]
    [InspectorHeader("EDIA TEST", "Testing height", "some form of description")]
    public int test;

    public string something;
    
}
