using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FMNetworkManager))]
[CanEditMultipleObjects]
public class FMNetworkManager_Editor : Editor
{
    private FMNetworkManager FMNetwork;

    private bool ShowServerSettings = false;
    private bool ShowClientSettings = false;

    SerializedProperty AutoInitProp;
    SerializedProperty NetworkTypeProp;


    SerializedProperty ServerSettings_ServerListenPortProp;
    SerializedProperty ServerSettings_UseAsyncListenerProp;
    SerializedProperty ServerSettings_UseMainThreadSenderProp;
    SerializedProperty ServerSettings_ConnectionCountProp;



    SerializedProperty ClientSettings_ClientListenPortProp;
    SerializedProperty ClientSettings_UseMainThreadSenderProp;
    SerializedProperty ClientSettings_AutoNetworkDiscoveryProp;
    SerializedProperty ClientSettings_ServerIPProp;
    SerializedProperty ClientSettings_IsConnectedProp;

    SerializedProperty ShowLogProp;
    SerializedProperty UIStatusProp;

    SerializedProperty OnReceivedByteDataEventProp;
    SerializedProperty OnReceivedStringDataEventProp;
    SerializedProperty GetRawReceivedDataProp;

    SerializedProperty NetworkObjectsProp;
    private bool NetworkObjectsFold = false;
    SerializedProperty SyncFPSProp;

    //SerializedProperty labelProp;
    //SerializedProperty dataLengthProp;

    void OnEnable()
    {

        AutoInitProp = serializedObject.FindProperty("AutoInit");
        NetworkTypeProp = serializedObject.FindProperty("NetworkType");

        ServerSettings_ServerListenPortProp = serializedObject.FindProperty("ServerSettings.ServerListenPort");
        ServerSettings_UseAsyncListenerProp = serializedObject.FindProperty("ServerSettings.UseAsyncListener");
        ServerSettings_UseMainThreadSenderProp = serializedObject.FindProperty("ServerSettings.UseMainThreadSender");
        ServerSettings_ConnectionCountProp = serializedObject.FindProperty("ServerSettings.ConnectionCount");


        ClientSettings_ClientListenPortProp = serializedObject.FindProperty("ClientSettings.ClientListenPort");
        ClientSettings_UseMainThreadSenderProp = serializedObject.FindProperty("ClientSettings.UseMainThreadSender");
        ClientSettings_AutoNetworkDiscoveryProp = serializedObject.FindProperty("ClientSettings.AutoNetworkDiscovery");
        ClientSettings_ServerIPProp = serializedObject.FindProperty("ClientSettings.ServerIP");
        ClientSettings_IsConnectedProp = serializedObject.FindProperty("ClientSettings.IsConnected");

        ShowLogProp = serializedObject.FindProperty("ShowLog");
        UIStatusProp = serializedObject.FindProperty("UIStatus");

        OnReceivedByteDataEventProp = serializedObject.FindProperty("OnReceivedByteDataEvent");
        OnReceivedStringDataEventProp = serializedObject.FindProperty("OnReceivedStringDataEvent");
        GetRawReceivedDataProp = serializedObject.FindProperty("GetRawReceivedData");

        NetworkObjectsProp = serializedObject.FindProperty("NetworkObjects");
        SyncFPSProp = serializedObject.FindProperty("SyncFPS");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        if (FMNetwork == null) FMNetwork = (FMNetworkManager)target;

        serializedObject.Update();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Networking");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(AutoInitProp, new GUIContent("Auto Init"));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(NetworkTypeProp, new GUIContent("NetworkType"));
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical();
                {
                    if (ShowServerSettings)
                    {
                        GUILayout.BeginHorizontal();

                        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                        if (GUILayout.Button("- Server Settings")) ShowServerSettings = false;
                        GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                        GUILayout.EndHorizontal();

                        GUILayout.BeginVertical("box");
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(ServerSettings_ServerListenPortProp, new GUIContent("ServerListenPort"));
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(ServerSettings_UseAsyncListenerProp, new GUIContent("UseAsyncListener"));
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(ServerSettings_UseMainThreadSenderProp, new GUIContent("UseMainThreadSender"));
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(ServerSettings_ConnectionCountProp, new GUIContent("Connection Count"));
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndVertical();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                        if (GUILayout.Button("+ Server Settings")) ShowServerSettings = true;
                        GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    if (ShowClientSettings)
                    {
                        GUILayout.BeginHorizontal();
                        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                        if (GUILayout.Button("- Client Settings")) ShowClientSettings = false;
                        GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                        GUILayout.EndHorizontal();

                        GUILayout.BeginVertical("box");
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(ClientSettings_ClientListenPortProp, new GUIContent("ClientListenPort"));
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(ClientSettings_UseMainThreadSenderProp, new GUIContent("UseMainThreadSender"));
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(ClientSettings_AutoNetworkDiscoveryProp, new GUIContent("AutoNetworkDiscovery"));
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(ClientSettings_ServerIPProp, new GUIContent("ServerIP"));
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(ClientSettings_IsConnectedProp, new GUIContent("IsConnected"));
                            GUILayout.EndHorizontal();

                        }
                        GUILayout.EndVertical();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                        if (GUILayout.Button("+ Client Settings")) ShowClientSettings = true;
                        GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Sync Transformation from Server");
            GUILayout.BeginVertical("box");
            {
                int NetworkObjectsNum = NetworkObjectsProp.FindPropertyRelative("Array.size").intValue;

                if (NetworkObjectsFold)
                {
                    GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                    if (GUILayout.Button("- NetworkObjects: " + NetworkObjectsNum)) NetworkObjectsFold = false;
                    GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                    DrawPropertyArray(NetworkObjectsProp);
                }
                else
                {
                    GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                    if (GUILayout.Button("+ NetworkObjects: " + NetworkObjectsNum)) NetworkObjectsFold = true;
                    GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                }

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(SyncFPSProp, new GUIContent("SyncFPS"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Receiver");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(OnReceivedByteDataEventProp, new GUIContent("OnReceivedByteDataEvent"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(OnReceivedStringDataEventProp, new GUIContent("OnReceivedStringDataEvent"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(GetRawReceivedDataProp, new GUIContent("GetRawReceivedData"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();


        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Debug");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Status: " + FMNetwork.Status);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(UIStatusProp, new GUIContent("UIStatus"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ShowLogProp, new GUIContent("ShowLog"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();


        serializedObject.ApplyModifiedProperties();
    }

    private void DrawPropertyArray(SerializedProperty property)
    {
        SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
        EditorGUILayout.PropertyField(arraySizeProp);

        EditorGUI.indentLevel++;

        for (int i = 0; i < arraySizeProp.intValue; i++)
        {
            EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
        }

        EditorGUI.indentLevel--;
    }
}
