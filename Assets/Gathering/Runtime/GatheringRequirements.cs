using System;
using UnityEngine;
using Worldforge.Item;

namespace Worldforge.Gathering
{
    [Serializable]
    public sealed class GatheringRequirements
    {
        [SerializeField] private ToolType _requiredToolType = ToolType.None;
        [SerializeField] private float _minimumHarvestPower = 0f;
        [SerializeField] private int _requiredToolTier = 0;
        [SerializeField] private float _staminaCostPerAction = 5f;
        [SerializeField] private float _maxInteractionDistance = 3f;

        public GatheringRequirements()
        {
        }

        public GatheringRequirements(
            ToolType requiredToolType,
            float minimumHarvestPower,
            int requiredToolTier = 0,
            float staminaCostPerAction = 5f,
            float maxInteractionDistance = 3f)
        {
            _requiredToolType = requiredToolType;
            _minimumHarvestPower = minimumHarvestPower;
            _requiredToolTier = requiredToolTier;
            _staminaCostPerAction = staminaCostPerAction;
            _maxInteractionDistance = maxInteractionDistance;
        }

        public ToolType RequiredToolType
        {
            get { return _requiredToolType; }
        }

        public float MinimumHarvestPower
        {
            get { return _minimumHarvestPower; }
        }

        public int RequiredToolTier
        {
            get { return _requiredToolTier; }
        }

        public float StaminaCostPerAction
        {
            get { return _staminaCostPerAction; }
        }

        public float MaxInteractionDistance
        {
            get { return _maxInteractionDistance; }
        }

        public bool RequiresTool
        {
            get { return _requiredToolType != ToolType.None; }
        }

        public GatheringValidationResult Validate(IGatheringTool tool, float playerStamina, float distanceToNode)
        {
            if (distanceToNode > _maxInteractionDistance)
            {
                return GatheringValidationResult.Failed(
                    GatheringFailureReason.OutOfRange,
                    $"Target is out of interaction range ({distanceToNode:F1}m > {_maxInteractionDistance:F1}m).");
            }

            if (playerStamina < _staminaCostPerAction)
            {
                return GatheringValidationResult.Failed(
                    GatheringFailureReason.InsufficientStamina,
                    $"Insufficient stamina to gather ({playerStamina:F1} < {_staminaCostPerAction:F1}).");
            }

            if (RequiresTool)
            {
                if (tool == null || tool.ToolType == ToolType.None)
                {
                    return GatheringValidationResult.Failed(
                        GatheringFailureReason.MissingTool,
                        $"A {_requiredToolType} tool is required to gather this resource.");
                }

                if (tool.ToolType != _requiredToolType)
                {
                    return GatheringValidationResult.Failed(
                        GatheringFailureReason.MissingTool,
                        $"Incorrect tool type. Required: {_requiredToolType}, Used: {tool.ToolType}.");
                }

                if (tool.ToolTier < _requiredToolTier)
                {
                    return GatheringValidationResult.Failed(
                        GatheringFailureReason.InsufficientToolTier,
                        $"Tool tier {tool.ToolTier} is lower than required tier {_requiredToolTier}.");
                }

                if (tool.HarvestPower < _minimumHarvestPower)
                {
                    return GatheringValidationResult.Failed(
                        GatheringFailureReason.InsufficientHarvestPower,
                        $"Tool harvest power ({tool.HarvestPower:F1}) is lower than minimum required ({_minimumHarvestPower:F1}).");
                }
            }

            return GatheringValidationResult.Success();
        }

        public void ValidateData()
        {
            _minimumHarvestPower = Mathf.Max(0f, _minimumHarvestPower);
            _requiredToolTier = Mathf.Max(0, _requiredToolTier);
            _staminaCostPerAction = Mathf.Max(0f, _staminaCostPerAction);
            _maxInteractionDistance = Mathf.Max(0.5f, _maxInteractionDistance);
        }
    }
}
