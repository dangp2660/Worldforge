using System;
using UnityEngine;

namespace Worldforge.Character.Movement
{
    public sealed class CharacterMotor
    {
        private readonly MovementInputProcessor _inputProcessor;
        private readonly GroundDetector _groundDetector;
        private readonly GravityProcessor _gravityProcessor;
        private readonly SlopeHandler _slopeHandler;

        private float _walkSpeed;
        private float _sprintSpeed;
        private float _maxSlopeAngle;
        private float _groundCheckDistance;
        private LayerMask _groundLayers;

        private Vector3 _currentVelocity;
        private bool _isGrounded;
        private GroundCheckResult _lastGroundResult;

        public CharacterMotor(
            MovementInputProcessor inputProcessor,
            GroundDetector groundDetector,
            GravityProcessor gravityProcessor,
            SlopeHandler slopeHandler)
        {
            _inputProcessor = inputProcessor ?? throw new ArgumentNullException(nameof(inputProcessor));
            _groundDetector = groundDetector ?? throw new ArgumentNullException(nameof(groundDetector));
            _gravityProcessor = gravityProcessor ?? throw new ArgumentNullException(nameof(gravityProcessor));
            _slopeHandler = slopeHandler ?? throw new ArgumentNullException(nameof(slopeHandler));
        }

        public Vector3 CurrentVelocity
        {
            get { return _currentVelocity; }
        }

        public bool IsGrounded
        {
            get { return _isGrounded; }
        }

        public GroundCheckResult LastGroundResult
        {
            get { return _lastGroundResult; }
        }

        public float WalkSpeed
        {
            get { return _walkSpeed; }
            set { _walkSpeed = Mathf.Max(0f, value); }
        }

        public float SprintSpeed
        {
            get { return _sprintSpeed; }
            set { _sprintSpeed = Mathf.Max(0f, value); }
        }

        public float MaxSlopeAngle
        {
            get { return _maxSlopeAngle; }
            set { _maxSlopeAngle = Mathf.Clamp(value, 0f, 89f); }
        }

        public float GroundCheckDistance
        {
            get { return _groundCheckDistance; }
            set { _groundCheckDistance = Mathf.Max(0f, value); }
        }

        public LayerMask GroundLayers
        {
            get { return _groundLayers; }
            set { _groundLayers = value; }
        }

        public Vector3 CalculateMovement(MovementFrameInput frameInput)
        {
            _lastGroundResult = _groundDetector.Detect(
                frameInput.CharacterPosition,
                frameInput.CharacterRadius,
                _groundCheckDistance,
                _groundLayers);

            _isGrounded = _lastGroundResult.IsGrounded
                && _lastGroundResult.SlopeAngle <= _maxSlopeAngle;

            var worldDirection = _inputProcessor.ProcessInput(
                frameInput.RawInput,
                frameInput.CameraTransform);

            var speed = frameInput.IsSprinting ? _sprintSpeed : _walkSpeed;
            var horizontalMovement = worldDirection * speed;

            var verticalVelocity = _gravityProcessor.Update(_isGrounded, frameInput.DeltaTime);

            var movement = new Vector3(
                horizontalMovement.x,
                verticalVelocity,
                horizontalMovement.z);

            movement = _slopeHandler.AdjustMovement(movement, _lastGroundResult, _maxSlopeAngle);

            _currentVelocity = movement;

            return movement * frameInput.DeltaTime;
        }

        public void ApplyConfiguration(CharacterMovementConfiguration configuration)
        {
            if (configuration == null)
            {
                return;
            }

            _walkSpeed = configuration.WalkSpeed;
            _sprintSpeed = configuration.SprintSpeed;
            _maxSlopeAngle = configuration.MaxSlopeAngle;
            _groundCheckDistance = configuration.GroundCheckDistance;
            _groundLayers = configuration.GroundLayers;

            _inputProcessor.DeadZone = configuration.InputDeadZone;
            _gravityProcessor.GravityMagnitude = configuration.GravityMagnitude;
            _gravityProcessor.TerminalVelocity = configuration.TerminalVelocity;
            _gravityProcessor.GroundedGravity = configuration.GroundedGravity;
        }

        public void Reset()
        {
            _currentVelocity = Vector3.zero;
            _isGrounded = false;
            _lastGroundResult = GroundCheckResult.NotGrounded;
            _gravityProcessor.Reset();
        }
    }
}
