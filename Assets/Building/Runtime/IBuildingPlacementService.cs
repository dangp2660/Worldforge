using System;
using UnityEngine;
using Worldforge.Core.Attributes;
using Worldforge.Inventory;

namespace Worldforge.Building
{
    // Represents the lifecycle state of a building placement interaction.
    public enum PlacementState
    {
        None = 0,
        Previewing = 1,
        Confirmed = 2,
        Cancelled = 3
    }

    // Service interface managing structure placement preview, validation, and confirmation.
    // Maps to Worldforge Building System architecture and Coding Standard.
    [TestTarget(Category = "Building", DisplayName = "Building Placement Service", Order = 15)]
    public interface IBuildingPlacementService
    {
        PlacementState CurrentState { get; }
        StructureDefinition ActiveDefinition { get; }
        Vector3 CurrentPosition { get; }
        Quaternion CurrentRotation { get; }
        bool IsPlacementValid { get; }
        PlacementValidationResult LastValidationResult { get; }

        event Action<StructureDefinition> PlacementStarted;
        event Action<PlacementResult> PlacementConfirmed;
        event Action<StructureDefinition> PlacementCancelled;
        event Action<PlacementValidationResult> PlacementValidityChanged;

        [TestMethod(DisplayName = "Start Placement", Order = 1, Description = "Begins previewing structure placement")]
        bool StartPlacement(StructureDefinition definition);

        void UpdatePlacement(Vector3 position, Quaternion rotation, IInventoryContainer inventory = null);

        void RotatePreview(float angleDegrees);

        [TestMethod(DisplayName = "Validate Placement", Order = 2, Description = "Validates placement without building")]
        PlacementValidationResult ValidatePlacement(
            StructureDefinition definition,
            Vector3 position,
            Quaternion rotation,
            IInventoryContainer inventory = null);

        [TestMethod(DisplayName = "Confirm Placement", Order = 3, Description = "Confirms placement and consumes resources")]
        PlacementResult ConfirmPlacement(IInventoryContainer inventory = null);

        [TestMethod(DisplayName = "Cancel Placement", Order = 4, Description = "Cancels placement without consuming resources")]
        void CancelPlacement();
    }
}
