using System.Threading.Tasks;
using UnityEngine;
using UXF;

namespace Edia {
    /// <summary>Global settings of the application</summary>
    public class SystemSettings : Singleton<SystemSettings> {
#region DECLARATIONS

        /// <summary>Instance of the Settings declaration class in order to (de)serialize to JSON</summary>
        [HideInInspector] public SettingsDeclaration Settings = new();
        private SettingsDeclaration receivedSettings = new();
        static UXF.LocalFileDataHander UXFFilesaver = null;

        [HideInInspector] public bool isRemote = false;

        private void Awake() {
            InitSystemSettings();
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region MAIN METHODS

        private void InitSystemSettings() {
            UXFFilesaver = GameObject.FindFirstObjectByType<UXF.LocalFileDataHander>();
            isRemote     = !FindFirstObjectByType<Edia.Controller.ControlPanel>();

            EventManager.StartListening(Edia.Events.Settings.EvUpdateSystemSettings, OnEvUpdateSystemSettings);
            EventManager.StartListening(Edia.Events.Settings.EvRequestSystemSettings, OnEvRequestSystemSettings);

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            LoadSettings();
        }

        private void SaveSettings() {
            FileManager.WriteString(Constants.FileNameEdiaSettings, UnityEngine.JsonUtility.ToJson(Settings, true), true);
        }

        private async void LoadSettings() {
            if (!FileManager.FileExists(Constants.FileNameEdiaSettings)) {
                Debug.Log("Settings file not found, saving defaults");
                SaveSettings();
            }

            string loadedSettings = FileManager.ReadStringFromApplicationPath(Constants.FileNameEdiaSettings);
            await Task.Delay(500);

            UXFFilesaver.storagePath      = Settings.pathToLogfiles;
            UXFFilesaver.dataSaveLocation = isRemote ? DataSaveLocation.PersistentDataPath : DataSaveLocation.Fixed;

            //! Locally
            OnEvUpdateSystemSettings(new eParam(loadedSettings));
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region EVENT LISTENERS

        /// <summary> Received new settings, apply them on the executer side </summary>
        private void OnEvUpdateSystemSettings(eParam obj) {
            receivedSettings = new SettingsDeclaration();
            receivedSettings = UnityEngine.JsonUtility.FromJson<SettingsDeclaration>(obj.GetString());

            Settings.InteractiveSide = receivedSettings.InteractiveSide;
            EventManager.TriggerEvent(Edia.Events.XR.EvUpdateInteractiveSide, null);

            Settings.pathToLogfiles = receivedSettings.pathToLogfiles;

            if (isRemote)
                UXFFilesaver.storagePath = Application.dataPath;
            else
                UXFFilesaver.storagePath = Settings.pathToLogfiles;

            SaveSettings();
        }

        /// <summary> Catches request to show system settings, collects them and send them out with a OPEN settings panel event. </summary>
        private void OnEvRequestSystemSettings(eParam obj) {
            EventManager.TriggerEvent(Edia.Events.Settings.EvProvideSystemSettings, new eParam(GetSettingsAsJSONstring()));
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
#region HELPERS

        /// <summary>Gets all settings from the 'SettingsDeclaration' instance 'systemSettings' as a JSON string</summary>
        /// <returns>JSON string</returns>
        private string GetSettingsAsJSONstring() {
            return UnityEngine.JsonUtility.ToJson(Settings, false);
        }

        public void AddToConsole(string _msg) {
            if (Experiment.Instance.ShowConsoleMessages)
                Edia.Utilities.Log.AddToConsoleLog(_msg, "SystemSettings");
        }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
    }
}