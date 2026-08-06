using UnityEngine;
using UnityEngine.InputSystem;
using Worldforge.Core.Services;

namespace Worldforge.Character.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterMovementController : MonoBehaviour
    {
        private CharacterMotor _motor;
        private CharacterController _characterController;
        private ILogService _logger;

        private InputAction _moveAction;
        private InputAction _sprintAction;

        private float _rotationSpeed;
        private float _groundCheckRadius;
        private bool _isInitialized;

        private bool _wasGrounded;
        private bool _wasSprinting;
        private bool _wasMoving;

        public bool IsGrounded
        {
            get { return _motor != null && _motor.IsGrounded; }
        }

        public bool IsSprinting
        {
            get { return _sprintAction != null && _sprintAction.IsPressed(); }
        }

        public Vector3 CurrentVelocity
        {
            get { return _motor != null ? _motor.CurrentVelocity : Vector3.zero; }
        }

        public void Initialize(CharacterMovementConfiguration configuration, ILogService logger)
        {
            _logger = logger;

            _characterController = GetComponent<CharacterController>();

            if (_characterController == null)
            {
                _characterController = gameObject.AddComponent<CharacterController>();
            }

            var inputProcessor = new MovementInputProcessor(configuration.InputDeadZone);
            var groundDetector = new GroundDetector();
            var gravityProcessor = new GravityProcessor(
                configuration.GravityMagnitude,
                configuration.TerminalVelocity,
                configuration.GroundedGravity);
            var slopeHandler = new SlopeHandler();

            _motor = new CharacterMotor(inputProcessor, groundDetector, gravityProcessor, slopeHandler);
            _motor.ApplyConfiguration(configuration);

            _rotationSpeed = configuration.RotationSpeed;
            _groundCheckRadius = configuration.GroundCheckRadius;

            BindInputActions();

            _isInitialized = true;

            _wasGrounded = IsGrounded;
            _wasSprinting = false;
            _wasMoving = false;

            _logger?.Info(
                "Gameplay.CharacterMovement",
                $"CharacterMovementController initialized successfully for '{gameObject.name}'.");
        }

        public void Shutdown()
        {
            if (_isInitialized)
            {
                _logger?.Info(
                    "Gameplay.CharacterMovement",
                    $"CharacterMovementController shutting down for '{gameObject.name}'.");
            }

            _isInitialized = false;

            ReleaseInputActions();

            _motor?.Reset();
            _motor = null;
            _logger = null;
        }

        private void Update()
        {
            if (!_isInitialized || _motor == null || _characterController == null)
            {
                return;
            }

            var rawInput = _moveAction != null
                ? _moveAction.ReadValue<Vector2>()
                : Vector2.zero;

            var isSprinting = _sprintAction != null && _sprintAction.IsPressed();

            var cameraTransform = Camera.main != null ? Camera.main.transform : null;

            var frameInput = new MovementFrameInput(
                rawInput,
                isSprinting,
                cameraTransform,
                transform.position,
                _groundCheckRadius,
                Time.deltaTime);

            var displacement = _motor.CalculateMovement(frameInput);

            _characterController.Move(displacement);

            ApplyOrientation(rawInput, cameraTransform);

            LogMovementInfo(rawInput, displacement, isSprinting);
        }

        private void LogMovementInfo(Vector2 rawInput, Vector3 displacement, bool currentIsSprinting)
        {
            if (_logger == null)
            {
                return;
            }

            var currentIsGrounded = IsGrounded;
            var currentVelocity = CurrentVelocity;
            var horizontalSpeedSqr = currentVelocity.x * currentVelocity.x + currentVelocity.z * currentVelocity.z;
            var isMoving = rawInput.sqrMagnitude > 0.01f || horizontalSpeedSqr > 0.001f;

            var groundedChanged = currentIsGrounded != _wasGrounded;
            var sprintChanged = currentIsSprinting != _wasSprinting;
            var moveStateChanged = isMoving != _wasMoving;

            if (groundedChanged || sprintChanged || moveStateChanged)
            {
                var currentSpeed = currentVelocity.magnitude;
                string eventReason;

                if (moveStateChanged)
                {
                    eventReason = isMoving ? "Start Moving" : "Stop Moving";
                }
                else if (groundedChanged)
                {
                    eventReason = currentIsGrounded ? "Landed" : "Airborne";
                }
                else
                {
                    eventReason = currentIsSprinting ? "Sprint Start" : "Sprint Stop";
                }

                _logger.Info(
                    "Gameplay.CharacterMovement",
                    $"[{eventReason}] Vel: {currentVelocity} | Speed: {currentSpeed:F2} m/s | Grounded: {currentIsGrounded} | Sprinting: {currentIsSprinting} | Displacement: {displacement}");

                _wasGrounded = currentIsGrounded;
                _wasSprinting = currentIsSprinting;
                _wasMoving = isMoving;
            }
        }

        private void ApplyOrientation(Vector2 rawInput, Transform cameraTransform)
        {
            if (rawInput.sqrMagnitude < 0.01f)
            {
                return;
            }

            var inputDirection = Vector3.zero;

            if (cameraTransform != null)
            {
                var cameraForward = cameraTransform.forward;
                var cameraRight = cameraTransform.right;

                cameraForward.y = 0f;
                cameraRight.y = 0f;
                cameraForward.Normalize();
                cameraRight.Normalize();

                if (cameraForward.sqrMagnitude < 0.001f)
                {
                    cameraForward = Vector3.forward;
                }

                if (cameraRight.sqrMagnitude < 0.001f)
                {
                    cameraRight = Vector3.right;
                }

                inputDirection = cameraRight * rawInput.x + cameraForward * rawInput.y;
            }
            else
            {
                inputDirection = new Vector3(rawInput.x, 0f, rawInput.y);
            }

            inputDirection.y = 0f;

            if (inputDirection.sqrMagnitude < 0.001f)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(inputDirection.normalized, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime);
        }

        private void BindInputActions()
        {
            var playerActionMap = InputSystem.actions?.FindActionMap("Player");

            if (playerActionMap == null)
            {
                _logger?.Warning(
                    "Gameplay.CharacterMovement",
                    "Player action map was not found in the project-wide input actions.");
                return;
            }

            _moveAction = playerActionMap.FindAction("Move");
            _sprintAction = playerActionMap.FindAction("Sprint");

            if (_moveAction == null)
            {
                _logger?.Warning(
                    "Gameplay.CharacterMovement",
                    "Move action was not found in the Player action map.");
            }

            if (_sprintAction == null)
            {
                _logger?.Warning(
                    "Gameplay.CharacterMovement",
                    "Sprint action was not found in the Player action map.");
            }
        }

        private void ReleaseInputActions()
        {
            _moveAction = null;
            _sprintAction = null;
        }

        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
