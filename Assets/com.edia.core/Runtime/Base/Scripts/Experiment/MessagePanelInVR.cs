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
    public class MessagePanelInVR : Singleton<MessagePanelInVR> {

        public enum PanelBehaviours {
            WorldPosition,
            StuckToHMD,
            WorldOrbit
        }

        [Header("Settings")]
        [InspectorHeader("EDIA CORE", "Message panel in VR", "The main VR Canvas panel to inform the VR User.")]
        [Tooltip("Panel behaviour. World position, stuck to HMD or world orbit. If world orbit, the panel will orbit around the user's head.")]
        public PanelBehaviours PanelBehaviour = PanelBehaviours.WorldPosition;

        [Tooltip("Stuck in front of users view at set distance.")]
        [SerializeField] private float _distanceFromHMD = 2.5f;

        [Tooltip("Scaling factor of message panel. Use lower values with lowe distance.")]
        [Range(0,1)]
        [SerializeField] private float _panelScaling = 1f;

        [Space(20)] [Header("Refs")]
        [SerializeField] private TextMeshProUGUI _msgField = null;
        [SerializeField] private GameObject _menuHolder    = null;
        [SerializeField] private Button     _buttonNEXT    = null;
        [SerializeField] private Button     _buttonProceed = null;

        // Locals
        private Image                         _backgroundImg     = null;
        private bool                          _hasClicked        = false;
        private List<string>                  _messageQueue      = new();
        private Coroutine                     _messageTimer      = null;
        private Coroutine                     _messagePanelFader = null;
        private Coroutine                     _messageTextFader  = null;
        private Coroutine                     _messagesRoutine   = null;
        private Canvas                        _canvas;
        private GraphicRaycaster              _graphicRaycaster;
        private TrackedDeviceGraphicRaycaster _trackedDeviceGraphicRaycaster;
        private Transform[]                   _panelChildren;
        private bool                          _isVisible = false;
        private GameObject                    _rotationPivot;
        // ---

        private void Awake() {
            _trackedDeviceGraphicRaycaster = GetComponent<TrackedDeviceGraphicRaycaster>();

            _canvas             = GetComponent<Canvas>();
            _canvas.worldCamera = XRManager.Instance.XRCam.GetComponent<Camera>();
            _backgroundImg      = transform.GetChild(0).GetComponent<Image>();

            _panelChildren = this.gameObject.GetComponentsInChildren<Transform>(true);

            if (GetComponent<GraphicRaycaster>() is not null)
                _graphicRaycaster = GetComponent<GraphicRaycaster>();

            // Set scaling
            transform.GetChild(0).localScale = new Vector3(_panelScaling, _panelScaling, _panelScaling);
            // Set panel behaviour
            switch (PanelBehaviour) {
                case PanelBehaviours.WorldPosition:
                    this.transform.SetParent(null, true);
                    break;
                case PanelBehaviours.StuckToHMD:
                    transform.parent        = XRManager.Instance.XRCam.transform;
                    transform.localPosition = new Vector3(0, 0, _distanceFromHMD);
                    transform.localRotation = Quaternion.identity;
                    break;
                case PanelBehaviours.WorldOrbit:
                    _rotationPivot = new GameObject("MessagePanelPivot");
                    _rotationPivot.transform.SetParent(XRManager.Instance.gameObject.transform, false);
                    _rotationPivot.transform.localPosition = Vector3.zero;
                    _rotationPivot.transform.rotation      = Quaternion.identity;
                    transform.parent              = _rotationPivot.transform;
                    transform.localPosition       = new Vector3(0, 1.7f, _distanceFromHMD);
                    transform.localRotation       = Quaternion.identity;
                    break;
                default:
                    break;
            }
        }

        private void Start() {
            EventManager.StartListening(Edia.Events.StateMachine.EvProceed, OnEvHideMessage);
        }

        private void OnDestroy() {
            EventManager.StopListening(Edia.Events.StateMachine.EvProceed, OnEvHideMessage);
        }

        private void Update() {
            if (_isVisible && PanelBehaviour is PanelBehaviours.WorldOrbit) {
                _rotationPivot.transform.rotation = Quaternion.Lerp(_rotationPivot.transform.rotation, Quaternion.Euler(0, XRManager.Instance.XRCam.transform.rotation.eulerAngles.y, 0), Time.deltaTime * 5f);
                transform.localPosition = new Vector3(0, XRManager.Instance.XRCam.transform.position.y, _distanceFromHMD);
            }
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
            _messageQueue    = messages;
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
        public void OnBtnNEXTPressed() {
            _hasClicked = true;
        }

        /// <summary> Button pressed to proceed experiment statemachine </summary>
        public void OnBtnProceedPressed() {
            EventManager.TriggerEvent(Edia.Events.StateMachine.EvProceed);
        }

        /// <summary>Shows the message in VR on a canvas.</summary>
        /// <param name="msg">Message to show</param>
        private void ShowMessageOnPanel(string msg) {
            if (_messageTimer != null) StopCoroutine(_messageTimer);
            if (_messagePanelFader != null) StopCoroutine(_messagePanelFader);

            _msgField.text = msg;

            ButtonToggling(_messageQueue.Count > 1 ? true : false, _messageQueue.Count == 1 ? true : false);

            _messageTextFader = _messageTextFader is not null ? null : StartCoroutine(Fader());

            Show(true);
        }

#endregion

#region SHOW / HIDE

        /// <summary>Event handler</summary>
        void OnEvHideMessage(eParam e) {
            HidePanel();
        }

        /// <summary> Shows the panel </summary>
        /// <param name="onOff"></param>
        public void Show(bool onOff) {
            _messagePanelFader = _messagePanelFader is not null ? null : StartCoroutine(TextFader());

            _canvas.enabled = onOff;
            if (_graphicRaycaster is not null)
                _graphicRaycaster.enabled = onOff;
            _trackedDeviceGraphicRaycaster.enabled = onOff;

            XRManager.Instance.EnableRayInteraction(onOff);
            _isVisible = onOff;
        }

        /// <summary>Doublechecks running routines and hides the panel</summary>
        public void HidePanel() {
            if (_messageTimer != null) StopCoroutine(_messageTimer);
            if (_messagePanelFader != null) StopCoroutine(_messagePanelFader);

            _messageQueue.Clear();

            HideMenu();
            Show(false);
        }

#endregion

#region MENU

        /// <summary> Hides the menu </summary>
        public void HideMenu() {
            ButtonToggling(false, false);
            XRManager.Instance.EnableRayInteraction(false);
        }

#endregion

#region TIMERS

        private IEnumerator HidePanelAfter(float duration) {
            yield return new WaitForSeconds(duration);
            HidePanel();
        }

        private IEnumerator Fader() {
            float duration    = 0.5f;
            float currentTime = 0f;

            while (currentTime < duration) {
                float alpha   = Mathf.Lerp(0f, 1f, currentTime / duration);
                float alphaBg = Mathf.Lerp(0f, 0.5f, currentTime / duration);
                _backgroundImg.color =  new Color(_backgroundImg.color.r, _backgroundImg.color.g, _backgroundImg.color.b, alphaBg);
                currentTime          += Time.deltaTime;
                yield return null;
            }

            yield break;
        }

        private IEnumerator TextFader() {
            float duration    = 0.5f;
            float currentTime = 0f;

            while (currentTime < duration) {
                float alpha = Mathf.Lerp(0f, 1f, currentTime / duration);
                _msgField.color =  new Color(_msgField.color.r, _msgField.color.g, _msgField.color.b, alpha);
                currentTime     += Time.deltaTime;
                yield return null;
            }

            yield break;
        }

#endregion
    }
}