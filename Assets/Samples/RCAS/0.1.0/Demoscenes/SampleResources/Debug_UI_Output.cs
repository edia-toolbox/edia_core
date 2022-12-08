using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using Unity.Collections;
using RCAS;

public class Debug_UI_Output : MonoBehaviour
{
    volatile string output = "";
    TMPro.TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMPro.TMP_Text>();

        Application.logMessageReceivedThreaded += HandleLog;
    }

    void Update()
    {
        text.text = output;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        output = logString + "\n" + output;
        if (type != LogType.Log)
        {
            output += stackTrace + "\n" + output;
        }
        if (output.Length > 5000) output = "";
    }
}
