using UnityEngine;


namespace Tobii.XR.Examples.Social
{
    public class TobiiSocialEyeData
    {
        private const float TargetDistance = 2f; // A fixed convergence distance of 2m.
        private const float InvalidGazeRayTimer = 2f; // After the gaze ray is invalid for too long, the ray should return to the camera forward direction.

        // 1 Euro Filter which is used to smooth the gaze direction.
        private readonly OneEuroFilter<Vector3> _oneEuroFilter = new OneEuroFilter<Vector3>(90f);
        
        private int _lastFrameNumber;
        private float _lastValidGazeRayCounter;
        
        public bool IsLeftEyeBlinking { get; private set; }
        public bool IsRightEyeBlinking { get; private set; }

        public Vector3 WorldGazePoint => _worldGazePoint;

        private Vector3 _worldGazePoint; // The position in world space where the player is looking.

        public TobiiSocialEyeData(float minCutoff = 10f, float beta = 5f, float dCutoff = 1.0f)
        {
            // Set the parameters for the 1 Euro Filter. 90hz is chosen but it will automatically update with the correct frequency at runtime.
            _oneEuroFilter.UpdateParams(90, minCutoff, beta, dCutoff);
        }

        public void Tick()
        {
            // Get the latest gaze ray from the eye tracker.
            var gazeRay = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World).GazeRay;
            var localGazeData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);
            IsLeftEyeBlinking = localGazeData.IsLeftEyeBlinking;
            IsRightEyeBlinking = localGazeData.IsRightEyeBlinking;
            
            // If the gaze ray is invalid in the current frame, use the previously set world gaze point,
            // but if too much time has passed, return the gaze ray to the forward direction.
            if (!gazeRay.IsValid)
            {
                _lastValidGazeRayCounter += Time.deltaTime;
                if (_lastValidGazeRayCounter >= InvalidGazeRayTimer)
                {
                    // Determine a filtered world space position in the forward direction of the camera
                    var mainCamera = Camera.main.transform;
                    _worldGazePoint = FilteredWorldGazePoint(mainCamera.position, mainCamera.forward, TargetDistance);
                }
            }
            else
            {
                _lastValidGazeRayCounter = 0;

                // Check if the frame has changed since the last call of this method, in order to avoid filtering duplicate data.
                if (_lastFrameNumber != Time.frameCount)
                {
                    // Determine a filtered world space position where the person is looking.
                    _worldGazePoint = FilteredWorldGazePoint(gazeRay.Origin, gazeRay.Direction, TargetDistance);

                    _lastFrameNumber = Time.frameCount;
                }    
            }
        }

        private Vector3 FilteredWorldGazePoint(Vector3 rayOrigin, Vector3 rayDirection, float targetDistance)
        {
            // Determine a world space position where the person is looking.
            var target = rayOrigin + rayDirection * targetDistance;
                
            // Apply the 1 Euro Filter to get a smoothed world gaze point.
            return _oneEuroFilter.Filter(target);
        }
    }
}