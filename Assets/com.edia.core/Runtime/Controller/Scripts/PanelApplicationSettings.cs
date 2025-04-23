using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Edia.Constants;

namespace Edia.Controller {
    public class PanelApplicationSettings : ExperimenterPanel {
        [Header("Refs")]
        public Button btnApply = null;

        public Button          btnClose                = null;
        public Button          btnBrowse               = null;
        public Button          btnQuit                 = null;
        public GameObject      panelFilePath           = null;
        public TMP_Dropdown    interactiveSideDropdown = null;
        public TextMeshProUGUI pathToLogfilesField     = null;

        [HideInInspector] public SettingsDeclaration localSystemSettingsContainer = null;

#region INITS

        public override void Awake() {
            base.Awake();

            HidePanel();
            SetupPanels();

            EventManager.StartListening(Edia.Events.Settings.EvOpenSystemSettings, OnEvOpenSystemSettings);
        }

        private void SetupPanels() {
            btnApply.onClick.AddListener(() => BtnApplyPressed());
            btnClose.onClick.AddListener(() => HidePanel());
            btnBrowse.onClick.AddListener(() => OpenFileBrowser());
            btnQuit.onClick.AddListener(() => BtnQuitPressed());

            PopulateInteractivesDropdown();
        }

        void OnDestroy() {
            EventManager.StopListening(Edia.Events.Settings.EvOpenSystemSettings, OnEvOpenSystemSettings);
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region SETTINGS HANDLING

        private void OnEvOpenSystemSettings(eParam obj) {
            // System settings should provide its settings package to the controller.
            EventManager.StartListening(Edia.Events.Settings.EvProvideSystemSettings, OnProcessSystemSettings); // listen to package 
            EventManager.TriggerEvent(Edia.Events.Settings.EvRequestSystemSettings); // trigger sending the package
        }

        /// <summary>
        /// Processes given JSON string with Edia-settings, shows panel with updated info
        /// </summary>
        /// <param name="obj">Edia-settings package as JSON string</param>
        private void OnProcessSystemSettings(eParam obj) {
            EventManager.StopListening(Edia.Events.Settings.EvProvideSystemSettings, OnProcessSystemSettings);

            localSystemSettingsContainer = UnityEngine.JsonUtility.FromJson<SettingsDeclaration>(obj.GetString());

            interactiveSideDropdown.value = GetDropdownOptionIndexByStringValue(localSystemSettingsContainer.InteractiveSide);
            pathToLogfilesField.text      = localSystemSettingsContainer.pathToLogfiles;

            btnApply.interactable = false;

            ShowPanel();

            panelFilePath.SetActive(ControlPanel.Instance.ControlMode == ControlModes.Local);
        }

        private int GetDropdownOptionIndexByStringValue(string value) {
            return interactiveSideDropdown.options.FindIndex(x => x.text == value);
        }

        private void UpdateLocalSettings() {
            localSystemSettingsContainer.InteractiveSide = interactiveSideDropdown.options[interactiveSideDropdown.value].text;
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

#region UI

        private void PopulateInteractivesDropdown() {
            interactiveSideDropdown.ClearOptions();

            List<TMP_Dropdown.OptionData> options   = new List<TMP_Dropdown.OptionData>();
            TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData("LEFT");
            options.Add(newOption);
            newOption = new TMP_Dropdown.OptionData("RIGHT");
            options.Add(newOption);
            newOption = new TMP_Dropdown.OptionData("BOTH");
            options.Add(newOption);
            newOption = new TMP_Dropdown.OptionData("NONE");
            options.Add(newOption);

            interactiveSideDropdown.AddOptions(options);
        }

        private void InteractiveValueChanged() {
            btnApply.interactable = true;
        }

        // Something has changed

        private void BtnApplyPressed() {
            UpdateLocalSettings();

            // Sent updated settings package to systemsettings, which would handle the changed values.
            EventManager.TriggerEvent(Edia.Events.Settings.EvUpdateSystemSettings,
                new eParam(UnityEngine.JsonUtility.ToJson(localSystemSettingsContainer, false)));
        }

        private void BtnQuitPressed() {
            Debug.Log($"{name}:Quit request sent");
            EventManager.TriggerEvent(Edia.Events.Core.EvQuitApplication);
        }

        private void OpenFileBrowser() {
            StartCoroutine(ShowLoadDialogCoroutine());
        }

        private IEnumerator ShowLoadDialogCoroutine() {
            yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, null, null, "Select Folder", "Select");

            if (FileBrowser.Success) {
                if (FileBrowser.Result[0] != localSystemSettingsContainer.pathToLogfiles) {
                    localSystemSettingsContainer.pathToLogfiles = FileBrowser.Result[0];
                    Debug.Log(localSystemSettingsContainer.pathToLogfiles);
                    pathToLogfilesField.text = FileBrowser.Result[0];
                }
            }
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------

    }
}