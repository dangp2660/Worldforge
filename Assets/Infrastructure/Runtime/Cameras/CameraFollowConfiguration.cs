using UnityEngine;

namespace Worldforge.Infrastructure.Cameras
{
    /// <summary>
    /// Definition data for camera follow behavior.
    /// Contains follow distance offsets and smoothing parameters.
    /// This ScriptableObject holds read-only configuration; it must not store runtime mutable state.
    /// </summary>
    [CreateAssetMenu(
        fileName = "CameraFollowConfiguration",
        menuName = "Worldforge/Infrastructure/Camera Follow Configuration")]
    public sealed class CameraFollowConfiguration : ScriptableObject
    {
        [Header("Follow Distance")]
        [Tooltip("Positional offset from the follow target to the camera.")]
        [SerializeField] private Vector3 _followOffset = new(0f, 8f, -6f);

        [Tooltip("Offset applied to the follow target position when calculating the look-at point.")]
        [SerializeField] private Vector3 _lookAtOffset = new(0f, 1.5f, 0f);

        [Tooltip("Minimum allowed distance between camera focus point and camera.")]
        [SerializeField, Min(0.1f)] private float _minDistance = 4.5f;

        [Tooltip("Maximum allowed distance between camera focus point and camera.")]
        [SerializeField, Min(0.1f)] private float _maxDistance = 15f;

        [Tooltip("Default distance between camera focus point and camera.")]
        [SerializeField, Min(0.1f)] private float _defaultDistance = 10.5f;

        [Header("Pitch Limits")]
        [Tooltip("Minimum pitch angle in degrees (looking up / close perspective).")]
        [SerializeField, Range(-89f, 89f)] private float _minPitchAngle = 32f;

        [Tooltip("Maximum pitch angle in degrees (looking down / top-down perspective).")]
        [SerializeField, Range(-89f, 89f)] private float _maxPitchAngle = 58f;

        [Header("Input Responsiveness")]
        [Tooltip("Horizontal look sensitivity (yaw).")]
        [SerializeField, Min(0.01f)] private float _mouseSensitivityX = 0.2f;

        [Tooltip("Vertical look sensitivity (pitch).")]
        [SerializeField, Min(0.01f)] private float _mouseSensitivityY = 0.2f;

        [Tooltip("Zoom sensitivity when scrolling.")]
        [SerializeField, Min(0.01f)] private float _zoomSensitivity = 2f;

        [Tooltip("Dead zone threshold for analog look input.")]
        [SerializeField, Range(0f, 0.5f)] private float _inputDeadZone = 0.01f;

        [Tooltip("Invert pitch (vertical look).")]
        [SerializeField] private bool _invertPitch;

        [Tooltip("Invert yaw (horizontal look).")]
        [SerializeField] private bool _invertYaw;

        [Header("Follow & Rotation Smoothing")]
        [Tooltip("Time in seconds for position smooth damp to reach target. Lower values are snappier.")]
        [SerializeField, Min(0.001f)] private float _positionSmoothTime = 0.12f;

        [Tooltip("Time in seconds for rotation smooth damp to reach target pose.")]
        [SerializeField, Min(0.001f)] private float _rotationSmoothTime = 0.05f;

        [Tooltip("Interpolation speed for rotation towards look-at direction. Higher values are snappier.")]
        [SerializeField, Min(0f)] private float _rotationLerpSpeed = 12f;

        [Tooltip("Time in seconds for distance/zoom smooth damp to reach target distance.")]
        [SerializeField, Min(0.001f)] private float _zoomSmoothTime = 0.1f;

        [Header("Behavior")]
        [Tooltip("When true, the camera snaps instantly to the desired pose on first target acquisition.")]
        [SerializeField] private bool _snapOnTargetAcquire = true;

        [Tooltip("When true, holding Right Mouse Button is required to rotate the camera (V Rising style).")]
        [SerializeField] private bool _requireRightMouseToRotate = true;

        [Tooltip("When true, zooming in/out automatically tilts the pitch angle between Min and Max pitch (V Rising style).")]
        [SerializeField] private bool _enablePitchZoomCoupling = true;

        [Tooltip("When true, the mouse cursor is permanently locked and hidden during active camera control.")]
        [SerializeField] private bool _lockCursor;

        public Vector3 FollowOffset
        {
            get { return _followOffset; }
        }

        public Vector3 LookAtOffset
        {
            get { return _lookAtOffset; }
        }

        public float MinDistance
        {
            get { return _minDistance; }
        }

        public float MaxDistance
        {
            get { return _maxDistance; }
        }

        public float DefaultDistance
        {
            get { return _defaultDistance; }
        }

        public float MinPitchAngle
        {
            get { return _minPitchAngle; }
        }

        public float MaxPitchAngle
        {
            get { return _maxPitchAngle; }
        }

        public float MouseSensitivityX
        {
            get { return _mouseSensitivityX; }
        }

        public float MouseSensitivityY
        {
            get { return _mouseSensitivityY; }
        }

        public float ZoomSensitivity
        {
            get { return _zoomSensitivity; }
        }

        public float InputDeadZone
        {
            get { return _inputDeadZone; }
        }

        public bool IsPitchInverted
        {
            get { return _invertPitch; }
        }

        public bool IsYawInverted
        {
            get { return _invertYaw; }
        }

        public float PositionSmoothTime
        {
            get { return _positionSmoothTime; }
        }

        public float RotationSmoothTime
        {
            get { return _rotationSmoothTime; }
        }

        public float RotationLerpSpeed
        {
            get { return _rotationLerpSpeed; }
        }

        public float ZoomSmoothTime
        {
            get { return _zoomSmoothTime; }
        }

        public bool SnapOnTargetAcquire
        {
            get { return _snapOnTargetAcquire; }
        }

        public bool RequireRightMouseToRotate
        {
            get { return _requireRightMouseToRotate; }
        }

        public bool EnablePitchZoomCoupling
        {
            get { return _enablePitchZoomCoupling; }
        }

        public bool LockCursor
        {
            get { return _lockCursor; }
        }
    }
}
