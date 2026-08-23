using System;
using Worldforge.Character.State.Events;

namespace Worldforge.Character.State
{
    /// <summary>
    /// Public API for the character state system, exposed to other modules.
    /// External systems (AI, Ability, UI) use this interface to read and observe state.
    /// Only authoritative gameplay systems should call <see cref="ForceTransitionTo"/>.
    /// </summary>
    public interface ICharacterStateService
    {
        /// <summary>The currently active gameplay state.</summary>
        CharacterStateId CurrentStateId { get; }

        /// <summary>
        /// Fired when the gameplay state changes.
        /// Payload is immutable — safe to read from multiple subscribers.
        /// </summary>
        event Action<CharacterStateChangedEvent> StateChanged;

        /// <summary>Returns true if the character is currently in the given state.</summary>
        bool IsInState(CharacterStateId stateId);

        /// <summary>
        /// Forces an immediate transition to the target state.
        /// Only call from authoritative gameplay systems (Health, Interaction, etc.).
        /// Do not use to drive animation.
        /// </summary>
        void ForceTransitionTo(CharacterStateId stateId);
    }
}
