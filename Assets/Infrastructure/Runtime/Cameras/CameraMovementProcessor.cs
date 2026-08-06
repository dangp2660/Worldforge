using System;
using UnityEngine;

namespace Worldforge.Infrastructure.Cameras
{
    public sealed class CameraMovementProcessor
    {
        private float _pitch = 15f;
        private float _yaw;
        private float _targetDistance = 6f;
        private float _currentDistance = 6f;

        private Vector3 _positionVelocity;
        private float _distanceVelocity;

        public CameraMovementProcessor()
        {
        }

        public float Pitch
        {
            get { return _pitch; }
        }

        public float Yaw
        {
            get { return _yaw; }
        }

        public float TargetDistance
        {
            get { return _targetDistance; }
        }

        public float CurrentDistance
        {
            get { return _currentDistance; }
        }

        public void SetPose(float pitch, float yaw, float distance)
        {
            _pitch = pitch;
            _yaw = Mathf.Repeat(yaw, 360f);
            _targetDistance = Mathf.Max(0.1f, distance);
            _currentDistance = _targetDistance;
            _positionVelocity = Vector3.zero;
            _distanceVelocity = 0f;
        }

        public void ResetVelocity()
        {
            _positionVelocity = Vector3.zero;
            _distanceVelocity = 0f;
        }

        public (Vector3 position, Quaternion rotation) CalculatePose(
            Vector3 currentPosition,
            Quaternion currentRotation,
            Transform targetTransform,
            CameraInputResult inputDelta,
            CameraFollowConfiguration configuration,
            float deltaTime,
            bool snapPose)
        {
            if (targetTransform == null || configuration == null)
            {
                return (currentPosition, currentRotation);
            }

            var focusPoint = targetTransform.position + configuration.LookAtOffset;

            _yaw = Mathf.Repeat(_yaw + inputDelta.YawDelta, 360f);

            _targetDistance = Mathf.Clamp(
                _targetDistance - inputDelta.ZoomDelta,
                configuration.MinDistance,
                configuration.MaxDistance);

            var hasManualPitchInput = Mathf.Abs(inputDelta.PitchDelta) > 0.0001f;

            if (hasManualPitchInput)
            {
                _pitch = Mathf.Clamp(
                    _pitch + inputDelta.PitchDelta,
                    configuration.MinPitchAngle,
                    configuration.MaxPitchAngle);
            }
            else if (configuration.EnablePitchZoomCoupling)
            {
                var distanceNormalized = Mathf.InverseLerp(
                    configuration.MinDistance,
                    configuration.MaxDistance,
                    _targetDistance);

                _pitch = Mathf.Lerp(
                    configuration.MinPitchAngle,
                    configuration.MaxPitchAngle,
                    distanceNormalized);
            }
            else
            {
                _pitch = Mathf.Clamp(_pitch, configuration.MinPitchAngle, configuration.MaxPitchAngle);
            }

            var dt = Mathf.Max(0.0001f, deltaTime);

            if (snapPose)
            {
                _currentDistance = _targetDistance;
                _positionVelocity = Vector3.zero;
                _distanceVelocity = 0f;

                var snapRotation = Quaternion.Euler(_pitch, _yaw, 0f);
                var snapPosition = focusPoint - (snapRotation * Vector3.forward * _currentDistance);
                return (snapPosition, snapRotation);
            }

            var zoomSmoothTime = Mathf.Max(0.001f, configuration.ZoomSmoothTime);
            _currentDistance = Mathf.SmoothDamp(
                _currentDistance,
                _targetDistance,
                ref _distanceVelocity,
                zoomSmoothTime,
                float.PositiveInfinity,
                dt);

            var targetRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            var desiredPosition = focusPoint - (targetRotation * Vector3.forward * _currentDistance);

            var positionSmoothTime = Mathf.Max(0.001f, configuration.PositionSmoothTime);
            var smoothedPosition = Vector3.SmoothDamp(
                currentPosition,
                desiredPosition,
                ref _positionVelocity,
                positionSmoothTime,
                float.PositiveInfinity,
                dt);

            var rotationLerpSpeed = Mathf.Max(0f, configuration.RotationLerpSpeed);
            var smoothedRotation = Quaternion.Slerp(
                currentRotation,
                targetRotation,
                rotationLerpSpeed * dt);

            return (smoothedPosition, smoothedRotation);
        }
    }
}
