using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameViewDecoder))]
[CanEditMultipleObjects]
public class GameViewDecoder_Editor : Editor
{
    private GameViewDecoder GVDecoder;


    SerializedProperty FastModeProp;
    SerializedProperty AsyncModeProp;
    SerializedProperty DecoderDelayProp;

    SerializedProperty ReceivedTextureProp;

    SerializedProperty OnReceivedTexture2DProp;

    SerializedProperty TestQuadProp;
    SerializedProperty TestImgProp;
    SerializedProperty labelProp;

    void OnEnable()
    {

        FastModeProp = serializedObject.FindProperty("FastMode");
        AsyncModeProp = serializedObject.FindProperty("AsyncMode");
        DecoderDelayProp = serializedObject.FindProperty("DecoderDelay");

        ReceivedTextureProp = serializedObject.FindProperty("ReceivedTexture");
        OnReceivedTexture2DProp = serializedObject.FindProperty("OnReceivedTexture2D");

        TestQuadProp = serializedObject.FindProperty("TestQuad");
        TestImgProp = serializedObject.FindProperty("TestImg");


        labelProp = serializedObject.FindProperty("label");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        if(GVDecoder==null) GVDecoder= (GameViewDecoder)target;

        serializedObject.Update();

        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Settings");

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(FastModeProp, new GUIContent("Fast Decode Mode"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.yellow;
                GUILayout.Label(" Experiment for Mac, Windows, Android (Forced Enabled on iOS)", style);
                GUILayout.EndHorizontal();

                if (GVDecoder.FastMode)
                {
                    //GUILayout.BeginVertical("box");
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(AsyncModeProp, new GUIContent("Async (multi-threading)"));
                        GUILayout.EndHorizontal();
                    }
                    //GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(DecoderDelayProp, new GUIContent("Delay Decode (sec)"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Decoded");

            if (GVDecoder.ReceivedTexture != null)
            {
                GUILayout.Label("Preview " + " ( " + GVDecoder.ReceivedTexture.width + " x " + GVDecoder.ReceivedTexture.height + " ) ");
            }
            else
            {
                GUILayout.Label("Preview (Empty)");
            }

            
            GUILayout.BeginVertical("box");
            {
                const float maxLogoWidth = 430.0f;
                EditorGUILayout.Separator();
                float w = EditorGUIUtility.currentViewWidth;
                Rect r = new Rect();
                r.width = Math.Min(w - 40.0f, maxLogoWidth);
                r.height = r.width / 4.886f;
                Rect r2 = GUILayoutUtility.GetRect(r.width, r.height);
                r.x = r2.x;
                r.y = r2.y;
                if (GVDecoder.ReceivedTexture != null)
                {
                    GUI.DrawTexture(r, GVDecoder.ReceivedTexture, ScaleMode.ScaleToFit);
                }
                else
                {
                    GUI.DrawTexture(r, new Texture2D((int)r.width, (int)r.height, TextureFormat.RGB24, false), ScaleMode.ScaleToFit);
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(ReceivedTextureProp, new GUIContent("ReceivedTexture"));
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(OnReceivedTexture2DProp, new GUIContent("OnReceivedTexture2D"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(TestQuadProp, new GUIContent("TestQuadProp"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(TestImgProp, new GUIContent("TestImgProp"));
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
