namespace Worldforge.Character.State
{
    /// <summary>
    /// Abstract base helper for character states.
    /// Provides no-op default implementations so subclasses only override what they need.
    /// Only inherit when a stable is-a relationship exists — do not use purely for code reuse.
    /// </summary>
    public abstract class CharacterStateBase : ICharacterState
    {
        /// <inheritdoc/>
        public abstract CharacterStateId StateId { get; }

        /// <inheritdoc/>
        public virtual void OnEnter(in CharacterStateContext context) { }

        /// <inheritdoc/>
        public virtual void OnTick(in CharacterStateContext context, float deltaTime) { }

        /// <inheritdoc/>
        public virtual void OnExit(in CharacterStateContext context) { }
    }
}
