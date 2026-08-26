namespace Worldforge.Interaction.Events
{
    // Fired when an interaction begins.
    public readonly struct InteractionStartedEvent
    {
        public InteractionContext Context { get; }

        public InteractionStartedEvent(InteractionContext context)
        {
            Context = context;
        }

        public override string ToString()
        {
            return $"InteractionStarted: {Context}";
        }
    }
}
