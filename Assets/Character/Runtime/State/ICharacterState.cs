namespace Worldforge.Character.State
{
    /// <summary>
    /// Contract that every character gameplay state must implement.
    /// States must only contain logic within their own responsibility boundary.
    /// States must not directly control systems outside their boundary.
    /// </summary>
    public interface ICharacterState
    {
        /// <summary>Unique identifier for this state.</summary>
        CharacterStateId StateId { get; }

        /// <summary>
        /// Called once when the state machine enters this state.
        /// Use to reset internal data, start timers, or publish an initial animation intent.
        /// </summary>
        void OnEnter(in CharacterStateContext context);

        /// <summary>
        /// Called every frame while this state is active.
        /// Do not evaluate transitions here — that is the state machine's responsibility.
        /// </summary>
        void OnTick(in CharacterStateContext context, float deltaTime);

        /// <summary>
        /// Called once immediately before the state machine transitions to another state.
        /// Use to clean up, stop timers, or cancel animations.
        /// </summary>
        void OnExit(in CharacterStateContext context);
    }
}
