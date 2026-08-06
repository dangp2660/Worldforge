using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Worldforge.Core.Services;

namespace Worldforge.Infrastructure.Cameras
{
    [DisallowMultipleComponent]
    public sealed class RuntimeCameraController : MonoBehaviour
    {
        private readonly CameraInputProcessor _inputProcessor = new();
        private readonly CameraMovementProcessor _movementProcessor = new();

        private CameraFollowConfiguration _configuration;
        private Func<Transform> _targetProvider;
        private Transform _followTarget;
        private ILogService _logger;

        private InputAction _lookAction;
        private bool _hasAppliedInitialPose;

        private bool _wasOrbiting;
        private bool _wasZooming;
        private Transform _lastTarget;

        public Transform FollowTarget
        {
            get { return _followTarget; }
        }

        public CameraFollowConfiguration Configuration
        {
            get { return _configuration; }
        }

        public CameraInputProcessor InputProcessor
        {
            get { return _inputProcessor; }
        }

        public CameraMovementProcessor MovementProcessor
        {
            get { return _movementProcessor; }
        }

        public float Pitch
        {
            get { return _movementProcessor.Pitch; }
        }

        public float Yaw
        {
            get { return _movementProcessor.Yaw; }
        }

        public float CurrentDistance
        {
            get { return _movementProcessor.CurrentDistance; }
        }

        private void OnEnable()
        {
            BindInputActions();
            if (_configuration != null && _configuration.LockCursor)
            {
                SetCursorLock(true);
            }
        }

        private void OnDisable()
        {
            ReleaseInputActions();
            SetCursorLock(false);
        }

        public static void SetCursorLock(bool isLocked)
        {
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
        }

        public void SetLogger(ILogService logger)
        {
            _logger = logger;
            _logger?.Info(
                "Infrastructure.Camera",
                $"RuntimeCameraController initialized for target '{(_followTarget != null ? _followTarget.name : "None")}'.");
        }

        public void ApplyConfiguration(CameraFollowConfiguration configuration)
        {
            if (configuration == null)
            {
                return;
            }

            _configuration = configuration;
            _inputProcessor.ApplyConfiguration(configuration);

            if (_configuration.LockCursor && !_configuration.RequireRightMouseToRotate)
            {
                SetCursorLock(true);
            }

            if (!_hasAppliedInitialPose)
            {
                var initialPitch = Mathf.Clamp(45f, configuration.MinPitchAngle, configuration.MaxPitchAngle);
                _movementProcessor.SetPose(initialPitch, transform.eulerAngles.y, configuration.DefaultDistance);
            }
        }

        public void SetTargetProvider(Func<Transform> provider)
        {
            _targetProvider = provider;
            RefreshFollowTarget();
        }

        public void SetFollowTarget(Transform target)
        {
            _followTarget = target;
            _movementProcessor.ResetVelocity();
            _hasAppliedInitialPose = false;

            if (_followTarget != null && _configuration != null)
            {
                var initialDistance = _configuration.DefaultDistance;
                var initialYaw = _followTarget.eulerAngles.y;
                var initialPitch = Mathf.Clamp(45f, _configuration.MinPitchAngle, _configuration.MaxPitchAngle);
                _movementProcessor.SetPose(initialPitch, initialYaw, initialDistance);
            }
        }

        public void ClearFollowTarget()
        {
            _targetProvider = null;
            _followTarget = null;
            _movementProcessor.ResetVelocity();
            _hasAppliedInitialPose = false;
            SetCursorLock(false);
        }

        public void SetOrbitAngles(float pitch, float yaw)
        {
            _movementProcessor.SetPose(pitch, yaw, _movementProcessor.TargetDistance);
        }

        public void SetZoomDistance(float distance)
        {
            _movementProcessor.SetPose(_movementProcessor.Pitch, _movementProcessor.Yaw, distance);
        }

        public void ResetPose()
        {
            if (_configuration == null)
            {
                return;
            }

            var defaultYaw = _followTarget != null ? _followTarget.eulerAngles.y : 0f;
            var defaultPitch = Mathf.Clamp(45f, _configuration.MinPitchAngle, _configuration.MaxPitchAngle);
            _movementProcessor.SetPose(defaultPitch, defaultYaw, _configuration.DefaultDistance);
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

            var rawLook = ReadRawLookInput();
            var rawScroll = ReadRawScrollInput();

            var inputResult = _inputProcessor.ProcessInput(rawLook, rawScroll);

            var shouldSnap = _configuration != null
                && _configuration.SnapOnTargetAcquire
                && !_hasAppliedInitialPose;

            var (newPosition, newRotation) = _movementProcessor.CalculatePose(
                transform.position,
                transform.rotation,
                _followTarget,
                inputResult,
                _configuration,
                Time.deltaTime,
                shouldSnap);

            transform.SetPositionAndRotation(newPosition, newRotation);
            _hasAppliedInitialPose = true;

            LogCameraInfo(inputResult, newPosition);
        }

        private void LogCameraInfo(CameraInputResult inputResult, Vector3 position)
        {
            if (_logger == null)
            {
                return;
            }

            var isOrbiting = Mathf.Abs(inputResult.YawDelta) > 0.001f || Mathf.Abs(inputResult.PitchDelta) > 0.001f;
            var isZooming = Mathf.Abs(inputResult.ZoomDelta) > 0.001f;

            var targetChanged = _followTarget != _lastTarget;
            var orbitChanged = isOrbiting != _wasOrbiting;
            var zoomChanged = isZooming != _wasZooming;

            if (targetChanged || orbitChanged || zoomChanged)
            {
                string eventReason;

                if (targetChanged)
                {
                    var targetName = _followTarget != null ? _followTarget.name : "None";
                    eventReason = $"Target Changed: {targetName}";
                }
                else if (orbitChanged)
                {
                    eventReason = isOrbiting ? "Start Orbiting" : "Stop Orbiting";
                }
                else
                {
                    eventReason = isZooming ? "Start Zooming" : "Stop Zooming";
                }

                _logger.Info(
                    "Infrastructure.Camera",
                    $"[{eventReason}] Pitch: {Pitch:F1}° | Yaw: {Yaw:F1}° | Distance: {CurrentDistance:F2}m | Pos: {position}");

                _wasOrbiting = isOrbiting;
                _wasZooming = isZooming;
                _lastTarget = _followTarget;
            }
        }

        private Vector2 ReadRawLookInput()
        {
            var requireRmb = _configuration != null && _configuration.RequireRightMouseToRotate;
            var isRmbPressed = Mouse.current != null && Mouse.current.rightButton.isPressed;

            if (requireRmb)
            {
                if (isRmbPressed)
                {
                    SetCursorLock(true);
                    if (_lookAction != null && _lookAction.enabled)
                    {
                        return _lookAction.ReadValue<Vector2>();
                    }
                }
                else
                {
                    if (_configuration != null && !_configuration.LockCursor)
                    {
                        SetCursorLock(false);
                    }
                    return Vector2.zero;
                }
            }
            else
            {
                if (_configuration != null && _configuration.LockCursor)
                {
                    SetCursorLock(true);
                }

                if (_lookAction != null && _lookAction.enabled)
                {
                    return _lookAction.ReadValue<Vector2>();
                }
            }

            return Vector2.zero;
        }

        private static float ReadRawScrollInput()
        {
            var scrollY = 0f;

            if (Mouse.current != null)
            {
                scrollY = Mouse.current.scroll.ReadValue().y;
            }

            if (Mathf.Abs(scrollY) < 0.001f && Input.mousePresent)
            {
                scrollY = Input.mouseScrollDelta.y;
            }

            if (Mathf.Abs(scrollY) < 0.001f)
            {
                return 0f;
            }

            if (Mathf.Abs(scrollY) >= 10f)
            {
                scrollY /= 120f;
            }

            return scrollY;
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

        private void BindInputActions()
        {
            var playerActionMap = InputSystem.actions?.FindActionMap("Player");
            if (playerActionMap == null)
            {
                return;
            }

            _lookAction = playerActionMap.FindAction("Look");
        }

        private void ReleaseInputActions()
        {
            _lookAction = null;
        }

        private void OnDestroy()
        {
            ClearFollowTarget();
            ReleaseInputActions();
        }
    }
}
