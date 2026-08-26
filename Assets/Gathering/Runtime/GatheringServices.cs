using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;
using Worldforge.Gathering;
using Worldforge.Interaction;
using Worldforge.Inventory.Services;
using Worldforge.Item;

namespace Worldforge.Gathering.Services
{
    public interface IGatheringService
    {
        Guid InstanceId { get; }

        int RegisteredNodeCount { get; }

        bool CanGather(string nodeId);

        void RegisterNodeDefinition(ResourceNodeDefinition definition);

        bool TryGetNodeDefinition(string nodeCode, out ResourceNodeDefinition definition);

        ResourceNodeDefinition GetNodeDefinition(string nodeCode);

        IReadOnlyList<ResourceNodeDefinition> GetAllNodeDefinitions();

        GatheringValidationResult ValidateGathering(
            ResourceNodeDefinition node,
            IGatheringTool tool,
            float playerStamina,
            float distanceToNode);

        float CalculateGatherDuration(ResourceNodeDefinition node, IGatheringTool tool);

        int CalculatePrimaryYield(ResourceNodeDefinition node, IGatheringTool tool, System.Random random = null);
    }

    public sealed class RuntimeGatheringService : IGatheringService
    {
        private readonly Dictionary<string, ResourceNodeDefinition> _nodeDefinitions =
            new(StringComparer.OrdinalIgnoreCase);

        public RuntimeGatheringService()
        {
            InstanceId = Guid.NewGuid();
        }

        public Guid InstanceId { get; }

        public int RegisteredNodeCount
        {
            get { return _nodeDefinitions.Count; }
        }

        public bool CanGather(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                return false;
            }

            return _nodeDefinitions.ContainsKey(nodeId);
        }

        public void RegisterNodeDefinition(ResourceNodeDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (string.IsNullOrWhiteSpace(definition.NodeCode))
            {
                throw new ArgumentException("Resource node definition must have a valid NodeCode.", nameof(definition));
            }

            _nodeDefinitions[definition.NodeCode] = definition;
        }

        public bool TryGetNodeDefinition(string nodeCode, out ResourceNodeDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(nodeCode))
            {
                definition = null;
                return false;
            }

            return _nodeDefinitions.TryGetValue(nodeCode, out definition);
        }

        public ResourceNodeDefinition GetNodeDefinition(string nodeCode)
        {
            if (TryGetNodeDefinition(nodeCode, out var definition))
            {
                return definition;
            }

            return null;
        }

        public IReadOnlyList<ResourceNodeDefinition> GetAllNodeDefinitions()
        {
            var list = new List<ResourceNodeDefinition>(_nodeDefinitions.Values);
            return list;
        }

        public GatheringValidationResult ValidateGathering(
            ResourceNodeDefinition node,
            IGatheringTool tool,
            float playerStamina,
            float distanceToNode)
        {
            if (node == null)
            {
                return GatheringValidationResult.Failed(
                    GatheringFailureReason.InvalidNode,
                    "Resource node definition is null.");
            }

            if (node.Requirements == null)
            {
                return GatheringValidationResult.Success();
            }

            return node.Requirements.Validate(tool, playerStamina, distanceToNode);
        }

        public float CalculateGatherDuration(ResourceNodeDefinition node, IGatheringTool tool)
        {
            if (node == null)
            {
                return 0f;
            }

            var baseDuration = Mathf.Max(0.1f, node.BaseGatherDuration);
            var efficiency = tool != null ? Mathf.Max(0.1f, tool.Efficiency) : 1f;

            return baseDuration / efficiency;
        }

        [ThreadStatic]
        private static System.Random s_sharedRandom;

        public int CalculatePrimaryYield(ResourceNodeDefinition node, IGatheringTool tool, System.Random random = null)
        {
            if (node == null)
            {
                return 0;
            }

            var min = Mathf.Max(1, node.PrimaryMinAmount);
            var max = Mathf.Max(min, node.PrimaryMaxAmount);

            var rng = random ?? (s_sharedRandom ??= new System.Random());
            var baseAmount = rng.Next(min, max + 1);

            // High tier or surplus harvest power provides a subtle scaling bonus if applicable
            if (tool != null && tool.HarvestPower > node.Hardness && node.Hardness > 0f)
            {
                var surplusRatio = (tool.HarvestPower - node.Hardness) / node.Hardness;
                if (surplusRatio >= 1f)
                {
                    baseAmount += Mathf.FloorToInt(surplusRatio);
                }
            }

            return baseAmount;
        }
    }

    internal sealed class GatheringServiceRegistrationProvider : IServiceRegistrationProvider
    {
        public int Order
        {
            get { return 110; }
        }

        public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
        {
            services.AddScoped<IGatheringService>(_ => new RuntimeGatheringService());
        }
    }

    internal sealed class GatheringInitializationSystemProvider : IGameSessionSystemProvider
    {
        public int Order
        {
            get { return 110; }
        }

        public IEnumerable<IGameSessionSystem> CreateSystems()
        {
            return new IGameSessionSystem[]
            {
                new GatheringInitializationSystem()
            };
        }
    }

    internal sealed class GatheringInitializationSystem : IGameSessionSystem
    {
        private static readonly IReadOnlyList<string> DependenciesList =
            new[] { "Gameplay.Inventory" };

        public string Name
        {
            get { return "Gameplay.Gathering"; }
        }

        public int Order
        {
            get { return 110; }
        }

        public IReadOnlyList<string> Dependencies
        {
            get { return DependenciesList; }
        }

        public void Initialize(GameSessionContext context)
        {
            context.Services.Resolve<IInventoryService>();
            var gatheringService = context.Services.Resolve<IGatheringService>();
            var logger = context.Services.TryResolve<ILogService>(out var resolvedLogger) ? resolvedLogger : null;

            // Load any pre-configured ResourceNodeDefinitions in Resources
            var preloadedNodes = UnityEngine.Resources.LoadAll<ResourceNodeDefinition>("Definitions/Nodes");
            if (preloadedNodes != null)
            {
                for (var i = 0; i < preloadedNodes.Length; i++)
                {
                    if (preloadedNodes[i] != null && !string.IsNullOrWhiteSpace(preloadedNodes[i].NodeCode))
                    {
                        gatheringService.RegisterNodeDefinition(preloadedNodes[i]);
                    }
                }
            }

            // Register Gathering interaction handler if interaction service is available
            if (context.Services.TryResolve<IInteractionService>(out var interactionService) && interactionService != null)
            {
                var gatheringHandler = new GatheringInteractionHandler(gatheringService, logger);
                interactionService.RegisterHandler(gatheringHandler);
                context.RegisterEventSubscription(
                    "Gameplay.Gathering.InteractionHandler",
                    () => interactionService.UnregisterHandler(gatheringHandler),
                    110);
            }

            context.RecordRuntimeState("gathering.serviceLifetime", ServiceLifetime.Scoped.ToString());
            context.RecordRuntimeState(
                "gathering.registeredNodeCount",
                gatheringService.RegisteredNodeCount.ToString(CultureInfo.InvariantCulture));
            context.RegisterRuntimeResource("Gameplay.Gathering.RuntimeCache", new GatheringRuntimeCache());

            logger?.Info("Gameplay.Gathering", "Gathering gameplay module initialized with " + gatheringService.RegisteredNodeCount + " node definitions.");
        }

        public void Shutdown(GameSessionContext context)
        {
            if (context.Services.TryResolve<IGatheringService>(out var gatheringService) && gatheringService != null)
            {
                context.RecordRuntimeState(
                    "gathering.registeredNodeCount",
                    gatheringService.RegisteredNodeCount.ToString(CultureInfo.InvariantCulture));
            }

            context.RecordRuntimeState("gathering.serviceLifetime", ServiceLifetime.Scoped.ToString());
        }
    }

    internal sealed class GatheringRuntimeCache : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
