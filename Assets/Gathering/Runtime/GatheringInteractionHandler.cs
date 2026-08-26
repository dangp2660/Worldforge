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

            if (node.IsDepleted || node.State == ResourceNodeState.Disabled)
            {
                return InteractionResult.Fail("Resource node is depleted.");
            }

            var tool = ResolveGatheringTool(context.Interactor);
            var stamina = ResolveStamina(context.Interactor);
            var distance = CalculateDistance(context.Interactor, node);

            var validation = node.ValidateGathering(tool, stamina, distance);
            if (!validation.IsSuccess)
            {
                return InteractionResult.Fail(validation.Message);
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
            var result = node.Harvest(tool, context.Interactor);

            if (!result.IsSuccess)
            {
                return InteractionResult.Fail(result.FailureMessage);
            }

            _logger?.Info(
                "Gameplay.Gathering",
                $"Gathered {result.PrimaryYieldAmount}x {result.PrimaryYieldItem?.DisplayName ?? "Resource"} " +
                $"from '{node.Definition.DisplayName}'. Depleted: {result.WasDepleted}. Damage: {result.DamageDealt:F1}.");

            return InteractionResult.Success();
        }

        public void Cancel(InteractionContext context)
        {
            if (context.Target is ResourceNodeBehaviour node)
            {
                node.CancelGathering();
            }
        }

        private static IGatheringTool ResolveGatheringTool(GameObject interactor)
        {
            if (interactor == null)
            {
                return null;
            }

            return interactor.GetComponent<IGatheringTool>()
                ?? interactor.GetComponentInChildren<IGatheringTool>();
        }

        private static float ResolveStamina(GameObject interactor)
        {
            // Default full stamina baseline (100f) until dedicated stamina provider is bound
            return 100f;
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
