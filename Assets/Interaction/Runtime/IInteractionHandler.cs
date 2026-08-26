namespace Worldforge.Interaction
{
    /// <summary>
    /// Extensibility point for domain-specific interaction logic.
    /// Each gameplay module (Gathering, NPC, Crafting...) registers its own handler.
    /// The interaction service delegates to the matching handler by InteractionType.
    /// </summary>
    public interface IInteractionHandler
    {
        bool CanHandle(InteractionType type);

        InteractionResult Validate(InteractionContext context);

        InteractionResult Execute(InteractionContext context);

        void Cancel(InteractionContext context);
    }
}
