using System;
using Worldforge.Character.State.Animation;

namespace Worldforge.Character.State.States
{
    /// <summary>
    /// Character is dead (HP reached zero or forced by the Health System).
    /// No automatic transitions out of this state — only exits via ForceTransition (respawn).
    /// </summary>
    internal sealed class DeadState : CharacterStateBase
    {
        private readonly Action<CharacterAnimationIntent> _onAnimationIntent;

        public DeadState(Action<CharacterAnimationIntent> onAnimationIntent = null)
        {
            _onAnimationIntent = onAnimationIntent;
        }

        public override CharacterStateId StateId => CharacterStateId.Dead;

        public override void OnEnter(in CharacterStateContext context)
        {
            PublishAnimationIntent();
        }

        // OnTick is intentionally omitted — no per-frame logic needed while dead.
        // Override here if a ragdoll timer or similar mechanic is required in future.

        public override void OnExit(in CharacterStateContext context)
        {
            // Called when respawning (ForceTransition to Idle).
        }

        private void PublishAnimationIntent()
        {
            if (_onAnimationIntent == null)
            {
                return;
            }

            var intent = new CharacterAnimationIntent(
                stateId: CharacterStateId.Dead,
                locomotionSpeed: 0f,
                isGrounded: false,
                isAirborne: false,
                isSprinting: false,
                isInteracting: false,
                isDead: true);

            _onAnimationIntent(intent);
        }
    }
}
