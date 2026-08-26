namespace Worldforge.Interaction
{
    // Immutable result of an interaction request.
    // Readonly struct to avoid heap allocation.
    public readonly struct InteractionResult
    {
        public bool IsSuccess { get; }

        public string FailureReason { get; }

        private InteractionResult(bool isSuccess, string failureReason)
        {
            IsSuccess = isSuccess;
            FailureReason = failureReason;
        }

        public static InteractionResult Success()
        {
            return new InteractionResult(true, null);
        }

        public static InteractionResult Fail(string reason)
        {
            return new InteractionResult(false, reason ?? "Unknown failure");
        }

        public override string ToString()
        {
            return IsSuccess ? "Success" : $"Failed: {FailureReason}";
        }
    }
}
