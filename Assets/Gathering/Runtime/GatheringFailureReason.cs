namespace Worldforge.Gathering
{
    public enum GatheringFailureReason
    {
        None = 0,
        InvalidNode = 1,
        NodeDepleted = 2,
        MissingTool = 3,
        InsufficientHarvestPower = 4,
        InsufficientToolTier = 5,
        InsufficientStamina = 6,
        OutOfRange = 7
    }
}
