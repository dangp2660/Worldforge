namespace Worldforge.Character.State.Animation
{
    /// <summary>
    /// Immutable intent value published each frame to describe the character's desired animation state.
    /// Readonly struct — zero heap allocation per frame.
    /// Produced by the gameplay state machine and consumed by <see cref="ICharacterAnimationDriver"/>.
    /// NOT an authoritative gameplay state — presentation only.
    /// </summary>
    public readonly struct CharacterAnimationIntent
    {
        /// <summary>Current gameplay state (for Animator reference only).</summary>
        public CharacterStateId StateId { get; }

        /// <summary>Horizontal movement speed (0 = idle, higher = faster).</summary>
        public float LocomotionSpeed { get; }

        /// <summary>Character is on the ground.</summary>
        public bool IsGrounded { get; }

        /// <summary>Character is in the air (jump or fall).</summary>
        public bool IsAirborne { get; }

        /// <summary>Character is sprinting.</summary>
        public bool IsSprinting { get; }

        /// <summary>Character is interacting with an object.</summary>
        public bool IsInteracting { get; }

        /// <summary>Character is dead.</summary>
        public bool IsDead { get; }

        public CharacterAnimationIntent(
            CharacterStateId stateId,
            float locomotionSpeed,
            bool isGrounded,
            bool isAirborne,
            bool isSprinting,
            bool isInteracting,
            bool isDead)
        {
            StateId = stateId;
            LocomotionSpeed = locomotionSpeed;
            IsGrounded = isGrounded;
            IsAirborne = isAirborne;
            IsSprinting = isSprinting;
            IsInteracting = isInteracting;
            IsDead = isDead;
        }

        /// <summary>Default intent — idle, grounded, alive.</summary>
        public static CharacterAnimationIntent Default => new CharacterAnimationIntent(
            stateId: CharacterStateId.Idle,
            locomotionSpeed: 0f,
            isGrounded: true,
            isAirborne: false,
            isSprinting: false,
            isInteracting: false,
            isDead: false);
    }
}
