using UnityEngine;
using Worldforge.Core.Services;
using Worldforge.Gathering.Services;
using Worldforge.Interaction;
using Worldforge.Item;

namespace Worldforge.Gathering
{
    /// <summary>
    /// Extensibility handler for gathering interactions.
    /// Registered with <see cref="IInteractionService"/> to handle <see cref="InteractionType.Gather"/>.
    /// </summary>
    public sealed class GatheringInteractionHandler : IInteractionHandler
    {
        private readonly IGatheringService _gatheringService;
        private readonly ILogService _logger;

        public GatheringInteractionHandler(IGatheringService gatheringService, ILogService logger)
        {
            _gatheringService = gatheringService;
            _logger = logger;
        }

        public bool CanHandle(InteractionType type)
        {
            return type == InteractionType.Gather;
        }

        public InteractionResult Validate(InteractionContext context)
        {
            if (context.Target is not ResourceNodeBehaviour node)
            {
                // Fallback for non-ResourceNodeBehaviour interactables with Gather type
                return InteractionResult.Success();
            }

            if (node.Definition == null)
            {
                return InteractionResult.Fail("Resource node has no definition assigned.");
            }

            if (node.IsDepleted || node.State == ResourceNodeState.Disabled || node.CurrentHealth <= 0f)
            {
                return InteractionResult.Fail("Resource node is depleted.");
            }

            // Ensure node is bound to service for active node tracking and events
            if (_gatheringService != null)
            {
                node.BindGatheringService(_gatheringService);
            }

            var tool = ResolveGatheringTool(context.Interactor);
            var stamina = ResolveStamina(context.Interactor);
            var distance = CalculateDistance(context.Interactor, node);

            var validation = _gatheringService != null
                ? _gatheringService.ValidateGathering(node, tool, stamina, distance)
                : node.ValidateGathering(tool, stamina, distance);

            if (!validation.IsSuccess)
            {
                _logger?.Info(
                    "Gameplay.Gathering",
                    $"Gathering validation failed on '{node.Definition.DisplayName}': {validation.Message} (Reason: {validation.FailureReason})");

                return InteractionResult.Fail(validation.Message);
            }

            // Calculate and set dynamic gather duration based on tool efficiency
            if (_gatheringService != null)
            {
                var dynamicDuration = _gatheringService.CalculateGatherDuration(node.Definition, tool);
                node.SetActiveGatherDuration(dynamicDuration);
            }

            return InteractionResult.Success();
        }

        public InteractionResult Execute(InteractionContext context)
        {
            if (context.Target is not ResourceNodeBehaviour node)
            {
                return InteractionResult.Success();
            }

            var tool = ResolveGatheringTool(context.Interactor);

            // Deduct stamina if requirements exist and interactor supports stamina
            if (node.Definition?.Requirements != null && node.Definition.Requirements.StaminaCostPerAction > 0f)
            {
                ConsumeStamina(context.Interactor, node.Definition.Requirements.StaminaCostPerAction);
            }

            var result = _gatheringService != null
                ? _gatheringService.ProcessGatheringAction(node, tool, context.Interactor)
                : node.Harvest(tool, context.Interactor);

            if (!result.IsSuccess)
            {
                _logger?.Warning(
                    "Gameplay.Gathering",
                    $"Gathering action failed on '{node.Definition?.DisplayName ?? "Node"}': {result.FailureMessage}");

                return InteractionResult.Fail(result.FailureMessage);
            }

            // Deliver gathered items to interactor if an item receiver is present
            DeliverGatheredItems(context.Interactor, result);

            _logger?.Info(
                "Gameplay.Gathering",
                $"Gathered {result.PrimaryYieldAmount}x {result.PrimaryYieldItem?.DisplayName ?? "Resource"} " +
                $"from '{node.Definition?.DisplayName ?? "Node"}'. XP: +{result.DiscoveryXP}. " +
                $"Depleted: {result.WasDepleted}. Damage: {result.DamageDealt:F1}. Remaining Health: {result.RemainingHealth:F1}.");

            return InteractionResult.Success();
        }

        public void Cancel(InteractionContext context)
        {
            if (context.Target is ResourceNodeBehaviour node)
            {
                node.CancelGathering();
                _logger?.Info(
                    "Gameplay.Gathering",
                    $"Gathering cancelled on '{node.Definition?.DisplayName ?? "Node"}'.");
            }
        }

        private static IGatheringTool ResolveGatheringTool(GameObject interactor)
        {
            if (interactor == null)
            {
                return null;
            }

            // Check for IGatheringToolProvider first
            var provider = interactor.GetComponent<IGatheringToolProvider>()
                ?? interactor.GetComponentInChildren<IGatheringToolProvider>();

            if (provider != null && provider.ActiveTool != null)
            {
                return provider.ActiveTool;
            }

            // Check for direct IGatheringTool component
            return interactor.GetComponent<IGatheringTool>()
                ?? interactor.GetComponentInChildren<IGatheringTool>();
        }

        private static float ResolveStamina(GameObject interactor)
        {
            // Default full stamina baseline (100f) until dedicated stamina provider is bound
            return 100f;
        }

        private static void ConsumeStamina(GameObject interactor, float amount)
        {
            // Extension point for stamina consumption once stamina service is active
        }

        private static void DeliverGatheredItems(GameObject interactor, GatheringHarvestResult result)
        {
            if (interactor == null || !result.IsSuccess)
            {
                return;
            }

            var receiver = interactor.GetComponent<IGatheredItemReceiver>()
                ?? interactor.GetComponentInChildren<IGatheredItemReceiver>();

            if (receiver == null)
            {
                return;
            }

            if (result.PrimaryYieldItem != null && result.PrimaryYieldAmount > 0)
            {
                receiver.ReceiveItem(result.PrimaryYieldItem, result.PrimaryYieldAmount);
            }

            if (result.BonusYields != null && result.BonusYields.Count > 0)
            {
                for (var i = 0; i < result.BonusYields.Count; i++)
                {
                    var bonus = result.BonusYields[i];
                    if (bonus.Item != null && bonus.Amount > 0)
                    {
                        receiver.ReceiveItem(bonus.Item, bonus.Amount);
                    }
                }
            }
        }

        private static float CalculateDistance(GameObject interactor, ResourceNodeBehaviour node)
        {
            if (interactor == null || node == null)
            {
                return 0f;
            }

            var targetPoint = node.InteractionPoint != null
                ? node.InteractionPoint.position
                : node.transform.position;

            return Vector3.Distance(interactor.transform.position, targetPoint);
        }
    }
}
