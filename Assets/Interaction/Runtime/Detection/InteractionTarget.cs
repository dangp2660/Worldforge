namespace Worldforge.Interaction.Detection
{
    // Immutable result from the detection system.
    // Contains the closest valid interactable and its distance.
    public readonly struct InteractionTarget
    {
        public IInteractable Interactable { get; }

        public float Distance { get; }

        public bool HasTarget
        {
            get { return Interactable != null; }
        }

        public InteractionTarget(IInteractable interactable, float distance)
        {
            Interactable = interactable;
            Distance = distance;
        }

        public static InteractionTarget None
        {
            get { return new InteractionTarget(null, float.MaxValue); }
        }

        public override string ToString()
        {
            return HasTarget
                ? $"{Interactable.InteractionPrompt} ({Distance:F2}m)"
                : "No target";
        }
    }
}
