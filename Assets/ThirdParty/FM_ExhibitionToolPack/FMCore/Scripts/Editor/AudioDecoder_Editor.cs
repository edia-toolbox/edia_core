using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioDecoder))]
[CanEditMultipleObjects]
public class AudioDecoder_Editor : Editor
{
    private AudioDecoder ADecoder;
    SerializedProperty labelProp;
    //SerializedProperty SourceChannelsProp;
    //SerializedProperty SourceSampleRateProp;
    //SerializedProperty DeviceSampleRateProp;

    void OnEnable()
    {
        labelProp = serializedObject.FindProperty("label");
        //SourceChannelsProp = serializedObject.FindProperty("SourceChannels");
        //SourceSampleRateProp = serializedObject.FindProperty("SourceSampleRate");
        //DeviceSampleRateProp = serializedObject.FindProperty("DeviceSampleRate");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        if(ADecoder== null) ADecoder = (AudioDecoder)target;

        serializedObject.Update();

        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Audio Info");
            GUILayout.BeginVertical("box");
            {
                //GUILayout.BeginHorizontal();
                //EditorGUILayout.PropertyField(SourceChannelsProp, new GUIContent("SourceChannels"));
                //GUILayout.EndHorizontal();

                //GUILayout.BeginHorizontal();
                //EditorGUILayout.PropertyField(SourceSampleRateProp, new GUIContent("SourceSampleRate"));
                //GUILayout.EndHorizontal();

                //GUILayout.BeginHorizontal();
                //EditorGUILayout.PropertyField(DeviceSampleRateProp, new GUIContent("DeviceSampleRate"));
                //GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Source Sample Rate: " + ADecoder.SourceSampleRate);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Source Channels: " + ADecoder.SourceChannels);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Device Sample Rate: " + ADecoder.DeviceSampleRate);
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
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
