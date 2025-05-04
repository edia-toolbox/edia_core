using Edia;
using UnityEngine;

public class themebla : MonoBehaviour
{
    private void Awake() {
        Debug.Log($"Awake: Playersettings theme value: {PlayerPrefs.GetString(Constants.THEME_PATH_KEY)}");
        ThemeDefinition loadedTheme = Resources.Load<ThemeDefinition>(PlayerPrefs.GetString(Constants.THEME_PATH_KEY));
        Constants.ActiveTheme = loadedTheme;
        Constants.UpdateTheme();
    }
}
