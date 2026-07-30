using System;
using UnityEngine;

namespace Worldforge.Infrastructure.Cameras
{
    [DisallowMultipleComponent]
    public sealed class RuntimeCameraController : MonoBehaviour
    {
        private Vector3 _followOffset = new(0f, 8f, -6f);
        private Vector3 _lookAtOffset = new(0f, 1.5f, 0f);
        private float _positionSmoothTime = 0.12f;
        private float _rotationLerpSpeed = 12f;
        private bool _snapOnTargetAcquire = true;

        private Func<Transform> _targetProvider;
        private Transform _followTarget;
        private Vector3 _positionVelocity;
        private bool _hasAppliedInitialPose;

        public Transform FollowTarget
        {
            get { return _followTarget; }
        }

        public void ApplyConfiguration(CameraFollowConfiguration configuration)
        {
            if (configuration == null)
            {
                return;
            }

            _followOffset = configuration.FollowOffset;
            _lookAtOffset = configuration.LookAtOffset;
            _positionSmoothTime = configuration.PositionSmoothTime;
            _rotationLerpSpeed = configuration.RotationLerpSpeed;
            _snapOnTargetAcquire = configuration.SnapOnTargetAcquire;
        }

        public void SetTargetProvider(Func<Transform> provider)
        {
            _targetProvider = provider;
            RefreshFollowTarget();
        }

        public void SetFollowTarget(Transform target)
        {
            _followTarget = target;
            _positionVelocity = Vector3.zero;
            _hasAppliedInitialPose = false;
        }

        public void ClearFollowTarget()
        {
            _targetProvider = null;
            _followTarget = null;
            _positionVelocity = Vector3.zero;
            _hasAppliedInitialPose = false;
        }

        private void LateUpdate()
        {
            if (_followTarget == null || !_followTarget.gameObject.activeInHierarchy)
            {
                RefreshFollowTarget();
            }

            if (_followTarget == null)
            {
                return;
            }

            var focusPoint = _followTarget.position + _lookAtOffset;
            var desiredPosition = focusPoint + _followOffset;
            var desiredRotation = Quaternion.LookRotation(focusPoint - desiredPosition, Vector3.up);

            if (_snapOnTargetAcquire && !_hasAppliedInitialPose)
            {
                transform.SetPositionAndRotation(desiredPosition, desiredRotation);
                _hasAppliedInitialPose = true;
                return;
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref _positionVelocity,
                _positionSmoothTime);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                _rotationLerpSpeed * Time.unscaledDeltaTime);
        }

        private void RefreshFollowTarget()
        {
            if (_targetProvider == null)
            {
                return;
            }

            var resolvedTarget = _targetProvider();
            if (resolvedTarget == _followTarget)
            {
                return;
            }

            SetFollowTarget(resolvedTarget);
        }
    }
}

