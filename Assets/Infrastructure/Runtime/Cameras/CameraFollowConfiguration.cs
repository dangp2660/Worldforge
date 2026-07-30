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

        [Header("Follow Smoothing")]
        [Tooltip("Time in seconds for the position smooth damp to reach the target. Lower values are snappier.")]
        [SerializeField, Min(0.01f)] private float _positionSmoothTime = 0.12f;

        [Tooltip("Interpolation speed for rotation towards the look-at direction. Higher values are snappier.")]
        [SerializeField, Min(0f)] private float _rotationLerpSpeed = 12f;

        [Header("Behavior")]
        [Tooltip("When true, the camera snaps instantly to the desired pose on first target acquisition.")]
        [SerializeField] private bool _snapOnTargetAcquire = true;

        public Vector3 FollowOffset
        {
            get { return _followOffset; }
        }

        public Vector3 LookAtOffset
        {
            get { return _lookAtOffset; }
        }

        public float PositionSmoothTime
        {
            get { return _positionSmoothTime; }
        }

        public float RotationLerpSpeed
        {
            get { return _rotationLerpSpeed; }
        }

        public bool SnapOnTargetAcquire
        {
            get { return _snapOnTargetAcquire; }
        }
    }
}
