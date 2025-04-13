using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StyleEditor : EditorWindow {
    public Sprite ProjectLogoSprite;
    
    // TODO Default EDIA color settings for UI
    ColorBlock defaultColorBlock = new ColorBlock {
        normalColor      = new Color32(255, 255, 255, 255),
        highlightedColor = new Color32(245, 245, 245, 255),
        pressedColor     = new Color32(200, 200, 200, 255),
        selectedColor    = new Color32(245, 245, 245, 255),
        disabledColor    = new Color32(200, 200, 200, 128),
        colorMultiplier  = 1.0f,
        fadeDuration     = 0.1f
    };

    // Custom editable color settings for user
    public ColorBlock ProjectColorBlock = new ColorBlock {
        normalColor      = new Color32(255, 255, 255, 255),
        highlightedColor = new Color32(245, 245, 245, 255),
        pressedColor     = new Color32(200, 200, 200, 255),
        selectedColor    = new Color32(245, 245, 245, 255),
        disabledColor    = new Color32(200, 200, 200, 128),
        colorMultiplier  = 1.0f,
        fadeDuration     = 0.1f
    };

    // TODO all other panel color etc
    /*
        Controller:
            - Main Panel bg
            - sub panel bg
            - 
    
    */
    
    [MenuItem("EDIA/StyleEditor")]
    public static void ShowWindow() {
        GetWindow<StyleEditor>("StyleEditor");
    }

    private void OnGUI() {
        GUILayout.Label("Project UI style editor", EditorStyles.boldLabel);
    
        ProjectLogoSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", ProjectLogoSprite, typeof(Sprite), false);
        
        ProjectColorBlock.normalColor      = EditorGUILayout.ColorField("Normal Color", ProjectColorBlock.normalColor);
        ProjectColorBlock.highlightedColor = EditorGUILayout.ColorField("Highlighted Color", ProjectColorBlock.highlightedColor);
        ProjectColorBlock.pressedColor     = EditorGUILayout.ColorField("Pressed Color", ProjectColorBlock.pressedColor);
        ProjectColorBlock.selectedColor    = EditorGUILayout.ColorField("Selected Color", ProjectColorBlock.selectedColor);
        ProjectColorBlock.disabledColor    = EditorGUILayout.ColorField("Disabled Color", ProjectColorBlock.disabledColor);

        if (GUILayout.Button("Apply Color to All Buttons")) {
            ApplyColorToButtons(ProjectColorBlock);
        }

        if (GUILayout.Button("Reset to Default Color Block")) {
            ApplyColorToButtons(defaultColorBlock);
        }
    }

    private void ApplyColorToButtons(ColorBlock colorBlock) {
        Button[] buttons = GetAllButtonsInScene();

        foreach (Button button in buttons) {
            Undo.RecordObject(button, "Apply Button Colors");
            ColorBlock colors = button.colors;
            colors.normalColor      = colorBlock.normalColor;
            colors.highlightedColor = colorBlock.highlightedColor;
            colors.pressedColor     = colorBlock.pressedColor;
            colors.selectedColor    = colorBlock.selectedColor;
            colors.disabledColor    = colorBlock.disabledColor;
            button.colors           = colors;
            EditorUtility.SetDirty(button);
        }

        Debug.Log($"Applied colors to {buttons.Length} buttons.");
    }

    private Button[] GetAllButtonsInScene() {
        return SceneManager.GetActiveScene()
            .GetRootGameObjects()
            .SelectMany(g => g.GetComponentsInChildren<Button>(true))
            .ToArray();
    }
}