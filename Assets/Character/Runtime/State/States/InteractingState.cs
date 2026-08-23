using System;
using Worldforge.Character.State.Animation;

namespace Worldforge.Character.State.States
{
    /// <summary>
    /// Placeholder state for character interactions with world objects.
    /// Registered in the transition table now so other systems can request it via ForceTransition.
    /// Will be expanded when the Interaction System is implemented.
    /// Entry and exit are controlled exclusively by ForceTransition from the Interaction System.
    /// </summary>
    internal sealed class InteractingState : CharacterStateBase
    {
        private readonly Action<CharacterAnimationIntent> _onAnimationIntent;

        public InteractingState(Action<CharacterAnimationIntent> onAnimationIntent = null)
        {
            _onAnimationIntent = onAnimationIntent;
        }

        public override CharacterStateId StateId => CharacterStateId.Interacting;

        public override void OnEnter(in CharacterStateContext context)
        {
            PublishAnimationIntent(context);
        }

        public override void OnExit(in CharacterStateContext context) { }

        private void PublishAnimationIntent(in CharacterStateContext context)
        {
            if (_onAnimationIntent == null)
            {
                return;
            }

            var intent = new CharacterAnimationIntent(
                stateId: CharacterStateId.Interacting,
                locomotionSpeed: 0f,
                isGrounded: context.IsGrounded,
                isAirborne: false,
                isSprinting: false,
                isInteracting: true,
                isDead: false);

            _onAnimationIntent(intent);
        }
    }
}
