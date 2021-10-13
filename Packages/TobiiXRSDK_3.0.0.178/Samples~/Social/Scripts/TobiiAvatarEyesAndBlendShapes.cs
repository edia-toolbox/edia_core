using UnityEngine;

namespace Tobii.XR.Examples.Social
{
    public class TobiiAvatarEyesAndBlendShapes : MonoBehaviour
    {
#pragma warning disable 649 // Field is never assigned
        [Header("References")]
        [SerializeField, Tooltip("The face's SkinnedMeshRenderer which contains blend shapes.")]
        private SkinnedMeshRenderer face;
        [SerializeField, Tooltip("Left eye transform which should face directly forward from the character.")] 
        private Transform leftEye;
        [SerializeField, Tooltip("Right eye transform which should face directly forward from the character.")] 
        private Transform rightEye;
        
        [Header("Settings")]
        [SerializeField, Tooltip("Movement curve of the blend shapes, which is used to set the acceleration/deceleration.")]
        private AnimationCurve blendShapeMovementCurve;
        [SerializeField, Tooltip("The speed of the eyelid in units per second when going from open to closed, or closed to open.")]
        private float eyelidSpeed = 30f;
        [SerializeField, Tooltip("The upward angle of the eye at which blend shapes start to have an effect.")]
        private float lookUpBlendShapeStartAngle = 10f;
        [SerializeField, Tooltip("The upward angle of the eye at which blend shapes have reached their maximum effect.")]
        private float lookUpBlendShapeEndAngle = 25f;
        [SerializeField, Tooltip("The downward angle of the eye at which blend shapes start to have an effect.")]
        private float lookDownBlendShapeStartAngle = -10f;
        [SerializeField, Tooltip("The downward angle of the eye at which blend shapes have reached their maximum effect.")]
        private float lookDownBlendShapeEndAngle = -25f;
        
#pragma warning restore 649

        private readonly TobiiSocialEyeData _socialEyeData = new TobiiSocialEyeData();
        private float _leftBlinkCurrentValue;
        private float _rightBlinkCurrentValue;

        private enum BlendShape
        {
            LeftEyeLid = 17,
            RightEyeLid = 18,
            BrowInnerUp = 21,
            BrowOuterUpLeft = 22,
            BrowOuterUpRight = 23,
            EyesLookDown = 62,
            EyesLookUp = 61,
        }

        private const float EyeBrowBlendShapeFactor = 0.25f;

        private void Update()
        {
            _socialEyeData.Tick();
            var worldGazePoint = _socialEyeData.WorldGazePoint;
            
            // Rotate each eye to look at the world gaze point.
            leftEye.LookAt(worldGazePoint);
            rightEye.LookAt(worldGazePoint);

            // Update the blink blend shapes using the latest eye data from the TobiiSocialEyeData script.
            // The data source can be changed for non-local players, to receive the blink bools and world gaze point over the network.
            UpdateBlinkBlendShape(_socialEyeData.IsLeftEyeBlinking, ref _leftBlinkCurrentValue, BlendShape.LeftEyeLid);
            UpdateBlinkBlendShape(_socialEyeData.IsRightEyeBlinking, ref _rightBlinkCurrentValue, BlendShape.RightEyeLid);

            // Get the vertical gaze angle of the eyes. Both eyes should have the same vertical angle, so only one eye is needed.
            // The vertical angle is retrieved from the transform to avoid sending an extra float over the network (only worldGazePoint is needed).
            var verticalGazeAngle = leftEye.transform.localEulerAngles.x;
            verticalGazeAngle = (verticalGazeAngle > 180) ? verticalGazeAngle - 360 : verticalGazeAngle;
            verticalGazeAngle *= -1;
            
            // Update the lookUp, lookDown, and eyebrow blend shapes using the latest eye data from the TobiiSocialEyeData script.
            UpdateLookingUpBlendShapes(_socialEyeData.WorldGazePoint, verticalGazeAngle);
            UpdateLookingDownBlendShapes(_socialEyeData.WorldGazePoint, verticalGazeAngle);
        }

        private void UpdateBlinkBlendShape(bool isEyeBlinking, ref float currentValue, BlendShape blendShape)
        {
            // If the blink's current value has already reached its target value, there's no need to set the blend shape.
            var target = isEyeBlinking ? 1f : 0f;
            if (Mathf.Approximately(currentValue, target)) return;

            // Increase the current value towards the target value.
            var direction = currentValue < target ? eyelidSpeed : -eyelidSpeed;
            currentValue = Mathf.Clamp01(currentValue + direction * Time.deltaTime);
            
            // Set the blink blend shape for this eye, using the animation curve to smoothly ease in and out (accelerate and decelerate).
            SetBlendShape(blendShape, blendShapeMovementCurve.Evaluate(currentValue));
        }
        
        private void UpdateLookingUpBlendShapes(Vector3 worldGazePoint, float verticalGazeAngle)
        {
            // Calculate a 'look up' gaze angle value on a scale from 0 to 1, from lookUpBlendShapeStartAngle to lookUpBlendShapeEndAngle.
            var lookUpNormalizedValue = Mathf.Clamp01((verticalGazeAngle - lookUpBlendShapeStartAngle) / (lookUpBlendShapeEndAngle - lookUpBlendShapeStartAngle));

            // Slowly increase the effect of the blend shape as the eye move past the start angle. 
            var lookUpBlendShapeValue = blendShapeMovementCurve.Evaluate(lookUpNormalizedValue);

            // Set the 'look up' blend shape values. Brows shouldn't move much when you look up, so the value is decreased with EyeBrowBlendShapeFactor.
            SetBlendShape(BlendShape.EyesLookUp, lookUpBlendShapeValue);
            SetBlendShape(BlendShape.BrowInnerUp, lookUpBlendShapeValue * EyeBrowBlendShapeFactor);
            SetBlendShape(BlendShape.BrowOuterUpLeft, lookUpBlendShapeValue * EyeBrowBlendShapeFactor);
            SetBlendShape(BlendShape.BrowOuterUpRight, lookUpBlendShapeValue * EyeBrowBlendShapeFactor);
        }

        private void UpdateLookingDownBlendShapes(Vector3 worldGazePoint, float verticalGazeAngle)
        {
            // Calculate a 'look down' gaze angle value on a scale from 0 to 1, from lookDownBlendShapeStartAngle to lookDownBlendShapeEndAngle.
            var lookDownNormalizedValue = Mathf.Clamp01((verticalGazeAngle - lookDownBlendShapeStartAngle) / (lookDownBlendShapeEndAngle - lookDownBlendShapeStartAngle));

            // When blinking while looking down, eyesLookDown and eyeBlink act at the same time, unrealistically stretching down the top eyelid.
            // To avoid this, the eyelids are only moved down by the amount not used by the blink blend shapes.
            var blinkValue = Mathf.Max(_leftBlinkCurrentValue, _rightBlinkCurrentValue);
            lookDownNormalizedValue = Mathf.Clamp(lookDownNormalizedValue, 0, 1 - blinkValue);

            // Slowly increase the effect of the blend shape as the eye move past the start angle. 
            var lookDownBlendShapeValue = blendShapeMovementCurve.Evaluate(lookDownNormalizedValue);

            // Set the 'look down' blend shape value.
            SetBlendShape(BlendShape.EyesLookDown, lookDownBlendShapeValue);
        }
        
        private void SetBlendShape(BlendShape blendShape, float normalizedValue)
        {
            // Set the desired blend shape, using a normalizedValue (0 to 1) and multiplying by 100 to fit the SkinnedMeshRenderer blend shape value range (0 to 100).
            face.SetBlendShapeWeight((int) blendShape, normalizedValue * 100f);
        }
    }
}