using System;
using Worldforge.Character.State.Animation;

namespace Worldforge.Character.State.States
{
    /// <summary>
    /// Character is moving on the ground (walk or sprint).
    /// Entry condition: IsAlive &amp;&amp; IsGrounded &amp;&amp; HasMoveInput.
    /// Walk vs sprint is differentiated by <see cref="CharacterStateContext.IsSprinting"/>.
    /// </summary>
    internal sealed class LocomotionState : CharacterStateBase
    {
        private readonly Action<CharacterAnimationIntent> _onAnimationIntent;

        public LocomotionState(Action<CharacterAnimationIntent> onAnimationIntent = null)
        {
            _onAnimationIntent = onAnimationIntent;
        }

        public override CharacterStateId StateId => CharacterStateId.Locomotion;

        public override void OnEnter(in CharacterStateContext context)
        {
            PublishAnimationIntent(context);
        }

        public override void OnTick(in CharacterStateContext context, float deltaTime)
        {
            // Update animation intent each frame to reflect actual speed.
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
                stateId: CharacterStateId.Locomotion,
                locomotionSpeed: context.HorizontalSpeed,
                isGrounded: context.IsGrounded,
                isAirborne: false,
                isSprinting: context.IsSprinting,
                isInteracting: false,
                isDead: false);

            _onAnimationIntent(intent);
        }
    }
}
