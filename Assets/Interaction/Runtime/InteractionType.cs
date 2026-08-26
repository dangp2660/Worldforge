namespace Worldforge.Interaction
{
    // Interaction types aligned with domain objects in WorldforgeSchema.
    // Each type corresponds to a distinct gameplay interaction category.
    public enum InteractionType
    {
        None = 0,
        Gather = 1,
        Talk = 2,
        Trade = 3,
        Craft = 4,
        Open = 5,
        Use = 6,
        Examine = 7,
        Activate = 8,
        Pickup = 9
    }
}
