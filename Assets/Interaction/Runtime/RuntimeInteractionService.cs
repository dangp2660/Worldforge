using System;
using System.Collections.Generic;
using UnityEngine;
using Worldforge.Character.State;
using Worldforge.Core.Services;
using Worldforge.Interaction.Detection;
using Worldforge.Interaction.Events;

namespace Worldforge.Interaction
{
    // Runtime implementation of IInteractionService.
    // Owns interaction lifecycle, handler registry, cooldown, and timed interaction state.
    // Delegates to ICharacterStateService for state transitions.
    internal sealed class RuntimeInteractionService : IInteractionService, IInteractionServiceInternal, IDisposable
    {
        private readonly ICharacterStateService _stateService;
        private readonly InteractionConfiguration _configuration;
        private readonly ILogService _logger;
        private readonly List<IInteractionHandler> _handlers = new List<IInteractionHandler>();

        private InteractionContext _activeContext;
        private IInteractionHandler _activeHandler;
        private float _interactionElapsed;
        private float _interactionDuration;
        private float _lastInteractionTime;
        private bool _isInteracting;

        public event Action<InteractionStartedEvent> InteractionStarted;
        public event Action<InteractionCompletedEvent> InteractionCompleted;
        public event Action<InteractionCancelledEvent> InteractionCancelled;

        public RuntimeInteractionService(
            ICharacterStateService stateService,
            InteractionConfiguration configuration,
            ILogService logger)
        {
            _stateService = stateService;
            _configuration = configuration;
            _logger = logger;
        }

        // ── IInteractionService ───────────────────────────────────────────────

        public bool IsInteracting
        {
            get { return _isInteracting; }
        }

        public InteractionTarget CurrentTarget { get; private set; }

        public InteractionResult RequestInteraction(GameObject interactor, IInteractable target)
        {
            if (interactor == null)
                return InteractionResult.Fail("Interactor is null.");

            if (target == null)
                return InteractionResult.Fail("Target is null.");

            if (_isInteracting)
                return InteractionResult.Fail("Already interacting.");

            if (!target.IsInteractable)
                return InteractionResult.Fail("Target is not interactable.");

            // Cooldown check
            if (_configuration != null && _configuration.InteractionCooldown > 0f)
            {
                if (Time.time - _lastInteractionTime < _configuration.InteractionCooldown)
                    return InteractionResult.Fail("Interaction on cooldown.");
            }

            // Range check with tolerance for bounds and height difference
            if (_configuration != null && target.InteractionPoint != null)
            {
                var distance = Vector3.Distance(interactor.transform.position, target.InteractionPoint.position);
                var maxAllowed = _configuration.MaxDetectionDistance + 0.5f;
                if (distance > maxAllowed)
                    return InteractionResult.Fail("Target out of range.");
            }

            // Character state check
            if (_stateService != null)
            {
                var currentState = _stateService.CurrentStateId;
                if (currentState == CharacterStateId.Dead)
                    return InteractionResult.Fail("Character is dead.");

                if (currentState == CharacterStateId.Airborne)
                    return InteractionResult.Fail("Character is airborne.");
            }

            // Find handler
            var handler = FindHandler(target.Type);
            if (handler == null)
            {
                // No handler: execute default interaction (instant)
                return ExecuteDefaultInteraction(interactor, target);
            }

            // Handler validation
            var context = new InteractionContext(interactor, target, target.Type, Time.time);
            var validateResult = handler.Validate(context);
            if (!validateResult.IsSuccess)
                return validateResult;

            // Start interaction
            _activeContext = context;
            _activeHandler = handler;
            _interactionDuration = target.InteractionDuration;
            _interactionElapsed = 0f;
            _isInteracting = true;

            // Transition character to Interacting state
            _stateService?.ForceTransitionTo(CharacterStateId.Interacting);

            // Notify target
            target.OnInteractionStarted(context);

            // Publish event
            InteractionStarted?.Invoke(new InteractionStartedEvent(context));

            _logger?.Info("Gameplay.Interaction",
                $"Interaction started: {context}");

            // Instant interaction — complete immediately
            if (_interactionDuration <= 0f)
            {
                CompleteInteraction();
            }

            return InteractionResult.Success();
        }

        public void CancelInteraction()
        {
            if (!_isInteracting) return;

            var context = _activeContext;
            var handler = _activeHandler;

            EndInteraction();

            // Notify handler
            handler?.Cancel(context);

            // Notify target
            context.Target?.OnInteractionCancelled(context);

            // Publish event
            InteractionCancelled?.Invoke(new InteractionCancelledEvent(context, "Cancelled by player"));

            _logger?.Info("Gameplay.Interaction",
                $"Interaction cancelled: {context}");
        }

        public void RegisterHandler(IInteractionHandler handler)
        {
            if (handler == null) return;
            if (_handlers.Contains(handler)) return;
            _handlers.Add(handler);
        }

        public void UnregisterHandler(IInteractionHandler handler)
        {
            if (handler == null) return;
            _handlers.Remove(handler);
        }

        // ── IInteractionServiceInternal ───────────────────────────────────────

        void IInteractionServiceInternal.AttachToBehaviour(InteractionBehaviour behaviour)
        {
            if (behaviour == null) return;
            behaviour.Initialize(this, _configuration);
        }

        // ── Tick ──────────────────────────────────────────────────────────────

        // Called each frame by InteractionBehaviour when a timed interaction is active.
        internal void Tick(float deltaTime)
        {
            if (!_isInteracting) return;
            if (_interactionDuration <= 0f) return;

            _interactionElapsed += deltaTime;

            if (_interactionElapsed >= _interactionDuration)
            {
                CompleteInteraction();
            }
        }

        // Exposes interaction progress (0..1) for UI progress bars.
        internal float InteractionProgress
        {
            get
            {
                if (!_isInteracting || _interactionDuration <= 0f) return 0f;
                return Mathf.Clamp01(_interactionElapsed / _interactionDuration);
            }
        }

        // Called by InteractionBehaviour to update the current detection target.
        internal void SetCurrentTarget(InteractionTarget target)
        {
            CurrentTarget = target;
        }

        // ── IDisposable ──────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_isInteracting)
            {
                CancelInteraction();
            }

            _handlers.Clear();
            InteractionStarted = null;
            InteractionCompleted = null;
            InteractionCancelled = null;

            _logger?.Info("Gameplay.Interaction", "Interaction service disposed.");
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void CompleteInteraction()
        {
            var context = _activeContext;
            var handler = _activeHandler;

            EndInteraction();

            // Execute handler logic
            var executeResult = handler?.Execute(context) ?? InteractionResult.Success();

            // Notify target
            context.Target?.OnInteractionCompleted(context);

            // Publish event
            InteractionCompleted?.Invoke(new InteractionCompletedEvent(context));

            _logger?.Info("Gameplay.Interaction",
                $"Interaction completed: {context} Result: {executeResult}");
        }

        private void EndInteraction()
        {
            _isInteracting = false;
            _activeHandler = null;
            _lastInteractionTime = Time.time;
            _interactionElapsed = 0f;
            _interactionDuration = 0f;

            // Return character to Idle state
            if (_stateService != null && _stateService.CurrentStateId == CharacterStateId.Interacting)
            {
                _stateService.ForceTransitionTo(CharacterStateId.Idle);
            }
        }

        private InteractionResult ExecuteDefaultInteraction(GameObject interactor, IInteractable target)
        {
            var context = new InteractionContext(interactor, target, target.Type, Time.time);

            _activeContext = context;
            _activeHandler = null;
            _interactionDuration = target.InteractionDuration;
            _interactionElapsed = 0f;
            _isInteracting = true;

            _stateService?.ForceTransitionTo(CharacterStateId.Interacting);
            target.OnInteractionStarted(context);
            InteractionStarted?.Invoke(new InteractionStartedEvent(context));

            _logger?.Info("Gameplay.Interaction",
                $"Default interaction started: {context}");

            if (_interactionDuration <= 0f)
            {
                CompleteInteraction();
            }

            return InteractionResult.Success();
        }

        private IInteractionHandler FindHandler(InteractionType type)
        {
            for (var i = 0; i < _handlers.Count; i++)
            {
                if (_handlers[i].CanHandle(type))
                    return _handlers[i];
            }

            return null;
        }
    }
}
