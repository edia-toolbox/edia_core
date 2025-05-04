#if UNITY_EDITOR
using Edia;
using UnityEditor;

[InitializeOnLoad]
public static class ApplyThemeOnPlay
{
    static ApplyThemeOnPlay()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode) {
            Constants.UpdateTheme();
        }
    }
}
#endif