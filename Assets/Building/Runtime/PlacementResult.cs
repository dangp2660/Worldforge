using UnityEngine;

namespace Worldforge.Building
{
    // Result object returned when completing or attempting a building placement.
    public sealed class PlacementResult
    {
        public bool IsSuccess { get; }
        public PlacementFailureReason FailureReason { get; }
        public string Message { get; }
        public StructureDefinition Structure { get; }
        public GameObject PlacedObject { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        private PlacementResult(
            bool isSuccess,
            PlacementFailureReason failureReason,
            string message,
            StructureDefinition structure,
            GameObject placedObject,
            Vector3 position,
            Quaternion rotation)
        {
            IsSuccess = isSuccess;
            FailureReason = failureReason;
            Message = message ?? string.Empty;
            Structure = structure;
            PlacedObject = placedObject;
            Position = position;
            Rotation = rotation;
        }

        public static PlacementResult Success(
            StructureDefinition structure,
            GameObject placedObject,
            Vector3 position,
            Quaternion rotation)
        {
            return new PlacementResult(
                true,
                PlacementFailureReason.None,
                "Structure placed successfully.",
                structure,
                placedObject,
                position,
                rotation);
        }

        public static PlacementResult Failure(
            PlacementFailureReason reason,
            string message,
            StructureDefinition structure = null)
        {
            return new PlacementResult(
                false,
                reason,
                message,
                structure,
                null,
                Vector3.zero,
                Quaternion.identity);
        }

        public override string ToString()
        {
            return IsSuccess
                ? $"[PlacementResult: Success] Structure='{Structure?.DisplayName ?? Structure?.StructureCode ?? "Unknown"}' at {Position}"
                : $"[PlacementResult: Failure] Reason={FailureReason}, Message='{Message}'";
        }
    }
}
