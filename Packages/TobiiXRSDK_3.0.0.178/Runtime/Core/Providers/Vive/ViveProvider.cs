// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System;
using System.Reflection;
using Tobii.XR;
using UnityEngine;

/// <summary>
/// Provides eye tracking data to TobiiXR using HTC's SR Anipal SDK. 
/// SR Anipal needs to be downloaded from https://hub.vive.com/en-US/download and added to the project separately. 
/// Tested with SR Anipal version 1.3.0.9.
/// </summary>
[ProviderDisplayName("VIVE"), SupportedPlatform(XRBuildTargetGroup.Standalone)]
public class HTCProvider : IEyeTrackingProvider
{
    private readonly TobiiXR_EyeTrackingData _eyeTrackingDataLocal = new TobiiXR_EyeTrackingData();
    private Matrix4x4 _localToWorldMatrix = Matrix4x4.identity;
    private GameObject _htcGameObject;
    private HmdToWorldTransformer _hmdToWorldTransformer;

    #region Reflected objects

    private Type _frameworkClass;
    private PropertyInfo _statusProperty;
    private Type _gazeIndexType;
    private Type _vector3RefType;
    private MethodInfo _getGazeRayFunc;
    private Type _eyeClass;
    private object _gazeIndexCombined;
    private Type _eyeIndexType;
    private object _eyeIndexLeft;
    private object _eyeIndexRight;
    private MethodInfo _getEyeOpennessFunc;
    private object _gazeIndexLeft;
    private object _gazeIndexRight;
    private Assembly _asm;

    #endregion

    public void GetEyeTrackingDataLocal(TobiiXR_EyeTrackingData data)
    {
        EyeTrackingDataHelper.Copy(_eyeTrackingDataLocal, data);
    }

    public Matrix4x4 LocalToWorldMatrix => _localToWorldMatrix;


    public bool Initialize()
    {
        try
        {
            _asm = Assembly.Load("Assembly-CSharp"); // SR Anipal is assumed to be in the main assembly
        }
        catch (Exception)
        {
            // This should only happen if there are 0 scripts in the game assembly
            // If there is no game assembly there is also no SR Anipal
            return false;
        }
        _frameworkClass = _asm.GetType("ViveSR.anipal.Eye.SRanipal_Eye_Framework");
        if (_frameworkClass == null) return false; // SR Anipal not installed
        _statusProperty = _frameworkClass.GetProperty("Status", BindingFlags.Public | BindingFlags.Static);
        if (_statusProperty == null) return false;

        var api = _asm.GetType("ViveSR.anipal.Eye.SRanipal_Eye_API");
        if (api == null) return false;
        var isViveProEyeFunc = api.GetMethod("IsViveProEye", BindingFlags.Public | BindingFlags.Static);
        if (isViveProEyeFunc == null) return false; // Incompatible SR Anipal?
        var isViveProEye = (bool) isViveProEyeFunc.Invoke(null, null);
        if (!isViveProEye) return false;

        _hmdToWorldTransformer = new HmdToWorldTransformer(estimatedEyeTrackerLatency_s: 0.040f);
        EnsureHtcFrameworkRunning();

        var status = (FrameworkStatus) _statusProperty.GetValue(null, null);
        if (status == FrameworkStatus.WORKING)
        {
            return TryReflectSrAnipal(); // Only reflect stuff if connection succeeded
        }

        return false;
    }

    private void EnsureHtcFrameworkRunning()
    {
        if (_htcGameObject != null) return;
        _htcGameObject = new GameObject("HTC")
        {
            hideFlags = HideFlags.HideInHierarchy
        };
        var sr = _htcGameObject.AddComponent(_frameworkClass);
        var startFrameworkFunc = _frameworkClass.GetMethod("StartFramework", BindingFlags.Instance | BindingFlags.Public);
        if (startFrameworkFunc == null) return; // Incompatible SR Anipal version
        startFrameworkFunc.Invoke(sr, null);
        var status = (FrameworkStatus) _statusProperty.GetValue(null, null);
        if (status == FrameworkStatus.WORKING) return;
        startFrameworkFunc.Invoke(sr, null); // Try a second time since it often times out on first attempt
    }

    public void Tick()
    {
        _hmdToWorldTransformer.Tick();
        EnsureHtcFrameworkRunning();

        var status = (FrameworkStatus) _statusProperty.GetValue(null, null);
        if (status != FrameworkStatus.WORKING) return;

        var combinedRayArgs = new[] {_gazeIndexCombined, Vector3.zero, Vector3.zero};
        _eyeTrackingDataLocal.GazeRay.IsValid = (bool) _getGazeRayFunc.Invoke(null, combinedRayArgs);
        _eyeTrackingDataLocal.Timestamp = Time.unscaledTime;
        _eyeTrackingDataLocal.GazeRay.Origin = (Vector3) combinedRayArgs[1];
        _eyeTrackingDataLocal.GazeRay.Direction = (Vector3) combinedRayArgs[2];

        // Blink left
        var eyeOpennessArgs = new[] {_eyeIndexLeft, 1.0f};
        var eyeOpennessIsValid = (bool) _getEyeOpennessFunc.Invoke(null, eyeOpennessArgs);
        _eyeTrackingDataLocal.IsLeftEyeBlinking = !eyeOpennessIsValid || (float) eyeOpennessArgs[1] < 0.1;

        // Blink right
        eyeOpennessArgs[0] = _eyeIndexRight;
        eyeOpennessIsValid = (bool) _getEyeOpennessFunc.Invoke(null, eyeOpennessArgs);
        _eyeTrackingDataLocal.IsRightEyeBlinking = !eyeOpennessIsValid || (float) eyeOpennessArgs[1] < 0.1;

        // Convergence distance
        var leftRayArgs = new[] {_gazeIndexLeft, Vector3.zero, Vector3.zero};
        var rightRayArgs = new[] {_gazeIndexRight, Vector3.zero, Vector3.zero};
        var leftRayValid = (bool) _getGazeRayFunc.Invoke(null, leftRayArgs);
        var rightRayValid = (bool) _getGazeRayFunc.Invoke(null, rightRayArgs);

        if (leftRayValid && rightRayValid)
        {
            _eyeTrackingDataLocal.ConvergenceDistanceIsValid = true;
            var convergenceDistance_mm = Convergence.CalculateDistance(
                (Vector3) leftRayArgs[1] * 1000f,
                (Vector3) leftRayArgs[2],
                (Vector3) rightRayArgs[1] * 1000f,
                (Vector3) rightRayArgs[2]);
            _eyeTrackingDataLocal.ConvergenceDistance = convergenceDistance_mm / 1000f; // Convert to meters
        }
        else
        {
            _eyeTrackingDataLocal.ConvergenceDistanceIsValid = false;
        }

        // Update world transform
        _hmdToWorldTransformer.Tick();
        _localToWorldMatrix = _hmdToWorldTransformer.GetLocalToWorldMatrix();
    }

    public void Destroy()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            GameObject.Destroy(_htcGameObject);
        }
        else
        {
            GameObject.DestroyImmediate(_htcGameObject);
        }
#else
        GameObject.Destroy(_htcGameObject);
#endif

        _htcGameObject = null;
    }

    private bool TryReflectSrAnipal()
    {
        _eyeClass = _asm.GetType("ViveSR.anipal.Eye.SRanipal_Eye");
        if (_eyeClass == null) return false;

        _gazeIndexType = _asm.GetType("ViveSR.anipal.Eye.GazeIndex");
        if (_gazeIndexType == null) return false;
        _gazeIndexCombined = Enum.ToObject(_gazeIndexType, GazeIndex.COMBINE);
        _gazeIndexLeft = Enum.ToObject(_gazeIndexType, GazeIndex.LEFT);
        _gazeIndexRight = Enum.ToObject(_gazeIndexType, GazeIndex.RIGHT);
        _vector3RefType = typeof(Vector3).MakeByRefType();
        _getGazeRayFunc = _eyeClass.GetMethod("GetGazeRay", new[] {_gazeIndexType, _vector3RefType, _vector3RefType});
        if (_getGazeRayFunc == null) return false;

        _eyeIndexType = _asm.GetType("ViveSR.anipal.Eye.EyeIndex");
        if (_eyeIndexType == null) return false;
        _eyeIndexLeft = Enum.ToObject(_eyeIndexType, EyeIndex.LEFT);
        _eyeIndexRight = Enum.ToObject(_eyeIndexType, EyeIndex.RIGHT);
        _getEyeOpennessFunc = _eyeClass.GetMethod("GetEyeOpenness", new[] {_eyeIndexType, typeof(float).MakeByRefType()});
        if (_getEyeOpennessFunc == null) return false;

        return true;
    }
    
    #region Copied SR Anipal Enums

    // ReSharper disable UnusedMember.Local
    // ReSharper disable InconsistentNaming
    // Copied from SR Anipal (verify this hasn't changed when upgrading)
    private enum FrameworkStatus
    {
        STOP,
        START,
        WORKING,
        ERROR,
        NOT_SUPPORT
    }

    private enum EyeIndex
    {
        LEFT,
        RIGHT,
    }

    private enum GazeIndex
    {
        LEFT,
        RIGHT,
        COMBINE
    }
    // ReSharper restore InconsistentNaming
    // ReSharper restore UnusedMember.Local

    #endregion
}