namespace Worldforge.Character.State.Events
{
    /// <summary>
    /// Immutable payload published when the character gameplay state changes.
    /// Readonly struct to avoid heap allocation on publish.
    /// No string identifiers — typed payload per Standard §18.
    /// </summary>
    public readonly struct CharacterStateChangedEvent
    {
        /// <summary>The state that was active before the transition.</summary>
        public CharacterStateId PreviousStateId { get; }

        /// <summary>The state that became active after the transition.</summary>
        public CharacterStateId NextStateId { get; }

        /// <summary>Time of the transition (Time.time).</summary>
        public float Timestamp { get; }

        public CharacterStateChangedEvent(
            CharacterStateId previousStateId,
            CharacterStateId nextStateId,
            float timestamp)
        {
            PreviousStateId = previousStateId;
            NextStateId = nextStateId;
            Timestamp = timestamp;
        }

        public override string ToString()
        {
            return $"{PreviousStateId} → {NextStateId} @ {Timestamp:F2}s";
        }
    }
}
