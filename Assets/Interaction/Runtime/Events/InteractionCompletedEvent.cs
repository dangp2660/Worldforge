namespace Worldforge.Interaction.Events
{
    // Fired when an interaction completes successfully.
    public readonly struct InteractionCompletedEvent
    {
        public InteractionContext Context { get; }

        public InteractionCompletedEvent(InteractionContext context)
        {
            Context = context;
        }

        public override string ToString()
        {
            return $"InteractionCompleted: {Context}";
        }
    }
}
