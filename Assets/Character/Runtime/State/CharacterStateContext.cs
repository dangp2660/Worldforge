using UnityEngine;

namespace Worldforge.Character.State
{
    /// <summary>
    /// Immutable runtime snapshot built each tick and shared by states and transition conditions.
    /// Readonly struct to avoid heap allocation.
    /// Constructed by <see cref="RuntimeCharacterStateService"/> from movement and health signals.
    /// </summary>
    public readonly struct CharacterStateContext
    {
        /// <summary>Character is standing on the ground.</summary>
        public bool IsGrounded { get; }

        /// <summary>Current world-space velocity of the character.</summary>
        public Vector3 CurrentVelocity { get; }

        /// <summary>Player is pressing a move button (input magnitude > deadzone).</summary>
        public bool HasMoveInput { get; }

        /// <summary>Player is pressing the sprint button.</summary>
        public bool IsSprinting { get; }

        /// <summary>Character is alive (HP > 0).</summary>
        public bool IsAlive { get; }

        /// <summary>Delta time for the current frame.</summary>
        public float DeltaTime { get; }

        public CharacterStateContext(
            bool isGrounded,
            Vector3 currentVelocity,
            bool hasMoveInput,
            bool isSprinting,
            bool isAlive,
            float deltaTime)
        {
            IsGrounded = isGrounded;
            CurrentVelocity = currentVelocity;
            HasMoveInput = hasMoveInput;
            IsSprinting = isSprinting;
            IsAlive = isAlive;
            DeltaTime = deltaTime;
        }

        /// <summary>Horizontal speed (Y axis ignored).</summary>
        public float HorizontalSpeed
        {
            get
            {
                var vx = CurrentVelocity.x;
                var vz = CurrentVelocity.z;
                return Mathf.Sqrt(vx * vx + vz * vz);
            }
        }

        /// <summary>Default context used before the first real frame data is available (e.g. on spawn).</summary>
        public static CharacterStateContext Default => new CharacterStateContext(
            isGrounded: true,
            currentVelocity: Vector3.zero,
            hasMoveInput: false,
            isSprinting: false,
            isAlive: true,
            deltaTime: 0f);
    }
}
