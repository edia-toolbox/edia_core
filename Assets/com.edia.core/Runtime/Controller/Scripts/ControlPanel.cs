using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static Edia.Constants;

namespace Edia.Controller {
    public class ControlPanel : Singleton<ControlPanel> {

        [Space(20)] [HideInInspector]
        public ControlModes ControlMode = ControlModes.Local;
        public bool ShowConsoleMessages = false;
        public bool ShowEventMessages   = false;

        [Header("Refs")]
        public Transform NonActivePanelHolder = null;
        public Transform PanelHolder = null;

        // Local
        public  PanelMessageBox          pMessageBox          = null;
        public  PanelConfigSelection     pConfigSelection     = null;
        public  PanelHeader              pHeader              = null;
        public  PanelApplicationSettings pApplicationSettings = null;
        public  PanelExperimentControl   pExperimentControl   = null;
        private List<Transform>          _currentPanelOrder   = new List<Transform>();

        // Remote
        [HideInInspector] public bool IsConnected = false;

        private void Awake() {
            this.transform.SetParent(null);

            DontDestroyOnLoad(this);

            PreparePanels();
            Invoke("Init", 0.3f); // delay init for remote situation in which RCAS will set the `ControlMode` value from awake in `RCAS2Controlpanel`

            EventManager.StartListening(Edia.Events.Core.EvQuitApplication, OnEvQuitApplication);
        }

        private void OnDestroy() {
            EventManager.StopListening(Edia.Events.ControlPanel.EvConnectionEstablished, OnEvConnectionEstablished);
        }

        private void Init() {
            EventManager.showLog = ShowEventMessages; // Show event calls in console for debugging

            if (ControlMode is ControlModes.Remote) {
                EventManager.StartListening(Edia.Events.ControlPanel.EvConnectionEstablished, OnEvConnectionEstablished);
            }
            else
                InitConfigFileSearch();
        }

        private void PreparePanels() {
            // Move all panels from task first to non visuable holder
            foreach (Transform t in PanelHolder) {
                t.SetParent(NonActivePanelHolder, true);
            }

            // Panels renaming
            foreach (Transform tr in NonActivePanelHolder) {
                tr.name = tr.GetSiblingIndex().ToString() + "_" + tr.name;
            }
        }

        private void OnEvConnectionEstablished(eParam obj) {
            EventManager.StopListening(Edia.Events.ControlPanel.EvConnectionEstablished, OnEvConnectionEstablished);

            IsConnected = true;
            InitConfigFileSearch();
        }

        private void InitConfigFileSearch() {
            pConfigSelection.Init();
        }

        public void ShowPanel(Transform panel, bool onOff) {
            panel.SetParent(onOff ? PanelHolder : NonActivePanelHolder, true);
            UpdatePanelOrder();
        }

        public void UpdatePanelOrder() {
            _currentPanelOrder.Clear();
            _currentPanelOrder = PanelHolder.Cast<Transform>().ToList();
            _currentPanelOrder.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });

            for (int i = 0; i < _currentPanelOrder.Count; ++i) {
                _currentPanelOrder[i].SetSiblingIndex(i);
            }
        }

        public void ShowMessage(string msg, bool autoHide) {
            pMessageBox.ShowMessage(msg, autoHide);
        }

        private void OnEvQuitApplication(eParam obj) {
            AddToConsole(($"{name}:Quiting.."));
            Invoke("DoQuit", 1f);
        }

        private void DoQuit() {
            AddToConsole(($"{name}:Bye.."));
            Application.Quit();
        }

        public void AddToConsole(string msg) {
            if (ShowConsoleMessages)
                Edia.LogUtilities.AddToConsoleLog(msg, this.name);
        }

        public void AddToConsole(string msg, LogType _type) {
            if (_type == LogType.Error) Debug.LogError(msg);
            else if (_type == LogType.Warning) Debug.LogWarning(msg);
            else AddToConsole(msg, _type);
        }
    }
}