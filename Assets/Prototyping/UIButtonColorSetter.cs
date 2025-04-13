using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))][ExecuteAlways] // allows component to run in edit-time

public class UIButtonColorSetter : MonoBehaviour
{
    public UIButtonColors colorPalette;

#if UNITY_EDITOR
    private void OnEnable()
    {
        UnityEditor.EditorApplication.update += ApplyColors;
    }
    
    private void OnDisable()
    {
        UnityEditor.EditorApplication.update -= ApplyColors;
    }
#endif

    private void OnValidate()
    {
        ApplyColors();
    }

    public void ApplyColors()
    {
        var button = GetComponent<Button>();

        if (colorPalette != null && button != null)
        {
            button.colors = colorPalette.buttonColorBlock;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(button);
#endif

        }
    }
}