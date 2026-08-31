namespace Worldforge.Building
{
    // Possible failure reasons when attempting to place a structure.
    public enum PlacementFailureReason
    {
        None = 0,
        NoStructureSelected = 1,
        InvalidDefinition = 2,
        RequiresGround = 3,
        RequiresFoundation = 4,
        Obstructed = 5,
        InsufficientResources = 6,
        NoPlacementSurface = 7,
        PlacementNotActive = 8
    }

    // Immutable validation result struct for building placement checks.
    public readonly struct PlacementValidationResult
    {
        public bool IsValid { get; }
        public PlacementFailureReason FailureReason { get; }
        public string Message { get; }

        public PlacementValidationResult(bool isValid, PlacementFailureReason failureReason, string message)
        {
            IsValid = isValid;
            FailureReason = failureReason;
            Message = message ?? string.Empty;
        }

        public static PlacementValidationResult Success()
        {
            return new PlacementValidationResult(true, PlacementFailureReason.None, "Placement position is valid.");
        }

        public static PlacementValidationResult Failure(PlacementFailureReason reason, string message)
        {
            return new PlacementValidationResult(false, reason, message);
        }

        public override string ToString()
        {
            return IsValid
                ? "[PlacementValidationResult: Valid]"
                : $"[PlacementValidationResult: Invalid] Reason: {FailureReason}, Message: {Message}";
        }
    }
}
