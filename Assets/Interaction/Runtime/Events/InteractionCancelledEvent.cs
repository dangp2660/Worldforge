namespace Worldforge.Interaction.Events
{
    // Fired when an interaction is cancelled before completion.
    public readonly struct InteractionCancelledEvent
    {
        public InteractionContext Context { get; }

        public string Reason { get; }

        public InteractionCancelledEvent(InteractionContext context, string reason)
        {
            Context = context;
            Reason = reason;
        }

        public override string ToString()
        {
            return $"InteractionCancelled: {Context} Reason: {Reason}";
        }
    }
}
