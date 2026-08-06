using System;
using UnityEngine;

namespace Worldforge.Infrastructure.Cameras
{
    public sealed class CameraInputProcessor
    {
        private float _deadZone = 0.01f;
        private float _sensitivityX = 0.15f;
        private float _sensitivityY = 0.15f;
        private float _zoomSensitivity = 1.5f;
        private bool _isPitchInverted;
        private bool _isYawInverted;

        public CameraInputProcessor()
        {
        }

        public CameraInputProcessor(
            float deadZone,
            float sensitivityX,
            float sensitivityY,
            float zoomSensitivity,
            bool isPitchInverted,
            bool isYawInverted)
        {
            _deadZone = Mathf.Clamp(deadZone, 0f, 0.5f);
            _sensitivityX = Mathf.Max(0.001f, sensitivityX);
            _sensitivityY = Mathf.Max(0.001f, sensitivityY);
            _zoomSensitivity = Mathf.Max(0.001f, zoomSensitivity);
            _isPitchInverted = isPitchInverted;
            _isYawInverted = isYawInverted;
        }

        public float DeadZone
        {
            get { return _deadZone; }
            set { _deadZone = Mathf.Clamp(value, 0f, 0.5f); }
        }

        public float SensitivityX
        {
            get { return _sensitivityX; }
            set { _sensitivityX = Mathf.Max(0.001f, value); }
        }

        public float SensitivityY
        {
            get { return _sensitivityY; }
            set { _sensitivityY = Mathf.Max(0.001f, value); }
        }

        public float ZoomSensitivity
        {
            get { return _zoomSensitivity; }
            set { _zoomSensitivity = Mathf.Max(0.001f, value); }
        }

        public bool IsPitchInverted
        {
            get { return _isPitchInverted; }
            set { _isPitchInverted = value; }
        }

        public bool IsYawInverted
        {
            get { return _isYawInverted; }
            set { _isYawInverted = value; }
        }

        public void ApplyConfiguration(CameraFollowConfiguration configuration)
        {
            if (configuration == null)
            {
                return;
            }

            _deadZone = configuration.InputDeadZone;
            _sensitivityX = configuration.MouseSensitivityX;
            _sensitivityY = configuration.MouseSensitivityY;
            _zoomSensitivity = configuration.ZoomSensitivity;
            _isPitchInverted = configuration.IsPitchInverted;
            _isYawInverted = configuration.IsYawInverted;
        }

        public CameraInputResult ProcessInput(Vector2 rawLook, float rawScroll)
        {
            var look = rawLook;

            if (look.sqrMagnitude < _deadZone * _deadZone)
            {
                look = Vector2.zero;
            }

            var yawMultiplier = _isYawInverted ? -1f : 1f;
            var pitchMultiplier = _isPitchInverted ? 1f : -1f;

            var yawDelta = look.x * _sensitivityX * yawMultiplier;
            var pitchDelta = look.y * _sensitivityY * pitchMultiplier;
            var zoomDelta = rawScroll * _zoomSensitivity;

            return new CameraInputResult(yawDelta, pitchDelta, zoomDelta);
        }
    }
}
