using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TType = Edia.Constants.ThemeComponents;

namespace Edia {
    [ExecuteInEditMode]
    public class ThemeHandler : MonoBehaviour {

        public TType ThemeComponent = TType.Button;

#if UNITY_EDITOR
        private void OnEnable() {
            Edia.Constants.OnThemeChanged += ApplyTheme;
            
        }

        private void OnDisable() {
            Edia.Constants.OnThemeChanged -= ApplyTheme;
        }

#endif

        /*
         	Button,
            ButtonText,
            Toggle,
            Slider,
            Dropdown,
            MainBG,
            MainText,
            PanelBG,
            PanelText,
            SubPanelBG,
            SubPanelText,
            Outlines,
            MessagePanelBG,
            MessagePanelTextPanelBG,
            MessagePanelText,
            ProgressbarBG,
            ProgressbarFill,
            ProgressbarText,
            HorizontalTimer,
            
         */
        
        
        // private void OnValidate() {
        //     ApplyTheme();
        // }

        public void ApplyTheme() {
            ThemeDefinition activeTheme = Edia.Constants.ActiveTheme;
            if (activeTheme is null) {
                return;
            }

            switch (ThemeComponent) {

#region ------ Button Colors ------

                case TType.Button:
                    var button = GetComponent<Button>();
                    if (button != null) {
                        button.colors = activeTheme.ButtonColorBlock;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(button);
#endif
                    }

                    break;

                case TType.ButtonText:
                    var buttonText = GetComponent<TextMeshProUGUI>();
                    if (buttonText != null) {
                        buttonText.color = activeTheme.ButtonTextColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(buttonText);
#endif
                    }

                    break;
                
                case TType.Outline:
                    var outline = GetComponent<Outline>();
                    if (outline != null) {
                        outline.effectColor = activeTheme.ButtonOutlineColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(outline);
#endif
                    }

                    break;

#endregion
#region ------ Toggle Colors ------

                case TType.Toggle:
                    var toggle = GetComponent<Toggle>();
                    if (toggle != null) {
                        toggle.colors = activeTheme.ToggleColorBlock;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(toggle);
#endif
                    }

                    break;

#endregion
#region ------ Dropdown Colors ------

                case TType.Dropdown:
                    var dropDown = GetComponent<Dropdown>();
                    if (dropDown != null) {
                        dropDown.colors = activeTheme.DropDownColorBlock;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(dropDown);
#endif
                    }

                    break;

#endregion
#region ------ Progress bar Colors ------

                case TType.ProgressbarBG:
                    var progressbarBG = GetComponent<Image>();
                    if (progressbarBG != null) {
                        progressbarBG.color = activeTheme.progressBarBGColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(progressbarBG);
#endif
                    }

                    break;
                case TType.ProgressbarFill:
                    var progressbarFill = GetComponent<Image>();
                    if (progressbarFill != null) {
                        progressbarFill.color = activeTheme.progressBarFillColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(progressbarFill);
#endif
                    }

                    break;
                case TType.ProgressbarText:
                    var progressbarText = GetComponent<TextMeshProUGUI>();
                    if (progressbarText != null) {
                        progressbarText.color = activeTheme.progressBarTextColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(progressbarText);
#endif
                    }

                    break;

#endregion
#region ------ Main Panel Colors ------

                case TType.MainBG:
                    var mainPanelImage = GetComponent<Image>();
                    if (mainPanelImage != null) {
                        mainPanelImage.color = activeTheme.MainBGColor;
                    }
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(mainPanelImage);
#endif
                    break;
                
                case TType.MainText:
                    var mainTextColor = GetComponent<TextMeshProUGUI>();
                    if (mainTextColor != null) {
                        mainTextColor.color = activeTheme.MainPanelTextColor;
                    }
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(mainTextColor);
#endif
                    break;

#endregion
#region ------ Panel Colors ------

                case TType.PanelBG:
                    var panelImage = GetComponent<Image>();
                    if (panelImage != null) {
                        panelImage.color = activeTheme.PanelBGColor;
                    }
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(panelImage);
#endif
                    break;

                case TType.PanelText:
                    var panelTextColor = GetComponent<TextMeshProUGUI>();
                    if (panelTextColor != null) {
                        panelTextColor.color = activeTheme.PanelTextColor;
                    }
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(panelTextColor);
#endif
                    break;

#endregion
#region ------ Horizontal Timer ------

                case TType.HorizontalTimer:
                    var bgImage = GetComponent<Image>();
                    if (bgImage is not null) {
                        bgImage.color = activeTheme.TimerBarBGColor;
                    }
                    
                    var frontImage = transform.GetChild(0).GetComponent<Image>();
                    if (frontImage is not null) {
                        frontImage.color = activeTheme.TimerBarFrontColor;
                    }
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(bgImage);
                    UnityEditor.EditorUtility.SetDirty(frontImage);
#endif
                    break;

#endregion
#region ------ SubPanel Colors ------

                case TType.SubPanelBG:
                    var subPanelImage = GetComponent<Image>();
                    if (subPanelImage != null) {
                        subPanelImage.color = activeTheme.SubPanelBGColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(subPanelImage);
#endif
                    }

                    break;

                case TType.SubPanelText:
                    var subPanelTextColor = GetComponent<TextMeshProUGUI>();
                    if (subPanelTextColor != null) {
                        subPanelTextColor.color = activeTheme.SubPanelTextColor;
                    }
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(subPanelTextColor);
#endif
                    break;

#endregion
#region ------ MessagePanel Colors ------

                case TType.MessagePanelBG:
                    var msgPanelBG = GetComponent<Image>();
                    if (msgPanelBG != null) {
                        msgPanelBG.color = activeTheme.MsgPanelBGColor;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(msgPanelBG);
#endif
                    }

                    break;

                case TType.MessagePanelTextPanelBG:
                    var msgPanelTextBG = GetComponent<Image>();
                    if (msgPanelTextBG != null) {
                        msgPanelTextBG.color = activeTheme.MsgPanelTextBGColor;
                    }
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(msgPanelTextBG);
#endif
                    break;
                
                case TType.MessagePanelText:
                    var msgPanelText = GetComponent<TextMeshProUGUI>();
                    if (msgPanelText != null) {
                        msgPanelText.color = activeTheme.MsgPanelTextColor;
                    }
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(msgPanelText);
#endif
                    break;
#endregion            
                
            }
        }
    }
}