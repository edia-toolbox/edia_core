using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameViewEncoder))]
[CanEditMultipleObjects]
public class GameViewEncoder_Editor : Editor
{
    private GameViewEncoder GVEncoder;

    SerializedProperty CaptureModeProp;

    SerializedProperty ResizeProp;

    SerializedProperty MainCamProp;
    SerializedProperty RenderCamProp;
    SerializedProperty ResolutionProp;
    SerializedProperty MatchScreenAspectProp;

    SerializedProperty FMDesktopFlipXProp;
    SerializedProperty FMDesktopFlipYProp;
    SerializedProperty FMDesktopRangeXProp;
    SerializedProperty FMDesktopRangeYProp;
    SerializedProperty FMDesktopOffsetXProp;
    SerializedProperty FMDesktopOffsetYProp;
    SerializedProperty FMDesktopMonitorIDProp;


    SerializedProperty FastModeProp;
    SerializedProperty AsyncModeProp;
    SerializedProperty GZipModeProp;
    SerializedProperty PanoramaModeProp;
    SerializedProperty CubemapResolutionProp;

    SerializedProperty QualityProp;
    SerializedProperty StreamFPSProp;
    //SerializedProperty CapturedTextureProp;
    SerializedProperty OnDataByteReadyEventProp;

    SerializedProperty labelProp;
    SerializedProperty dataLengthProp;

    void OnEnable()
    {
        CaptureModeProp = serializedObject.FindProperty("CaptureMode");

        ResizeProp = serializedObject.FindProperty("Resize");

        MainCamProp = serializedObject.FindProperty("MainCam");
        RenderCamProp = serializedObject.FindProperty("RenderCam");
        ResolutionProp = serializedObject.FindProperty("Resolution");
        MatchScreenAspectProp = serializedObject.FindProperty("MatchScreenAspect");

        FMDesktopFlipXProp = serializedObject.FindProperty("FMDesktopFlipX");
        FMDesktopFlipYProp = serializedObject.FindProperty("FMDesktopFlipY");
        FMDesktopRangeXProp = serializedObject.FindProperty("FMDesktopRangeX");
        FMDesktopRangeYProp = serializedObject.FindProperty("FMDesktopRangeY");
        FMDesktopOffsetXProp = serializedObject.FindProperty("FMDesktopOffsetX");
        FMDesktopOffsetYProp = serializedObject.FindProperty("FMDesktopOffsetY");
        FMDesktopMonitorIDProp = serializedObject.FindProperty("FMDesktopMonitorID");

        FastModeProp = serializedObject.FindProperty("FastMode");
        AsyncModeProp = serializedObject.FindProperty("AsyncMode");
        GZipModeProp = serializedObject.FindProperty("GZipMode");

        PanoramaModeProp = serializedObject.FindProperty("PanoramaMode");
        CubemapResolutionProp = serializedObject.FindProperty("CubemapResolution");

        QualityProp = serializedObject.FindProperty("Quality");
        StreamFPSProp = serializedObject.FindProperty("StreamFPS");
        //CapturedTextureProp = serializedObject.FindProperty("CapturedTexture");
        OnDataByteReadyEventProp = serializedObject.FindProperty("OnDataByteReadyEvent");

        labelProp = serializedObject.FindProperty("label");
        dataLengthProp = serializedObject.FindProperty("dataLength");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        if (GVEncoder == null) GVEncoder = (GameViewEncoder)target;

        serializedObject.Update();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Mode");

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(CaptureModeProp, new GUIContent("Capture Mode"));
            GUILayout.EndHorizontal();

            if (GVEncoder.CaptureMode == GameViewCaptureMode.MainCam)
            {
                if(GVEncoder.MainCam == null)
                {
                    if (GVEncoder.MainCam == null) GVEncoder.MainCam = GVEncoder.gameObject.GetComponent<Camera>();
                    if (GVEncoder.MainCam == null) GVEncoder.MainCam = GVEncoder.gameObject.AddComponent<Camera>();
                }
                else
                {
                    if (GVEncoder.MainCam != GVEncoder.gameObject.GetComponent<Camera>()) GVEncoder.MainCam = null;

                    if (GVEncoder.MainCam == null) GVEncoder.MainCam = GVEncoder.gameObject.GetComponent<Camera>();
                    if (GVEncoder.MainCam == null) GVEncoder.MainCam = GVEncoder.gameObject.AddComponent<Camera>();
                }
                GUILayout.BeginVertical("box");
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.yellow;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("capture camera with screen aspect", style);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            else if (GVEncoder.CaptureMode == GameViewCaptureMode.RenderCam)
            {
                GUILayout.BeginVertical("box");
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.yellow;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("render texture with free aspect", style);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            else if (GVEncoder.CaptureMode == GameViewCaptureMode.FullScreen)
            {
                GUILayout.BeginVertical("box");
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.yellow;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("capture full screen with UI Canvas", style);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }


        }
        GUILayout.EndVertical();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Settings");
            GUILayout.BeginVertical("box");
            {
                if (GVEncoder.CaptureMode == GameViewCaptureMode.MainCam)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(MainCamProp, new GUIContent("MainCam"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ResizeProp, new GUIContent("Resize"));
                    GUILayout.EndHorizontal();
                }
                if (GVEncoder.CaptureMode == GameViewCaptureMode.RenderCam)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(RenderCamProp, new GUIContent("RenderCam"));
                    GUILayout.EndHorizontal();

                    if (GVEncoder.RenderCam == null)
                    {
                        //GUILayout.BeginVertical("box");
                        {
                            GUIStyle style = new GUIStyle();
                            style.normal.textColor = Color.red;

                            GUILayout.BeginHorizontal();
                            GUILayout.Label(" Render Camera cannot be null", style);
                            GUILayout.EndHorizontal();

                        }
                        //GUILayout.EndVertical();
                    }

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ResolutionProp, new GUIContent("Resolution"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(MatchScreenAspectProp, new GUIContent("MatchScreenAspect"));
                    GUILayout.EndHorizontal();

                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(PanoramaModeProp, new GUIContent("Panorama Mode"));
                    GUILayout.EndHorizontal();

                    if (GVEncoder.PanoramaMode)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(CubemapResolutionProp, new GUIContent("Cubemap Sampling"));
                        GUILayout.EndHorizontal();
                    }

                    {
                        GUIStyle style = new GUIStyle();
                        style.normal.textColor = Color.yellow;

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(" Experiment feature: Render as Panorama", style);
                        GUILayout.EndHorizontal();
                    }
                }

                if (GVEncoder.CaptureMode == GameViewCaptureMode.FullScreen)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ResizeProp, new GUIContent("Resize"));
                    GUILayout.EndHorizontal();
                }

                if (GVEncoder.CaptureMode == GameViewCaptureMode.Desktop)
                {
                    GUILayout.BeginHorizontal();
                    GUIStyle style = new GUIStyle();

                    if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.WindowsPlayer)
                    {
                        style.normal.textColor = Color.red;
                        GUILayout.Label("[Experiment] supported Windows 10 only", style);
                    }
                    else
                    {
                        style.normal.textColor = Color.yellow;
                        GUILayout.Label("[Experiment] supported Windows 10 only (Windows 8 may work too)", style);
                    }
                    
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopMonitorIDProp, new GUIContent("Monitor ID" + "(Count: " + GVEncoder.FMDesktopMonitorCount + ")"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ResolutionProp, new GUIContent("Resolution"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(MatchScreenAspectProp, new GUIContent("MatchScreenAspect"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopFlipXProp, new GUIContent("Flip X"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopFlipYProp, new GUIContent("Flip Y"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopRangeXProp, new GUIContent("Range X"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopRangeYProp, new GUIContent("Range Y"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopOffsetXProp, new GUIContent("Offset X"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopOffsetYProp, new GUIContent("Offset Y"));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(QualityProp, new GUIContent("Quality"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(StreamFPSProp, new GUIContent("StreamFPS"));
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FastModeProp, new GUIContent("Fast Encode Mode"));
                    GUILayout.EndHorizontal();

                    if (GVEncoder.FastMode)
                    {
                        //GUILayout.BeginVertical("box");
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(AsyncModeProp, new GUIContent("Async (multi-threading)"));
                            GUILayout.EndHorizontal();
                        }
                        //GUILayout.EndVertical();
                    }

                    GUILayout.BeginHorizontal();
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.yellow;
                    GUILayout.Label(" Experiment for Mac, Windows, Android (Forced Enabled on iOS)", style);
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
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Encoded");
            if (GVEncoder.CapturedTexture != null)
            {
                GUILayout.Label("Preview " + " ( " + GVEncoder.CapturedTexture.width + " x " + GVEncoder.CapturedTexture.height + " ) ");
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
                if (GVEncoder.CapturedTexture != null)
                {
                    GUI.DrawTexture(r, GVEncoder.CapturedTexture, ScaleMode.ScaleToFit);
                }
                else
                {
                    GUI.DrawTexture(r, new Texture2D((int)r.width, (int)r.height, TextureFormat.RGB24, false), ScaleMode.ScaleToFit);
                }
            }
            GUILayout.EndVertical();

            //GUILayout.BeginHorizontal();
            //EditorGUILayout.PropertyField(CapturedTextureProp, new GUIContent("Captured Texture"));
            //GUILayout.EndHorizontal();

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


                //GUILayout.BeginHorizontal();
                //GUILayout.Label("Encoded Size(byte): " + GVEncoder.dataLength);
                //GUILayout.EndHorizontal();

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
