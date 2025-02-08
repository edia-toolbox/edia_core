using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Edia {
    /// <summary>Show the user a message in VR</summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(TrackedDeviceGraphicRaycaster))]
    public class MessagePanelInVR : MonoBehaviour {
        [Header("Settings")]
        [Tooltip("Auto orientates itself in front of user. Draws on top of the 3D environment.")]
        [SerializeField] private bool _stickToHMD = false;
        [SerializeField] private float _distanceFromHMD = 2f;

        [Space(20)] [Header("Refs")]
        [SerializeField] private TextMeshProUGUI _msgField = null;
        [SerializeField] private GameObject _menuHolder = null;
        [SerializeField] private Button _buttonNEXT = null;
        [SerializeField] private Button _buttonProceed = null;
        [SerializeField] private bool _hasSolidBackground = true;
        
        // Locals
        private Image _backgroundImg = null;
        private bool _hasClicked = false;
        private List<string> _messageQueue = new();

        private Coroutine _messageTimer = null;
        private Coroutine _messagePanelFader = null;
        private Coroutine _messageTextFader = null;
        private Coroutine _messagesRoutine = null;
        private Canvas _canvas;
        private GraphicRaycaster _graphicRaycaster;
        private TrackedDeviceGraphicRaycaster _trackedDeviceGraphicRaycaster;
        private Transform[] _panelChildren;
        
        // Singleton
        private static MessagePanelInVR instance = null;
        public static MessagePanelInVR Instance {
            get {
                if ((object)instance == null) {
                    instance = (MessagePanelInVR)FindObjectOfType(typeof(MessagePanelInVR));

                    if (instance == null) {
                        GameObject singletonObject = new GameObject(typeof(MessagePanelInVR).ToString());
                        instance = singletonObject.AddComponent<MessagePanelInVR>();
                    }
                }

                return instance;
            }
        }

        // ---

        private void Awake() {
            _trackedDeviceGraphicRaycaster = GetComponent<TrackedDeviceGraphicRaycaster>();
            
            _canvas = GetComponent<Canvas>();
            _canvas.worldCamera = _stickToHMD ? XRManager.Instance.CamOverlay.GetComponent<Camera>() : XRManager.Instance.XRCam.GetComponent<Camera>();
            _backgroundImg = transform.GetChild(0).GetComponent<Image>();
            
            _panelChildren = this.gameObject.GetComponentsInChildren<Transform>(true);
            
            if (GetComponent<GraphicRaycaster>() != null)
                _graphicRaycaster = GetComponent<GraphicRaycaster>();

            if (_stickToHMD) {
                transform.parent = XRManager.Instance.XRCam.transform;
                transform.localPosition = new Vector3(0, 0, _distanceFromHMD);
                transform.localRotation = Quaternion.identity;

                foreach (var child in _panelChildren) {
                    child.gameObject.layer = LayerMask.NameToLayer("CamOverlay");    
                }
            }
        }

        private void Start() {
            EventManager.StartListening(Edia.Events.StateMachine.EvProceed, OnEvHideMessage);
        }

        private void OnDestroy() {
            EventManager.StopListening(Edia.Events.StateMachine.EvProceed, OnEvHideMessage);
        }


        #region MESSAGE OPTIONS

        /// <summary>Shows the message in VR on a canvas for a certain duration.</summary>
        /// <param name="msg">Message to show</param>
        /// <param name="duration">Duration</param>
        public void ShowMessage(string msg, float duration) {
            ShowMessage(msg);
            _messageTimer = StartCoroutine(HidePanelAfter(duration));
            HideMenu();
        }

        /// <summary>Shows one message including proceed button</summary>
        /// <param name="msg"></param>
        public void ShowMessage(string msg) {
            ShowMessage(new List<string> { msg });
        }

        /// <summary>Shows a series of messages, user has to click NEXT button to go through them</summary>
        /// <param name="messages"></param>
        public void ShowMessage(List<string> messages) {
            _messageQueue = messages;
            _messagesRoutine = StartCoroutine(ProcessMessageQueue());

            Show(true);
        }

        private IEnumerator ProcessMessageQueue() {
            EventManager.StartListening(Edia.Events.ControlPanel.EvNextMessagePanelMsg, OnEvNextMessagepanelMsg);

            while (_messageQueue.Count > 0) {
                ShowMessageOnPanel(_messageQueue[0]);
                _hasClicked = false;

                while (!_hasClicked) {
                    yield return new WaitForEndOfFrame();
                }

                _messageQueue.RemoveAt(0);
            }
        }

        private void ButtonToggling(bool onOffNext, bool onOffProceed) {
            _buttonNEXT.interactable = onOffNext;
            _buttonNEXT.OnDeselect(null);
            _buttonNEXT.gameObject.SetActive(onOffNext);

            EventManager.TriggerEvent(Edia.Events.ControlPanel.EvEnableButton, new eParam(new[] { "NEXT", onOffNext.ToString() }));

            _buttonProceed.interactable = onOffProceed;
            _buttonProceed.OnDeselect(null);
            _buttonProceed.gameObject.SetActive(onOffProceed);

            _menuHolder.SetActive(onOffNext || onOffProceed);
        }

        private void OnEvNextMessagepanelMsg(eParam e) {
            OnBtnNEXTPressed();
        }

        /// <summary> Button pressed to show next message </summary>
        private void OnBtnNEXTPressed() {
            _hasClicked = true;
        }

        /// <summary> Button pressed to proceed experiment statemachine </summary>
        public void OnBtnProceedPressed() {
            XRManager.Instance.EnableXROverlayRayInteraction(false);
            EventManager.TriggerEvent(Edia.Events.StateMachine.EvProceed);
        }

        /// <summary>Shows the message in VR on a canvas.</summary>
        /// <param name="msg">Message to show</param>
        public void ShowMessageOnPanel(string msg) {
            if (_messageTimer != null) StopCoroutine(_messageTimer);
            if (_messagePanelFader != null) StopCoroutine(_messagePanelFader);

            _msgField.text = msg;

            ButtonToggling(_messageQueue.Count > 1 ? true : false, _messageQueue.Count == 1 ? true : false);

            _messageTextFader = _messageTextFader is not null ? null : StartCoroutine(Fader());

            Show(true);
        }

        #endregion // -------------------------------------------------------------------------------------------------------------------------------

        #region SHOW / HIDE

        /// <summary>Event handler</summary>
        void OnEvHideMessage(eParam e) {
            HidePanel();
        }

        /// <summary> Shows the panel </summary>
        /// <param name="onOff"></param>
        public void Show(bool onOff) {
            _canvas.enabled = onOff;
            if (_graphicRaycaster != null)
                _graphicRaycaster.enabled = onOff;
            _trackedDeviceGraphicRaycaster.enabled = onOff;

            // EventManager.TriggerEvent(Edia.Events.XR.EvEnableXROverlay, new eParam(onOff));
            
            XRManager.Instance.EnableOverlayCam(_stickToHMD);
            XRManager.Instance.EnableXROverlayRayInteraction(_stickToHMD);
            XRManager.Instance.EnableXRRayInteraction(!_stickToHMD);

            _messagePanelFader = _messagePanelFader is not null ? null : StartCoroutine(TextFader());
        }

        /// <summary>Doublechecks running routines and hides the panel</summary>
        public void HidePanel() {
            if (_messageTimer != null) StopCoroutine(_messageTimer);
            if (_messagePanelFader != null) StopCoroutine(_messagePanelFader);

            _messageQueue.Clear();
            HideMenu();

            XRManager.Instance.EnableXROverlayRayInteraction(false);
            XRManager.Instance.EnableXRRayInteraction(false);
            
            Show(false);
        }

        #endregion // -------------------------------------------------------------------------------------------------------------------------------

        #region MENU

        /// <summary> Hides the menu </summary>
        public void HideMenu() {
            ButtonToggling(false, false);
            XRManager.Instance.EnableXROverlayRayInteraction(false);
        }

        #endregion // -------------------------------------------------------------------------------------------------------------------------------

        #region TIMERS

        private IEnumerator HidePanelAfter(float duration) {
            yield return new WaitForSeconds(duration);
            HidePanel();
        }

        private IEnumerator Fader() {
            float duration = 0.5f;
            float currentTime = 0f;

            while (currentTime < duration) {
                float alpha = Mathf.Lerp(0f, 1f, currentTime / duration);
                float alphaBg = Mathf.Lerp(0f, _hasSolidBackground ? 1f : 0.5f, currentTime / duration);
                _backgroundImg.color = new Color(_backgroundImg.color.r, _backgroundImg.color.g, _backgroundImg.color.b, alphaBg);
                currentTime += Time.deltaTime;
                yield return null;
            }

            yield break;
        }

        private IEnumerator TextFader() {
            float duration = 0.5f;
            float currentTime = 0f;

            while (currentTime < duration) {
                float alpha = Mathf.Lerp(0f, 1f, currentTime / duration);
                _msgField.color = new Color(_msgField.color.r, _msgField.color.g, _msgField.color.b, alpha);
                currentTime += Time.deltaTime;
                yield return null;
            }

            yield break;
        }

        #endregion // -------------------------------------------------------------------------------------------------------------------------------
    }
}