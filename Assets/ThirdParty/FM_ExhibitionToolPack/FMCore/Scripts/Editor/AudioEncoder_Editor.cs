using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioEncoder))]
[CanEditMultipleObjects]
public class AudioEncoder_Editor : Editor
{
    private AudioEncoder AEncoder;

    SerializedProperty StreamGameSoundProp;
    //SerializedProperty OutputSampleRateProp;
    //SerializedProperty OutputChannelsProp;
    SerializedProperty GZipModeProp;

    SerializedProperty StreamFPSProp;


    SerializedProperty OnDataByteReadyEventProp;


    SerializedProperty labelProp;
    SerializedProperty dataLengthProp;

    void OnEnable()
    {

        StreamGameSoundProp = serializedObject.FindProperty("StreamGameSound");
        //OutputSampleRateProp = serializedObject.FindProperty("OutputSampleRate");
        //OutputChannelsProp = serializedObject.FindProperty("OutputChannels");

        StreamFPSProp = serializedObject.FindProperty("StreamFPS");
        GZipModeProp = serializedObject.FindProperty("GZipMode");


        OnDataByteReadyEventProp = serializedObject.FindProperty("OnDataByteReadyEvent");

        labelProp = serializedObject.FindProperty("label");
        dataLengthProp = serializedObject.FindProperty("dataLength");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        if(AEncoder== null) AEncoder = (AudioEncoder)target;

        serializedObject.Update();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Capture");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(StreamGameSoundProp, new GUIContent("Stream Game Sound"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Audio Info");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Output Channels: " + AEncoder.OutputChannels);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Output Sample Rate: " + AEncoder.OutputSampleRate);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Encoded");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(StreamFPSProp, new GUIContent("StreamFPS"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(GZipModeProp, new GUIContent("GZip Mode"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.yellow;
                GUILayout.Label(" Experiment feature: Reduce network traffic", style);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(OnDataByteReadyEventProp, new GUIContent("OnDataByteReadyEvent"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Pair Encoder & Decoder ");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(labelProp, new GUIContent("label"));
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(dataLengthProp, new GUIContent("Encoded Size(byte)"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
