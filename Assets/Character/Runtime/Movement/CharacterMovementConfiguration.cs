using UnityEngine;

namespace Worldforge.Character.Movement
{
    [CreateAssetMenu(
        fileName = "CharacterMovementConfiguration",
        menuName = "Worldforge/Character/Movement Configuration")]
    public sealed class CharacterMovementConfiguration : ScriptableObject
    {
        [Header("Movement Speed")]
        [SerializeField] private float _walkSpeed = 4f;
        [SerializeField] private float _sprintSpeed = 7f;

        [Header("Rotation")]
        [SerializeField] private float _rotationSpeed = 720f;

        [Header("Gravity")]
        [SerializeField] private float _gravityMagnitude = 20f;
        [SerializeField] private float _terminalVelocity = 50f;
        [SerializeField] private float _groundedGravity = -2f;

        [Header("Ground Detection")]
        [SerializeField] private float _groundCheckDistance = 0.15f;
        [SerializeField] private float _groundCheckRadius = 0.3f;
        [SerializeField] private LayerMask _groundLayers = ~0;

        [Header("Slope")]
        [SerializeField] private float _maxSlopeAngle = 45f;

        [Header("Input")]
        [SerializeField] private float _inputDeadZone = 0.1f;

        public float WalkSpeed
        {
            get { return _walkSpeed; }
        }

        public float SprintSpeed
        {
            get { return _sprintSpeed; }
        }

        public float RotationSpeed
        {
            get { return _rotationSpeed; }
        }

        public float GravityMagnitude
        {
            get { return _gravityMagnitude; }
        }

        public float TerminalVelocity
        {
            get { return _terminalVelocity; }
        }

        public float GroundedGravity
        {
            get { return _groundedGravity; }
        }

        public float GroundCheckDistance
        {
            get { return _groundCheckDistance; }
        }

        public float GroundCheckRadius
        {
            get { return _groundCheckRadius; }
        }

        public LayerMask GroundLayers
        {
            get { return _groundLayers; }
        }

        public float MaxSlopeAngle
        {
            get { return _maxSlopeAngle; }
        }

        public float InputDeadZone
        {
            get { return _inputDeadZone; }
        }

        private void OnValidate()
        {
            _walkSpeed = Mathf.Max(0f, _walkSpeed);
            _sprintSpeed = Mathf.Max(0f, _sprintSpeed);
            _rotationSpeed = Mathf.Max(0f, _rotationSpeed);
            _gravityMagnitude = Mathf.Max(0f, _gravityMagnitude);
            _terminalVelocity = Mathf.Max(0f, _terminalVelocity);
            _groundedGravity = Mathf.Min(0f, _groundedGravity);
            _groundCheckDistance = Mathf.Max(0f, _groundCheckDistance);
            _groundCheckRadius = Mathf.Max(0.01f, _groundCheckRadius);
            _maxSlopeAngle = Mathf.Clamp(_maxSlopeAngle, 0f, 89f);
            _inputDeadZone = Mathf.Clamp01(_inputDeadZone);
        }
    }
}
