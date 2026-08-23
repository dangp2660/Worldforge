namespace Worldforge.Character.State
{
    /// <summary>
    /// Defines all valid gameplay state IDs for a character.
    /// Enum-based to avoid string allocation in the gameplay loop.
    /// </summary>
    public enum CharacterStateId
    {
        /// <summary>Uninitialized or indeterminate state.</summary>
        None = 0,

        /// <summary>Character is standing still with no move input.</summary>
        Idle = 1,

        /// <summary>Character is moving on the ground (walk or sprint).</summary>
        Locomotion = 2,

        /// <summary>Character is in the air (jump or fall).</summary>
        Airborne = 3,

        /// <summary>Character is interacting with a world object (placeholder for v0.2).</summary>
        Interacting = 4,

        /// <summary>Character is dead. Only exits via ForceTransition (respawn).</summary>
        Dead = 5
    }
}
