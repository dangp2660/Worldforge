using UnityEngine;
using UnityEngine.InputSystem;
using Worldforge.Character.Movement;
using Worldforge.Character.State.Animation;

namespace Worldforge.Character.State
{
    /// <summary>
    /// Unity Integration Boundary for the character state system.
    /// Receives Unity lifecycle callbacks, builds <see cref="CharacterStateContext"/> from movement data,
    /// and drives <see cref="RuntimeCharacterStateService"/> each frame.
    /// Contains no domain logic — adapter only.
    /// </summary>
    public sealed class CharacterStateBehaviour : MonoBehaviour, ICharacterAnimationDriver
    {
        [Header("Debug")]
        [SerializeField] private string _currentStateDebug;
        [SerializeField] private float _horizontalSpeedDebug;
        [SerializeField] private bool _isGroundedDebug;

        private RuntimeCharacterStateService _stateService;
        private CharacterMovementController _movementController;
        private InputAction _moveAction;
        private InputAction _sprintAction;
        private bool _isInitialized;

        /// <summary>Current state ID — readable from other components and the Inspector.</summary>
        public CharacterStateId CurrentStateId =>
            _stateService != null ? _stateService.CurrentStateId : CharacterStateId.None;

        /// <summary>
        /// Initializes this behaviour with the state service.
        /// Called by <see cref="CharacterStateInitializationSystem"/> after the player spawns.
        /// </summary>
        internal void Initialize(RuntimeCharacterStateService stateService)
        {
            if (stateService == null) return;

            _stateService = stateService;
            _stateService.SetAnimationDriver(this);

            _movementController = GetComponent<CharacterMovementController>();

            BindInputActions();

            var initialContext = BuildContext();
            _stateService.Start(initialContext);

            _isInitialized = true;
        }

        /// <summary>Shuts down and cleans up. Called on despawn or scene unload.</summary>
        public void Shutdown()
        {
            if (!_isInitialized) return;

            _isInitialized = false;
            ReleaseInputActions();

            _stateService?.SetAnimationDriver(null);
            _stateService = null;
            _movementController = null;
        }

        // ── ICharacterAnimationDriver ──────────────────────────────────────────

        /// <summary>
        /// Receives animation intent from active states.
        /// v0.1: writes debug fields only.
        /// v0.2+: will forward to Animator parameters.
        /// </summary>
        void ICharacterAnimationDriver.ApplyIntent(in CharacterAnimationIntent intent)
        {
            _currentStateDebug = intent.StateId.ToString();
            _horizontalSpeedDebug = intent.LocomotionSpeed;
            _isGroundedDebug = intent.IsGrounded;
        }

        // ── Unity Lifecycle ────────────────────────────────────────────────────

        private void Update()
        {
            if (!_isInitialized || _stateService == null) return;

            var context = BuildContext();
            _stateService.Tick(context);
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        // ── Context Builder ────────────────────────────────────────────────────

        private CharacterStateContext BuildContext()
        {
            var isGrounded = _movementController != null && _movementController.IsGrounded;
            var currentVelocity = _movementController != null
                ? _movementController.CurrentVelocity
                : Vector3.zero;

            var rawInput = _moveAction != null
                ? _moveAction.ReadValue<Vector2>()
                : Vector2.zero;

            var hasMoveInput = rawInput.sqrMagnitude > 0.01f;
            var isSprinting = _sprintAction != null && _sprintAction.IsPressed();

            return new CharacterStateContext(
                isGrounded: isGrounded,
                currentVelocity: currentVelocity,
                hasMoveInput: hasMoveInput,
                isSprinting: isSprinting,
                isAlive: true, // v0.1: always alive. Health System will update this field.
                deltaTime: Time.deltaTime);
        }

        // ── Input Binding ──────────────────────────────────────────────────────

        private void BindInputActions()
        {
            var playerMap = InputSystem.actions?.FindActionMap("Player");
            if (playerMap == null) return;

            _moveAction = playerMap.FindAction("Move");
            _sprintAction = playerMap.FindAction("Sprint");
        }

        private void ReleaseInputActions()
        {
            _moveAction = null;
            _sprintAction = null;
        }
    }
}
