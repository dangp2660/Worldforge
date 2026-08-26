using System;

namespace Worldforge.Gathering
{
    [Serializable]
    public readonly struct GatheringValidationResult : IEquatable<GatheringValidationResult>
    {
        public static readonly GatheringValidationResult Successful = new(true, GatheringFailureReason.None, string.Empty);

        public bool IsSuccess { get; }
        public GatheringFailureReason FailureReason { get; }
        public string Message { get; }

        public GatheringValidationResult(bool isSuccess, GatheringFailureReason failureReason, string message)
        {
            IsSuccess = isSuccess;
            FailureReason = failureReason;
            Message = message ?? string.Empty;
        }

        public static GatheringValidationResult Success()
        {
            return Successful;
        }

        public static GatheringValidationResult Failed(GatheringFailureReason reason, string message)
        {
            return new GatheringValidationResult(false, reason, message);
        }

        public bool Equals(GatheringValidationResult other)
        {
            return IsSuccess == other.IsSuccess &&
                   FailureReason == other.FailureReason &&
                   string.Equals(Message, other.Message, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is GatheringValidationResult other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsSuccess, (int)FailureReason, Message);
        }

        public static bool operator ==(GatheringValidationResult left, GatheringValidationResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GatheringValidationResult left, GatheringValidationResult right)
        {
            return !left.Equals(right);
        }
    }
}
