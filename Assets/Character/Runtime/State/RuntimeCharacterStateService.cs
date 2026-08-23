using System;
using Worldforge.Character.State.Animation;
using Worldforge.Character.State.Events;
using Worldforge.Character.State.States;
using Worldforge.Character.State.Transition;
using Worldforge.Core.Services;

namespace Worldforge.Character.State
{
    /// <summary>
    /// Runtime implementation of <see cref="ICharacterStateService"/>.
    /// Owns the <see cref="CharacterStateMachine"/> and <see cref="CharacterTransitionRegistry"/>.
    /// Ticked each frame by <see cref="CharacterStateBehaviour"/> (Unity Integration Boundary).
    /// Forwards typed state-change events and animation intents to registered consumers.
    /// </summary>
    internal sealed class RuntimeCharacterStateService : ICharacterStateService, ICharacterStateServiceInternal, IDisposable
    {
        private readonly CharacterStateMachine _stateMachine;
        private readonly ILogService _logger;

        private ICharacterAnimationDriver _animationDriver;
        private bool _isStarted;

        public event Action<CharacterStateChangedEvent> StateChanged;

        public RuntimeCharacterStateService(ILogService logger)
        {
            _logger = logger;

            var registry = CharacterTransitionRegistry.CreateDefault();

            Action<CharacterAnimationIntent> onIntent = OnAnimationIntentPublished;

            // State composition happens here — not in MonoBehaviour.
            var states = new ICharacterState[]
            {
                new IdleState(onIntent),
                new LocomotionState(onIntent),
                new AirborneState(onIntent),
                new InteractingState(onIntent),
                new DeadState(onIntent)
            };

            _stateMachine = new CharacterStateMachine(states, registry, CharacterStateId.Idle);
            _stateMachine.StateChanged += OnStateMachineStateChanged;
        }

        // ── ICharacterStateService ─────────────────────────────────────────────

        public CharacterStateId CurrentStateId => _stateMachine.CurrentStateId;

        public bool IsInState(CharacterStateId stateId) => _stateMachine.CurrentStateId == stateId;

        public void ForceTransitionTo(CharacterStateId stateId)
        {
            var context = CharacterStateContext.Default;
            _stateMachine.ForceTransition(stateId, context);
        }

        // ── ICharacterStateServiceInternal ────────────────────────────────────

        void ICharacterStateServiceInternal.AttachToBehaviour(CharacterStateBehaviour behaviour)
        {
            if (behaviour == null) return;
            behaviour.Initialize(this);
        }

        // ── Lifecycle ──────────────────────────────────────────────────────────

        /// <summary>
        /// Starts the state machine after the character has spawned.
        /// Called by <see cref="CharacterStateInitializationSystem"/>.
        /// </summary>
        public void Start(in CharacterStateContext context)
        {
            if (_isStarted) return;
            _stateMachine.Start(context);
            _isStarted = true;

            _logger?.Info(
                "Gameplay.CharacterState",
                $"Character state machine started. Initial state: {CurrentStateId}");
        }

        /// <summary>
        /// Advances the state machine by one frame.
        /// Called by <see cref="CharacterStateBehaviour"/> in Update.
        /// </summary>
        public void Tick(in CharacterStateContext context)
        {
            if (!_isStarted) return;
            _stateMachine.Tick(context);
        }

        /// <summary>
        /// Assigns an animation driver to receive intents from active states.
        /// Null is valid — the state machine operates correctly without a driver.
        /// </summary>
        public void SetAnimationDriver(ICharacterAnimationDriver driver)
        {
            _animationDriver = driver;
        }

        public void Dispose()
        {
            if (_isStarted)
            {
                _stateMachine.Stop(CharacterStateContext.Default);
            }

            _stateMachine.StateChanged -= OnStateMachineStateChanged;
            _animationDriver = null;
            _isStarted = false;

            _logger?.Info("Gameplay.CharacterState", "Character state service disposed.");
        }

        // ── Private ────────────────────────────────────────────────────────────

        private void OnStateMachineStateChanged(CharacterStateChangedEvent changedEvent)
        {
            _logger?.Info("Gameplay.CharacterState", $"State changed: {changedEvent}");
            StateChanged?.Invoke(changedEvent);
        }

        private void OnAnimationIntentPublished(CharacterAnimationIntent intent)
        {
            _animationDriver?.ApplyIntent(intent);
        }
    }
}
