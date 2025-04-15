using UnityEngine;
using UnityEngine.UI;
using TType = Edia.Constants.ThemeComponents;

namespace Edia {
    [ExecuteInEditMode]
    public class ThemeHandler : MonoBehaviour {

        public TType ThemeComponent = TType.Button;

        // Keep UNITY_EDITOR as needed, but make sure the events work
#if UNITY_EDITOR
        private void OnEnable() {
            Edia.Constants.OnThemeChanged += ApplyTheme;
            ApplyTheme();
        }

        private void OnDisable() {
            Edia.Constants.OnThemeChanged -= ApplyTheme;
        }

#endif

        private void OnValidate() {
            ApplyTheme();
        }

        public void ApplyTheme() {

            ColorThemeDefinition activeTheme = Edia.Constants.ActiveTheme;
            if (activeTheme == null) {
                Debug.LogWarning("No active theme available");
                return;
            }

            switch (ThemeComponent) {
                case TType.Button:
                    var button = GetComponent<Button>();
                    if (button != null) {
                        button.colors = activeTheme.ButtonColorBlock;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(button);
#endif
                    }

                    break;
                // Other cases remain the same...
                case TType.ControlPanelBG:
                    var controlPanelImage = GetComponent<Image>();
                    if (controlPanelImage != null) {
                        controlPanelImage.color = activeTheme.ControlPanelBackgroundColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(controlPanelImage);
#endif
                    }

                    break;
                case TType.ItemPanelBG:
                    var itemPanelImage = GetComponent<Image>();
                    if (itemPanelImage != null) {
                        itemPanelImage.color = activeTheme.ItemPanelBackgroundColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(itemPanelImage);
#endif
                    }

                    break;
                case TType.SubPanelBG:
                    var subPanelImage = GetComponent<Image>();
                    if (subPanelImage != null) {
                        subPanelImage.color = activeTheme.SubPanelBackgroundColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(subPanelImage);
#endif
                    }

                    break;
                // Handle other types...
            }
        }
    }
}