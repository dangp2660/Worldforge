using System;
using System.Collections.Generic;
using UnityEngine;
using Worldforge.Character.State.Events;
using Worldforge.Character.State.Transition;

namespace Worldforge.Character.State
{
    /// <summary>
    /// Core orchestrator for the character gameplay state machine.
    /// Owns the active state, evaluates transitions each tick, and fires the StateChanged event.
    /// No MonoBehaviour, Animator, or Unity Presentation API dependency.
    /// Must be ticked externally by <see cref="CharacterStateBehaviour"/>.
    /// </summary>
    public sealed class CharacterStateMachine
    {
        private readonly Dictionary<CharacterStateId, ICharacterState> _states;
        private readonly CharacterTransitionRegistry _transitionRegistry;

        private ICharacterState _currentState;

        /// <summary>
        /// Fired when the gameplay state changes.
        /// Typed struct payload — no string identifiers per Standard §18.
        /// </summary>
        public event Action<CharacterStateChangedEvent> StateChanged;

        public CharacterStateMachine(
            IEnumerable<ICharacterState> states,
            CharacterTransitionRegistry transitionRegistry,
            CharacterStateId initialStateId = CharacterStateId.Idle)
        {
            if (states == null) throw new ArgumentNullException(nameof(states));
            if (transitionRegistry == null) throw new ArgumentNullException(nameof(transitionRegistry));

            _states = new Dictionary<CharacterStateId, ICharacterState>();

            foreach (var state in states)
            {
                if (state == null) continue;
                _states[state.StateId] = state;
            }

            _transitionRegistry = transitionRegistry;

            // Set initial state without calling OnEnter — context is not yet available.
            _currentState = _states.TryGetValue(initialStateId, out var initial) ? initial : null;
        }

        /// <summary>ID of the currently active gameplay state.</summary>
        public CharacterStateId CurrentStateId =>
            _currentState != null ? _currentState.StateId : CharacterStateId.None;

        /// <summary>
        /// Advances the state machine by one frame.
        /// Order: evaluate transitions → execute if resolved → tick active state.
        /// </summary>
        public void Tick(in CharacterStateContext context)
        {
            if (_currentState == null) return;

            var resolvedTransition = EvaluateTransitions(context);

            if (resolvedTransition.HasValue)
            {
                ExecuteTransition(resolvedTransition.Value.ToState, context);
            }
            else
            {
                _currentState.OnTick(context, context.DeltaTime);
            }
        }

        /// <summary>
        /// Forces an immediate transition to the target state, bypassing the transition table.
        /// Use for death, respawn, or externally driven state changes (e.g. from the Health System).
        /// </summary>
        public void ForceTransition(CharacterStateId targetStateId, in CharacterStateContext context)
        {
            if (!_states.ContainsKey(targetStateId)) return;
            ExecuteTransition(targetStateId, context);
        }

        /// <summary>
        /// Initializes the machine with first-frame context and calls OnEnter on the initial state.
        /// Call this after the character has spawned.
        /// </summary>
        public void Start(in CharacterStateContext context)
        {
            if (_currentState == null) return;
            _currentState.OnEnter(context);
        }

        /// <summary>Stops the machine and calls OnExit on the active state.</summary>
        public void Stop(in CharacterStateContext context)
        {
            _currentState?.OnExit(context);
        }

        /// <summary>Returns a debug string describing the current state.</summary>
        public string GetDebugInfo()
        {
            return $"CharacterStateMachine | Current: {CurrentStateId}";
        }

        private CharacterStateTransition? EvaluateTransitions(in CharacterStateContext context)
        {
            var transitions = _transitionRegistry.Transitions;

            for (var i = 0; i < transitions.Count; i++)
            {
                var transition = transitions[i];

                if (transition.ToState == CurrentStateId) continue;
                if (!_states.ContainsKey(transition.ToState)) continue;

                if (transition.Evaluate(CurrentStateId, context))
                {
                    return transition;
                }
            }

            return null;
        }

        private void ExecuteTransition(CharacterStateId targetStateId, in CharacterStateContext context)
        {
            if (!_states.TryGetValue(targetStateId, out var targetState)) return;

            var previousStateId = CurrentStateId;

            _currentState?.OnExit(context);
            _currentState = targetState;
            _currentState.OnEnter(context);

            var stateChangedEvent = new CharacterStateChangedEvent(
                previousStateId: previousStateId,
                nextStateId: targetStateId,
                timestamp: Time.time);

            StateChanged?.Invoke(stateChangedEvent);
        }
    }
}
