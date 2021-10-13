using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngineInternal;

namespace Tobii.XR
{
    public static class CoordinatesHelper
    {
        private static readonly List<XRNodeState> TempNodeStates = new List<XRNodeState>(10);
        private static Vector3? _cachedHeadToCenterEyeTranslation = null;

        public static Vector3 GetHeadToCenterEyeTranslation()
        {
            // There are no known XR headsets that change head to center eye translation so use cached result if available
            if (_cachedHeadToCenterEyeTranslation.HasValue) return _cachedHeadToCenterEyeTranslation.Value;

            // Some headsets report invalid values before rendering has started
            if (!XRDisplaySubSystemHelper.IsRendering()) return Vector3.zero;

            Matrix4x4? head = null;
            Matrix4x4? centerEye = null;
            InputTracking.GetNodeStates(TempNodeStates);

            foreach (var state in TempNodeStates)
            {
                if (state.nodeType != XRNode.Head && state.nodeType != XRNode.CenterEye) continue;

                if (!state.TryGetPosition(out var pos)) break;
                if (!state.TryGetRotation(out var rot)) break;

                if (state.nodeType == XRNode.Head) head = Matrix4x4.TRS(pos, rot, Vector3.one);
                if (state.nodeType == XRNode.CenterEye) centerEye = Matrix4x4.TRS(pos, rot, Vector3.one);
            }

            if (!head.HasValue || !centerEye.HasValue) return Vector3.zero;

            var mat = centerEye.Value.inverse * head.Value; // Vce = Minv_ce*Mhead*Vhead

            // Extract translation. We assume no rotation between head and center eye
            _cachedHeadToCenterEyeTranslation = new Vector3(mat.m03, mat.m13, mat.m23);
            return _cachedHeadToCenterEyeTranslation.Value;
        }

        private static Vector3? _cachedLeftOffset;
        private static Vector3? _cachedRightOffset;

        public static bool GetEyeOffsets(out Vector3 leftOffset, out Vector3 rightOffset)
        {
            if (_cachedLeftOffset.HasValue && _cachedRightOffset.HasValue)
            {
                leftOffset = _cachedLeftOffset.Value;
                rightOffset = _cachedRightOffset.Value;
                return true;
            }

            var states = new List<XRNodeState>();
            InputTracking.GetNodeStates(states);

            Matrix4x4? leftEye = null;
            Matrix4x4? rightEye = null;
            Matrix4x4? centerEye = null;

            foreach (var state in states)
            {
                if (state.nodeType != XRNode.CenterEye && state.nodeType != XRNode.LeftEye && state.nodeType != XRNode.RightEye) continue;
                Vector3 pos;
                Quaternion rot;

                if (!state.TryGetPosition(out pos)) break;
                if (!state.TryGetRotation(out rot)) break;
                var mat = Matrix4x4.TRS(pos, rot, Vector3.one);

                switch (state.nodeType)
                {
                    case XRNode.LeftEye:
                        leftEye = mat;
                        break;
                    case XRNode.RightEye:
                        rightEye = mat;
                        break;
                    case XRNode.CenterEye:
                        centerEye = mat;
                        break;
                }
            }

            if (leftEye.HasValue && rightEye.HasValue && centerEye.HasValue)
            {
                var leftMat = centerEye.Value.inverse * leftEye.Value;
                var rightMat = centerEye.Value.inverse * rightEye.Value;
                _cachedLeftOffset = leftOffset = new Vector3(leftMat.m03, leftMat.m13, leftMat.m23);
                _cachedRightOffset = rightOffset = new Vector3(rightMat.m03, rightMat.m13, rightMat.m23);
                return true;
            }

            // Not enough data to compute offsets so report zero
            leftOffset = Vector3.zero;
            rightOffset = Vector3.zero;
            return false;
        }

        /// <summary>
        /// Converts gaze rays to projected gaze points in screen space coordinates.
        /// Be aware that screen space and texture space may have different Y directions. Also be aware that in single pass rendering the screen buffer contains both eyes so you would have to renormalize the x value of left and right gaze point.
        /// </summary>
        /// <param name="leftGazeRayViewSpace">Left gaze ray in view space coordinates.</param>
        /// <param name="rightGazeRayViewSpace">Right gaze ray in view space coordinates.</param>
        /// <param name="leftGazePoint">Normalized Gaze point where 0, 0 is bottom left and 1, 1 is top right of the left screen. Null means no value could be produced.</param>
        /// <param name="rightGazePoint">Normalized Gaze point where 0, 0 is bottom left and 1, 1 is top right of the right screen. Null means no value could be produced.</param>
        public static void GetScreenSpaceFor(TobiiXR_GazeRay leftGazeRayViewSpace, TobiiXR_GazeRay rightGazeRayViewSpace, out Vector2? leftGazePoint, out Vector2? rightGazePoint)
        {
            leftGazePoint = null;
            rightGazePoint = null;
            var cam = CameraHelper.GetMainCamera();
            Vector3 leftOffset;
            Vector3 rightOffset;
            if (!GetEyeOffsets(out leftOffset, out rightOffset)) return;

            if (leftGazeRayViewSpace.IsValid)
                leftGazePoint = GetScreenSpaceFor(leftGazeRayViewSpace.Direction, leftOffset - leftGazeRayViewSpace.Origin, cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left));
            if (rightGazeRayViewSpace.IsValid)
                rightGazePoint = GetScreenSpaceFor(rightGazeRayViewSpace.Direction, rightOffset - rightGazeRayViewSpace.Origin, cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right));
        }

        /// <summary>
        /// Project a view space gaze ray onto the screen
        /// </summary>
        /// <param name="gazeDirectionViewSpace">Gaze direction in view space coordinates.</param>
        /// <param name="realToVirtualEyeOffset">The offset between gaze origin and virtual camera position.</param>
        /// <param name="projection">The projection matrix used for the target screen. If you plan to use the gaze point in a shader consider getting the projection using GL.GetGPUProjectionMatrix.</param>
        /// <returns>Normalized Gaze point where 0, 0 is bottom left and 1, 1 is top right of the screen</returns>
        public static Vector2 GetScreenSpaceFor(Vector3 gazeDirectionViewSpace, Vector3 realToVirtualEyeOffset, Matrix4x4 projection)
        {
            var projectionDistance = projection.decomposeProjection.zNear;
            var len = (projectionDistance - realToVirtualEyeOffset.z) / gazeDirectionViewSpace.z;
            var pProjected = new Vector3(len * gazeDirectionViewSpace.x + realToVirtualEyeOffset.x, len * gazeDirectionViewSpace.y + realToVirtualEyeOffset.y, projectionDistance);
            pProjected.z = -pProjected.z; // Flip Z to get OpenGL convention used in camera space
            var pNdc = projection.MultiplyPoint(pProjected);
            return new Vector2((pNdc.x + 1.0f) / 2.0f, (pNdc.y + 1.0f) / 2.0f);
        }
    }
}