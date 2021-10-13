using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager_helper : MonoBehaviour
{
    public void Action_LoadSceneName(string _scene)
    {
        SceneManager.LoadScene(_scene);
    }

    public void Action_Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}