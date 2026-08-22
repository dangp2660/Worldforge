using UnityEngine;
using UnityEngine.InputSystem;
using Worldforge.Character.Traversal;
using Worldforge.Core.Services;

namespace Worldforge.Character.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterMovementController : MonoBehaviour
    {
        private CharacterMotor _motor;
        private CharacterController _characterController;
        private TraversalConfiguration _traversalConfiguration;
        private ILogService _logger;

        private InputAction _moveAction;
        private InputAction _sprintAction;

        private float _rotationSpeed;
        private float _groundCheckRadius;
        private bool _isInitialized;

        private bool _wasGrounded;
        private bool _wasSprinting;
        private bool _wasMoving;
        private SurfaceType _wasSurfaceType = SurfaceType.Default;

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

        public TraversalCheckResult LastTraversalResult
        {
            get { return _motor != null ? _motor.LastTraversalResult : TraversalCheckResult.DefaultAllowed; }
        }

        public bool HasTraversalSystem
        {
            get { return _motor != null && _motor.HasTraversalSystem; }
        }

        public void Initialize(
            CharacterMovementConfiguration configuration,
            ILogService logger,
            TraversalConfiguration traversalConfiguration = null)
        {
            _logger = logger;

            _characterController = GetComponent<CharacterController>();

            if (_characterController == null)
            {
                _characterController = gameObject.AddComponent<CharacterController>();
            }

            var extraColliders = GetComponents<Collider>();
            for (var i = 0; i < extraColliders.Length; i++)
            {
                if (!(extraColliders[i] is CharacterController))
                {
                    extraColliders[i].enabled = false;
                }
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

            _traversalConfiguration = traversalConfiguration ?? Resources.Load<TraversalConfiguration>("TraversalConfiguration");

            if (_traversalConfiguration != null)
            {
                var traversalEvaluator = new TraversalEvaluator(_traversalConfiguration);
                var surfaceDetector = new SurfaceDetector();
                _motor.SetTraversalSystem(traversalEvaluator, surfaceDetector);

                _logger?.Info(
                    "Gameplay.CharacterMovement",
                    $"Traversal system initialized for '{gameObject.name}'.");

                Debug.Log(
                    $"<color=#55FF55><b>[Worldforge Traversal]</b></color> Traversal system ACTIVE on '{gameObject.name}'. " +
                    $"Rules: {_traversalConfiguration.SurfaceRules?.Length ?? 0}, MaxSlope: {_traversalConfiguration.DefaultMaxSlopeAngle}°");
            }
            else
            {
                Debug.LogWarning(
                    $"<color=#FFAA55><b>[Worldforge Traversal]</b></color> Traversal configuration is NULL on '{gameObject.name}'. " +
                    "Traversal system is disabled.");
            }

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

        private Collider _lastControllerHitCollider;

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

            var feetPosition = transform.position + _characterController.center - Vector3.up * (_characterController.height * 0.5f);

            var frameInput = new MovementFrameInput(
                rawInput,
                isSprinting,
                cameraTransform,
                feetPosition,
                _groundCheckRadius,
                Time.deltaTime);

            var displacement = _motor.CalculateMovement(frameInput, _lastControllerHitCollider);

            _characterController.Move(displacement);

            ApplyOrientation(rawInput, cameraTransform);

            LogMovementInfo(rawInput, displacement, isSprinting);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider == null)
            {
                return;
            }

            _lastControllerHitCollider = hit.collider;
        }

        private void LogMovementInfo(Vector2 rawInput, Vector3 displacement, bool currentIsSprinting)
        {
            var currentVelocity = CurrentVelocity;
            var horizontalSpeed = new Vector2(currentVelocity.x, currentVelocity.z).magnitude;
            var isMoving = rawInput.sqrMagnitude > 0.01f || horizontalSpeed > 0.01f;

            var traversalResult = LastTraversalResult;
            var currentSurface = traversalResult.SurfaceType;
            var surfaceChanged = currentSurface != _wasSurfaceType;
            var moveStateChanged = isMoving != _wasMoving;
            var sprintChanged = currentIsSprinting != _wasSprinting;

            if (surfaceChanged || moveStateChanged || (isMoving && sprintChanged))
            {
                var displaySpeed = isMoving ? horizontalSpeed : 0f;
                string status;

                if (!traversalResult.IsTraversable)
                {
                    status = $"Blocked ({currentSurface})";
                    displaySpeed = 0f;
                }
                else if (isMoving)
                {
                    status = currentIsSprinting ? "Sprinting" : "Moving";
                }
                else
                {
                    status = "Idle";
                }

                var logMessage = $"[Movement] {status} | Surface: {currentSurface} | Speed: {displaySpeed:F2} m/s (x{traversalResult.EffectiveSpeedMultiplier:F2})";

                _logger?.Info("Gameplay.CharacterMovement", logMessage);
                Debug.Log(logMessage);

                _wasMoving = isMoving;
                _wasSprinting = currentIsSprinting;
                _wasSurfaceType = currentSurface;
                _wasGrounded = IsGrounded;
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
