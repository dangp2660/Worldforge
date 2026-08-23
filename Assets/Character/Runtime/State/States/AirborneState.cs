using System;
using Worldforge.Character.State.Animation;

namespace Worldforge.Character.State.States
{
    /// <summary>
    /// Character is airborne (jumped or fell off a ledge).
    /// Entry condition: IsAlive &amp;&amp; !IsGrounded.
    /// Tracks airborne time for debugging and future gameplay logic (e.g. fall damage).
    /// </summary>
    internal sealed class AirborneState : CharacterStateBase
    {
        private readonly Action<CharacterAnimationIntent> _onAnimationIntent;

        private float _airborneTime;

        public AirborneState(Action<CharacterAnimationIntent> onAnimationIntent = null)
        {
            _onAnimationIntent = onAnimationIntent;
        }

        public override CharacterStateId StateId => CharacterStateId.Airborne;

        /// <summary>Time spent airborne since the last OnEnter (seconds).</summary>
        public float AirborneTime => _airborneTime;

        public override void OnEnter(in CharacterStateContext context)
        {
            _airborneTime = 0f;
            PublishAnimationIntent(context);
        }

        public override void OnTick(in CharacterStateContext context, float deltaTime)
        {
            _airborneTime += deltaTime;
        }

        public override void OnExit(in CharacterStateContext context)
        {
            _airborneTime = 0f;
        }

        private void PublishAnimationIntent(in CharacterStateContext context)
        {
            if (_onAnimationIntent == null)
            {
                return;
            }

            var intent = new CharacterAnimationIntent(
                stateId: CharacterStateId.Airborne,
                locomotionSpeed: context.HorizontalSpeed,
                isGrounded: false,
                isAirborne: true,
                isSprinting: false,
                isInteracting: false,
                isDead: false);

            _onAnimationIntent(intent);
        }
    }
}
