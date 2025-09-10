using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

[EdiaHeader("EDIA CORE", "Unity Console", "Displays Unity console in VR")]
public class UnityConsoleInVR : MonoBehaviour {
    private       TextMeshProUGUI _consoleTextField;
    private       Queue<string>   logMessages = new Queue<string>();
    private const int             maxMessages = 20;
    private       ScrollRect      _scrollRect;

    private void Awake() {
        _consoleTextField              =  GetComponentInChildren<TextMeshProUGUI>();
        _scrollRect                    =  GetComponentInChildren<ScrollRect>();
        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy() {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type) {
        string message = $"[{type}] {logString}";
        logMessages.Enqueue(message);

        while (logMessages.Count > maxMessages) {
            logMessages.Dequeue();
        }

        UpdateConsoleText();
    }

    private void UpdateConsoleText() {
        _consoleTextField.text = string.Join("\n", logMessages);
        Canvas.ForceUpdateCanvases();
        
        if (_scrollRect != null)
        {
            // Scroll to the bottom
            _scrollRect.verticalNormalizedPosition = 0f;
            // (Optional) Force update again if needed
            Canvas.ForceUpdateCanvases();
        }

    }
}