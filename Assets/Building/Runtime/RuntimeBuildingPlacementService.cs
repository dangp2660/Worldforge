using System;
using UnityEngine;
using Worldforge.Core.Services;
using Worldforge.Inventory;
using Worldforge.Item;

namespace Worldforge.Building
{
    // Authoritative runtime implementation of IBuildingPlacementService.
    // Handles structure placement lifecycle, spatial validation, resource validation, and finalization.
    public sealed class RuntimeBuildingPlacementService : IBuildingPlacementService
    {
        private readonly BuildingPlacementConfiguration _configuration;
        private readonly ILogService _logger;
        private readonly Collider[] _overlapBuffer = new Collider[16];

        private PlacementState _currentState = PlacementState.None;
        private StructureDefinition _activeDefinition;
        private Vector3 _currentPosition;
        private Quaternion _currentRotation = Quaternion.identity;
        private PlacementValidationResult _lastValidationResult = PlacementValidationResult.Failure(
            PlacementFailureReason.PlacementNotActive,
            "Placement is not active.");

        public event Action<StructureDefinition> PlacementStarted;
        public event Action<PlacementResult> PlacementConfirmed;
        public event Action<StructureDefinition> PlacementCancelled;
        public event Action<PlacementValidationResult> PlacementValidityChanged;

        public PlacementState CurrentState
        {
            get { return _currentState; }
        }

        public StructureDefinition ActiveDefinition
        {
            get { return _activeDefinition; }
        }

        public Vector3 CurrentPosition
        {
            get { return _currentPosition; }
        }

        public Quaternion CurrentRotation
        {
            get { return _currentRotation; }
        }

        public bool IsPlacementValid
        {
            get { return _lastValidationResult.IsValid; }
        }

        public PlacementValidationResult LastValidationResult
        {
            get { return _lastValidationResult; }
        }

        public RuntimeBuildingPlacementService(
            BuildingPlacementConfiguration configuration,
            ILogService logger = null)
        {
            _configuration = configuration ?? ScriptableObject.CreateInstance<BuildingPlacementConfiguration>();
            _logger = logger;
        }

        public bool StartPlacement(StructureDefinition definition)
        {
            if (definition == null)
            {
                _logger?.Warning("Building.Placement", "Cannot start placement: StructureDefinition is null.");
                return false;
            }

            if (!definition.IsValid(out var reason))
            {
                _logger?.Warning("Building.Placement", $"Cannot start placement: Invalid definition '{definition.name}': {reason}");
                return false;
            }

            // Cancel any ongoing placement before starting new one
            if (_currentState == PlacementState.Previewing)
            {
                CancelPlacement();
            }

            _activeDefinition = definition;
            _currentPosition = Vector3.zero;
            _currentRotation = Quaternion.identity;
            _currentState = PlacementState.Previewing;
            _lastValidationResult = PlacementValidationResult.Failure(
                PlacementFailureReason.NoPlacementSurface,
                "Aim at a valid ground surface to place.");

            _logger?.Info("Building.Placement", $"Started placement preview for '{definition.DisplayName}'.");
            PlacementStarted?.Invoke(definition);
            return true;
        }

        public void UpdatePlacement(Vector3 position, Quaternion rotation, IInventoryContainer inventory = null)
        {
            if (_currentState != PlacementState.Previewing || _activeDefinition == null)
            {
                return;
            }

            _currentPosition = position;
            _currentRotation = rotation;

            var newValidation = ValidatePlacement(_activeDefinition, position, rotation, inventory);
            var validityChanged = _lastValidationResult.IsValid != newValidation.IsValid
                || _lastValidationResult.FailureReason != newValidation.FailureReason;

            _lastValidationResult = newValidation;

            if (validityChanged)
            {
                PlacementValidityChanged?.Invoke(newValidation);
            }
        }

        public void RotatePreview(float angleDegrees)
        {
            if (_currentState != PlacementState.Previewing || _activeDefinition == null)
            {
                return;
            }

            if (!_activeDefinition.PlacementRule.CanRotate)
            {
                return;
            }

            _currentRotation *= Quaternion.Euler(0f, angleDegrees, 0f);
        }

        public PlacementValidationResult ValidatePlacement(
            StructureDefinition definition,
            Vector3 position,
            Quaternion rotation,
            IInventoryContainer inventory = null)
        {
            if (definition == null)
            {
                return PlacementValidationResult.Failure(
                    PlacementFailureReason.NoStructureSelected,
                    "No structure selected for placement.");
            }

            if (!definition.IsValid(out var invalidReason))
            {
                return PlacementValidationResult.Failure(
                    PlacementFailureReason.InvalidDefinition,
                    $"Structure definition is invalid: {invalidReason}");
            }

            var rule = definition.PlacementRule;

            // 1. Ground Surface Detection Check
            if (rule.RequiresGround)
            {
                var rayOrigin = position + Vector3.up * 1f;
                var rayDistance = _configuration.GroundCheckRayDistance + 1f;
                var groundMask = _configuration.GroundLayerMask.value == 0 ? ~0 : _configuration.GroundLayerMask.value;

                var hasGround = Physics.Raycast(
                    rayOrigin,
                    Vector3.down,
                    out var hit,
                    rayDistance,
                    groundMask,
                    QueryTriggerInteraction.Ignore);

                if (!hasGround)
                {
                    return PlacementValidationResult.Failure(
                        PlacementFailureReason.RequiresGround,
                        "Structure must be placed on a valid ground surface.");
                }
            }

            // 2. Foundation Requirement Check
            if (rule.RequiresFoundation)
            {
                var foundationMask = _configuration.FoundationLayerMask.value;
                if (foundationMask != 0)
                {
                    var hasFoundation = Physics.Raycast(
                        position + Vector3.up * 0.5f,
                        Vector3.down,
                        out _,
                        2f,
                        foundationMask,
                        QueryTriggerInteraction.Ignore);

                    if (!hasFoundation)
                    {
                        return PlacementValidationResult.Failure(
                            PlacementFailureReason.RequiresFoundation,
                            "Structure requires an underlying foundation.");
                    }
                }
            }

            // 3. Obstruction Collision Check
            var obstructionMask = _configuration.ObstructionLayerMask.value;
            if (obstructionMask != 0)
            {
                var footprint = rule.Footprint;
                var halfExtents = new Vector3(
                    Mathf.Max(0.5f, footprint.x * 0.45f),
                    0.5f,
                    Mathf.Max(0.5f, footprint.y * 0.45f));

                var checkCenter = position + Vector3.up * 0.6f;
                var hitCount = Physics.OverlapBoxNonAlloc(
                    checkCenter,
                    halfExtents,
                    _overlapBuffer,
                    rotation,
                    obstructionMask,
                    QueryTriggerInteraction.Ignore);

                // Clean buffer references
                for (var i = 0; i < hitCount; i++)
                {
                    _overlapBuffer[i] = null;
                }

                if (hitCount > 0)
                {
                    return PlacementValidationResult.Failure(
                        PlacementFailureReason.Obstructed,
                        "Placement position is obstructed by another object.");
                }
            }

            // 4. Resource Requirements Check
            if (inventory != null && definition.HasRequirements)
            {
                for (var i = 0; i < definition.Requirements.Count; i++)
                {
                    var req = definition.Requirements[i];
                    if (req == null || req.Item == null)
                    {
                        continue;
                    }

                    var available = inventory.GetItemCount(req.Item);
                    if (available < req.Amount)
                    {
                        return PlacementValidationResult.Failure(
                            PlacementFailureReason.InsufficientResources,
                            $"Insufficient resource: {req.Item.DisplayName} ({available}/{req.Amount}).");
                    }
                }
            }

            return PlacementValidationResult.Success();
        }

        public PlacementResult ConfirmPlacement(IInventoryContainer inventory = null)
        {
            if (_currentState != PlacementState.Previewing || _activeDefinition == null)
            {
                return PlacementResult.Failure(
                    PlacementFailureReason.PlacementNotActive,
                    "No placement session is currently active.");
            }

            // Final validation pass before committing transaction
            var validation = ValidatePlacement(_activeDefinition, _currentPosition, _currentRotation, inventory);
            if (!validation.IsValid)
            {
                _logger?.Warning(
                    "Building.Placement",
                    $"Cannot confirm placement for '{_activeDefinition.DisplayName}': {validation.Message}");
                return PlacementResult.Failure(validation.FailureReason, validation.Message, _activeDefinition);
            }

            // Consume required resources
            if (inventory != null && _activeDefinition.HasRequirements)
            {
                for (var i = 0; i < _activeDefinition.Requirements.Count; i++)
                {
                    var req = _activeDefinition.Requirements[i];
                    if (req != null && req.Item != null && req.Amount > 0)
                    {
                        inventory.RemoveItem(req.Item, req.Amount);
                    }
                }
            }

            // Instantiate placed structure GameObject
            GameObject placedObject;
            if (_activeDefinition.Prefab != null)
            {
                placedObject = UnityEngine.Object.Instantiate(
                    _activeDefinition.Prefab,
                    _currentPosition,
                    _currentRotation);
            }
            else
            {
                // Fallback placeholder primitive cube
                placedObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                placedObject.name = $"Placed_{_activeDefinition.DisplayName}";
                placedObject.transform.position = _currentPosition + Vector3.up * 0.5f;
                placedObject.transform.rotation = _currentRotation;
                var footprint = _activeDefinition.PlacementRule.Footprint;
                placedObject.transform.localScale = new Vector3(footprint.x, 1f, footprint.y);
            }

            var result = PlacementResult.Success(
                _activeDefinition,
                placedObject,
                _currentPosition,
                _currentRotation);

            var completedDef = _activeDefinition;
            _currentState = PlacementState.Confirmed;
            _activeDefinition = null;

            _logger?.Info(
                "Building.Placement",
                $"Successfully placed '{completedDef.DisplayName}' at {_currentPosition}.");

            PlacementConfirmed?.Invoke(result);
            _currentState = PlacementState.None;

            return result;
        }

        public void CancelPlacement()
        {
            if (_currentState != PlacementState.Previewing || _activeDefinition == null)
            {
                return;
            }

            var cancelledDef = _activeDefinition;
            _currentState = PlacementState.Cancelled;
            _activeDefinition = null;

            _logger?.Info(
                "Building.Placement",
                $"Placement for '{cancelledDef.DisplayName}' was cancelled. No resources consumed.");

            PlacementCancelled?.Invoke(cancelledDef);
            _currentState = PlacementState.None;
        }
    }
}
