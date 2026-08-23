namespace Worldforge.Character.State.Animation
{
    /// <summary>
    /// Adapter contract for receiving animation intent from the gameplay state machine.
    /// Implemented by a Presentation Layer MonoBehaviour (e.g. AnimationController).
    /// Decouples the state machine from the Animator — Standard §19.
    /// The Animator has no write-back authority over gameplay state.
    /// </summary>
    public interface ICharacterAnimationDriver
    {
        /// <summary>
        /// Receives the animation intent and applies it to the Animator (parameters, blend trees, etc.).
        /// Called each frame after the state machine completes its tick.
        /// </summary>
        void ApplyIntent(in CharacterAnimationIntent intent);
    }
}
