using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TType = Edia.Constants.ThemeComponents;

namespace Edia {
    [ExecuteInEditMode]
    public class ThemeHandler : MonoBehaviour {

        public TType ThemeComponent = TType.Button;

// #if UNITY_EDITOR
        private void OnEnable() {
            Edia.Constants.OnThemeChanged += ApplyTheme;
        }

        private void OnDisable() {
            Edia.Constants.OnThemeChanged -= ApplyTheme;
        }
// #endif

        /*
            Applies color settings from theme file

            Button,
            Toggle,
            Slider,
            Dropdown,
            CTRLPanel,
            Panel,
            SubPanel,
            PrimaryText,
            SecundaryText,
            ThirdText,
            Outlines,
            MgsPanelBG,
            MsgPanelTextBG,
            MsgPanelText,
            ProgressbarBG,
            ProgressbarFill,
            ProgressbarText,
            HorizontalTimer

         */

        public void ApplyTheme() {
            ThemeDefinition activeTheme = Edia.Constants.ActiveTheme;
            if (activeTheme is null) {
                Debug.Log($"{this.name} Theme is null");
                return;
            }

            switch (ThemeComponent) {
                case TType.Button:
                    var button = GetComponent<Button>();
                    if (button is not null) {
                        button.colors = activeTheme.GlobalColorBlock;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(button);
#endif
                    }

                    break;

                case TType.Toggle:
                    var toggle = GetComponent<Toggle>();
                    if (toggle is not null) {
                        toggle.colors = activeTheme.GlobalColorBlock;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(toggle);
#endif
                    }

                    break;
                case TType.Slider:
                    var slider = GetComponent<Slider>();
                    if (slider is not null) {
                        slider.colors = activeTheme.GlobalColorBlock;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(slider);
#endif
                    }

                    break;

                case TType.Dropdown:
                    var dropDown = GetComponent<TMP_Dropdown>();
                    if (dropDown is not null) {
                        dropDown.colors = activeTheme.GlobalColorBlock;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(dropDown);
#endif
                    }

                    break;
                case TType.CTRLPanel:
                    var mainPanelImage = GetComponent<Image>();
                    if (mainPanelImage is not null) {
                        mainPanelImage.color = activeTheme.ControllerPanelColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(mainPanelImage);
#endif
                    }

                    break;

                case TType.Panel:
                    var panelImage = GetComponent<Image>();
                    if (panelImage is not null) {
                        panelImage.color = activeTheme.PanelColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(panelImage);
#endif
                    }

                    break;


                case TType.SubPanel:
                    var subPanelImage = GetComponent<Image>();
                    if (subPanelImage is not null) {
                        subPanelImage.color = activeTheme.SubPanelColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(subPanelImage);
#endif
                    }

                    break;

                case TType.PrimaryText:
                    var primaryText = GetComponent<TextMeshProUGUI>();
                    if (primaryText is not null) {
                        primaryText.color = activeTheme.PrimaryTextColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(primaryText);
#endif
                    }

                    break;

                case TType.SecundaryText:
                    var secundaryText = GetComponent<TextMeshProUGUI>();
                    if (secundaryText is not null) {
                        secundaryText.color = activeTheme.SecundaryTextColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(secundaryText);
#endif
                    }

                    break;
                case TType.TertiaryText:
                    var tertiaryTextColor = GetComponent<TextMeshProUGUI>();
                    if (tertiaryTextColor is not null) {
                        tertiaryTextColor.color = activeTheme.TertiaryTextColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(tertiaryTextColor);
#endif
                    }

                    break;

                case TType.Outlines:
                    var outline = GetComponent<Outline>();
                    if (outline is not null) {
                        outline.effectColor = activeTheme.OutlinesColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(outline);
#endif
                    }

                    break;


                case TType.MgsPanelBG:
                    var msgPanelBG = GetComponent<Image>();
                    if (msgPanelBG is not null) {
                        msgPanelBG.color = activeTheme.MsgPanelColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(msgPanelBG);
#endif
                    }

                    break;

                case TType.MsgPanelTextBG:
                    var msgPanelTextBG = GetComponent<Image>();
                    if (msgPanelTextBG is not null) {
                        msgPanelTextBG.color = activeTheme.MsgPanelTextBGColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(msgPanelTextBG);
#endif
                    }

                    break;

                case TType.MsgPanelText:
                    var msgPanelText = GetComponent<TextMeshProUGUI>();
                    if (msgPanelText is not null) {
                        msgPanelText.color = activeTheme.MsgPanelTextColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(msgPanelText);
#endif
                    }

                    break;

                case TType.ProgressbarBG:
                    var progressbarBG = GetComponent<Image>();
                    if (progressbarBG is not null) {
                        progressbarBG.color = activeTheme.progressBarBGColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(progressbarBG);
#endif
                    }

                    break;
                case TType.ProgressbarFill:
                    var progressbarFill = GetComponent<Image>();
                    if (progressbarFill is not null) {
                        progressbarFill.color = activeTheme.progressBarFillColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(progressbarFill);
#endif
                    }

                    break;
                case TType.ProgressbarText:
                    var progressbarText = GetComponent<TextMeshProUGUI>();
                    if (progressbarText is not null) {
                        progressbarText.color = activeTheme.progressBarTextColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(progressbarText);
#endif
                    }

                    break;

                case TType.HorizontalTimer:
                    var bgImage = GetComponent<Image>();
                    if (bgImage is not null) {
                        bgImage.color = activeTheme.TimerBarBGColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(bgImage);
#endif

                    }

                    var frontImage = transform.GetChild(0).GetComponent<Image>();
                    if (frontImage is not null) {
                        frontImage.color = activeTheme.TimerBarFrontColor;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(frontImage);
#endif
                    }
                    break;
            }
        }
    }
}