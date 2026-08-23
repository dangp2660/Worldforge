using System;
using Worldforge.Character.State.Animation;

namespace Worldforge.Character.State.States
{
    /// <summary>
    /// Character is standing still with no move input while grounded.
    /// Entry condition: IsAlive &amp;&amp; IsGrounded &amp;&amp; !HasMoveInput.
    /// </summary>
    internal sealed class IdleState : CharacterStateBase
    {
        private readonly Action<CharacterAnimationIntent> _onAnimationIntent;

        public IdleState(Action<CharacterAnimationIntent> onAnimationIntent = null)
        {
            _onAnimationIntent = onAnimationIntent;
        }

        public override CharacterStateId StateId => CharacterStateId.Idle;

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
                stateId: CharacterStateId.Idle,
                locomotionSpeed: 0f,
                isGrounded: context.IsGrounded,
                isAirborne: false,
                isSprinting: false,
                isInteracting: false,
                isDead: false);

            _onAnimationIntent(intent);
        }
    }
}
