using Tobii.StreamEngine;
using UnityEngine;

namespace Tobii.XR
{
    public static class StreamEngineDataMapper
    {
        public static Vector3 GetUnityPositionFrom(TobiiVector3 pos)
        {
            return new Vector3(pos.x * -1f / 1000f, pos.y / 1000f, pos.z / 1000f);
        }

        private static void FillGazeRayFrom(ref TobiiXR_GazeRay gazeRay, tobii_validity_t originValidity, TobiiVector3 origin, tobii_validity_t directionValidity, TobiiVector3 direction, Vector3 headToCenterEyeTranslation)
        {
            gazeRay.IsValid =
                originValidity == tobii_validity_t.TOBII_VALIDITY_VALID &&
                directionValidity == tobii_validity_t.TOBII_VALIDITY_VALID;
            gazeRay.Origin.x = origin.x * -1 / 1000f;
            gazeRay.Origin.y = origin.y / 1000f;
            gazeRay.Origin.z = origin.z / 1000f;
            gazeRay.Origin += headToCenterEyeTranslation;
            gazeRay.Direction.x = direction.x * -1;
            gazeRay.Direction.y = direction.y;
            gazeRay.Direction.z = direction.z;
        }

        public static void FromConsumerData(TobiiXR_EyeTrackingData to,
            ref tobii_wearable_consumer_data_t data, bool convergenceDistanceSupported, Vector3 headToCenterEyeTranslation)
        {
            FillGazeRayFrom(ref to.GazeRay, data.gaze_origin_combined_validity, data.gaze_origin_combined_mm_xyz, data.gaze_direction_combined_validity, data.gaze_direction_combined_normalized_xyz, headToCenterEyeTranslation);

            if (convergenceDistanceSupported)
            {
                to.ConvergenceDistance = data.convergence_distance_mm / 1000f;
                to.ConvergenceDistanceIsValid =
                    data.convergence_distance_validity == tobii_validity_t.TOBII_VALIDITY_VALID;
            }

            to.IsLeftEyeBlinking = data.left.blink == tobii_state_bool_t.TOBII_STATE_BOOL_TRUE ||
                                   data.left.blink_validity ==
                                   tobii_validity_t.TOBII_VALIDITY_INVALID;
            to.IsRightEyeBlinking = data.right.blink == tobii_state_bool_t.TOBII_STATE_BOOL_TRUE ||
                                    data.right.blink_validity ==
                                    tobii_validity_t.TOBII_VALIDITY_INVALID;
        }

        public static void FromAdvancedData(TobiiXR_EyeTrackingData to,
            ref tobii_wearable_advanced_data_t data, bool convergenceDistanceSupported, Vector3 headToCenterEyeTranslation)
        {
            FillGazeRayFrom(ref to.GazeRay, data.gaze_origin_combined_validity, data.gaze_origin_combined_mm_xyz, data.gaze_direction_combined_validity, data.gaze_direction_combined_normalized_xyz, headToCenterEyeTranslation);

            if (convergenceDistanceSupported)
            {
                to.ConvergenceDistance = data.convergence_distance_mm / 1000f;
                to.ConvergenceDistanceIsValid = BoolFromValidity(data.convergence_distance_validity);
            }

            to.IsLeftEyeBlinking = data.left.blink == tobii_state_bool_t.TOBII_STATE_BOOL_TRUE ||
                                   !BoolFromValidity(data.left.blink_validity);
            to.IsRightEyeBlinking = data.right.blink == tobii_state_bool_t.TOBII_STATE_BOOL_TRUE ||
                                    !BoolFromValidity(data.right.blink_validity);
        }

        private static bool BoolFromValidity(tobii_validity_t validity)
        {
            return validity == tobii_validity_t.TOBII_VALIDITY_VALID;
        }

        public static void MapAdvancedData(TobiiXR_AdvancedEyeTrackingData to,
            ref tobii_wearable_advanced_data_t data, bool convergenceDistanceSupported, Vector3 headToCenterEyeTranslation)
        {
            to.SystemTimestamp = data.timestamp_system_us;
            to.DeviceTimestamp = data.timestamp_tracker_us;

            FillGazeRayFrom(ref to.Left.GazeRay, data.left.gaze_origin_validity, data.left.gaze_origin_mm_xyz, data.left.gaze_direction_validity, data.left.gaze_direction_normalized_xyz, headToCenterEyeTranslation);
            FillGazeRayFrom(ref to.Right.GazeRay, data.right.gaze_origin_validity, data.right.gaze_origin_mm_xyz, data.right.gaze_direction_validity, data.right.gaze_direction_normalized_xyz, headToCenterEyeTranslation);
            FillGazeRayFrom(ref to.GazeRay, data.gaze_origin_combined_validity, data.gaze_origin_combined_mm_xyz, data.gaze_direction_combined_validity, data.gaze_direction_combined_normalized_xyz, headToCenterEyeTranslation);

            if (convergenceDistanceSupported)
            {
                to.ConvergenceDistance = data.convergence_distance_mm / 1000f;
                to.ConvergenceDistanceIsValid = BoolFromValidity(data.convergence_distance_validity);
            }

            to.Left.IsBlinking = data.left.blink == tobii_state_bool_t.TOBII_STATE_BOOL_TRUE ||
                                 !BoolFromValidity(data.left.blink_validity);
            to.Right.IsBlinking = data.right.blink == tobii_state_bool_t.TOBII_STATE_BOOL_TRUE ||
                                  !BoolFromValidity(data.right.blink_validity);

            to.Left.PupilDiameterValid = BoolFromValidity(data.left.pupil_diameter_validity);
            to.Left.PupilDiameter = data.left.pupil_diameter_mm;
            to.Right.PupilDiameterValid = BoolFromValidity(data.right.pupil_diameter_validity);
            to.Right.PupilDiameter = data.right.pupil_diameter_mm;

            to.Left.PositionGuideValid = BoolFromValidity(data.left.position_guide_validity);
            to.Left.PositionGuide.x = data.left.position_guide_xy.x;
            to.Left.PositionGuide.y = data.left.position_guide_xy.y;
            to.Right.PositionGuideValid = BoolFromValidity(data.right.position_guide_validity);
            to.Right.PositionGuide.y = data.right.position_guide_xy.y;
            to.Right.PositionGuide.x = data.right.position_guide_xy.x;
        }
        
        public static void FillPositionGuideData(ref PositionGuideData to,
            ref tobii_wearable_advanced_data_t data)
        {
            FillPositionGuideData(ref to, data.left.position_guide_xy, data.left.position_guide_validity,
                data.right.position_guide_xy, data.right.position_guide_validity);
        }

        public static void FillPositionGuideData(ref PositionGuideData to,
            ref tobii_wearable_consumer_data_t data)
        {
            FillPositionGuideData(ref to, data.left.position_guide_xy, data.left.position_guide_validity,
                data.right.position_guide_xy, data.right.position_guide_validity);
        }

        private static void FillPositionGuideData(ref PositionGuideData to,
            TobiiVector2 leftPositionGuide, tobii_validity_t leftValidity, TobiiVector2 rightPositionGuide,
            tobii_validity_t rightValidity)
        {
            to.Left.x = leftPositionGuide.x;
            to.Left.y = leftPositionGuide.y;
            to.LeftIsValid = BoolFromValidity(leftValidity);

            to.Right.x = rightPositionGuide.x;
            to.Right.y = rightPositionGuide.y;
            to.RightIsValid = BoolFromValidity(rightValidity);
        }
    }
}