using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Worldforge.Character.Spawning;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;
using Worldforge.Gathering;
using Worldforge.Interaction;
using Worldforge.Inventory;
using Worldforge.Inventory.Services;
using Worldforge.Item;

namespace Worldforge.Gathering.Services
{
    public interface IGatheringService
    {
        Guid InstanceId { get; }

        int RegisteredNodeCount { get; }

        int ActiveNodeCount { get; }

        int TotalGatherCount { get; }

        int TotalDepletedCount { get; }

        int TotalRespawnedCount { get; }

        bool CanGather(string nodeId);

        void RegisterNodeDefinition(ResourceNodeDefinition definition);

        bool TryGetNodeDefinition(string nodeCode, out ResourceNodeDefinition definition);

        ResourceNodeDefinition GetNodeDefinition(string nodeCode);

        IReadOnlyList<ResourceNodeDefinition> GetAllNodeDefinitions();

        void RegisterActiveNode(ResourceNodeBehaviour node);

        void UnregisterActiveNode(ResourceNodeBehaviour node);

        IReadOnlyList<ResourceNodeBehaviour> GetAllActiveNodes();

        GatheringValidationResult ValidateGathering(
            ResourceNodeDefinition node,
            IGatheringTool tool,
            float playerStamina,
            float distanceToNode);

        GatheringValidationResult ValidateGathering(
            ResourceNodeBehaviour node,
            IGatheringTool tool,
            float playerStamina,
            float distanceToNode);

        float CalculateGatherDuration(ResourceNodeDefinition node, IGatheringTool tool);

        int CalculatePrimaryYield(ResourceNodeDefinition node, IGatheringTool tool, System.Random random = null);

        GatheringHarvestResult ProcessGatheringAction(
            ResourceNodeBehaviour node,
            IGatheringTool tool,
            GameObject interactor);

        event Action<ResourceNodeGatheredEvent> NodeGathered;

        event Action<ResourceNodeStateChangedEvent> NodeStateChanged;

        event Action<ResourceNodeDepletedEvent> NodeDepleted;

        event Action<ResourceNodeRespawnedEvent> NodeRespawned;

        void NotifyNodeGathered(ResourceNodeGatheredEvent evt);

        void NotifyNodeStateChanged(ResourceNodeStateChangedEvent evt);

        void NotifyNodeDepleted(ResourceNodeDepletedEvent evt);

        void NotifyNodeRespawned(ResourceNodeRespawnedEvent evt);
    }

    public sealed class RuntimeGatheringService : IGatheringService, IDisposable
    {
        private readonly Dictionary<string, ResourceNodeDefinition> _nodeDefinitions =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly List<ResourceNodeBehaviour> _activeNodes = new();

        private int _totalGatherCount;
        private int _totalDepletedCount;
        private int _totalRespawnedCount;

        [ThreadStatic]
        private static System.Random s_sharedRandom;

        public RuntimeGatheringService()
        {
            InstanceId = Guid.NewGuid();
        }

        public Guid InstanceId { get; }

        public int RegisteredNodeCount
        {
            get { return _nodeDefinitions.Count; }
        }

        public int ActiveNodeCount
        {
            get { return _activeNodes.Count; }
        }

        public int TotalGatherCount
        {
            get { return _totalGatherCount; }
        }

        public int TotalDepletedCount
        {
            get { return _totalDepletedCount; }
        }

        public int TotalRespawnedCount
        {
            get { return _totalRespawnedCount; }
        }

        public event Action<ResourceNodeGatheredEvent> NodeGathered;
        public event Action<ResourceNodeStateChangedEvent> NodeStateChanged;
        public event Action<ResourceNodeDepletedEvent> NodeDepleted;
        public event Action<ResourceNodeRespawnedEvent> NodeRespawned;

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
            return new List<ResourceNodeDefinition>(_nodeDefinitions.Values);
        }

        public void RegisterActiveNode(ResourceNodeBehaviour node)
        {
            if (node == null || _activeNodes.Contains(node))
            {
                return;
            }

            _activeNodes.Add(node);
        }

        public void UnregisterActiveNode(ResourceNodeBehaviour node)
        {
            if (node == null)
            {
                return;
            }

            _activeNodes.Remove(node);
        }

        public IReadOnlyList<ResourceNodeBehaviour> GetAllActiveNodes()
        {
            return _activeNodes.AsReadOnly();
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

        public GatheringValidationResult ValidateGathering(
            ResourceNodeBehaviour node,
            IGatheringTool tool,
            float playerStamina,
            float distanceToNode)
        {
            if (node == null)
            {
                return GatheringValidationResult.Failed(
                    GatheringFailureReason.InvalidNode,
                    "Resource node instance is null.");
            }

            return node.ValidateGathering(tool, playerStamina, distanceToNode);
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

            // Surplus harvest power above hardness provides extra yield scaling
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

        public GatheringHarvestResult ProcessGatheringAction(
            ResourceNodeBehaviour node,
            IGatheringTool tool,
            GameObject interactor)
        {
            if (node == null)
            {
                return GatheringHarvestResult.Failed("Resource node is null.");
            }

            var result = node.Harvest(tool, interactor);
            return result;
        }

        public void NotifyNodeGathered(ResourceNodeGatheredEvent evt)
        {
            _totalGatherCount++;
            NodeGathered?.Invoke(evt);
        }

        public void NotifyNodeStateChanged(ResourceNodeStateChangedEvent evt)
        {
            NodeStateChanged?.Invoke(evt);
        }

        public void NotifyNodeDepleted(ResourceNodeDepletedEvent evt)
        {
            _totalDepletedCount++;
            NodeDepleted?.Invoke(evt);
        }

        public void NotifyNodeRespawned(ResourceNodeRespawnedEvent evt)
        {
            _totalRespawnedCount++;
            NodeRespawned?.Invoke(evt);
        }

        public void Dispose()
        {
            _activeNodes.Clear();
            _nodeDefinitions.Clear();

            NodeGathered = null;
            NodeStateChanged = null;
            NodeDepleted = null;
            NodeRespawned = null;
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

        private IGatheringService _gatheringService;
        private IPlayerSpawnService _spawnService;
        private ILogService _logger;

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
            _gatheringService = context.Services.Resolve<IGatheringService>();
            _logger = context.Services.TryResolve<ILogService>(out var resolvedLogger) ? resolvedLogger : null;

            // Load any pre-configured ResourceNodeDefinitions in Resources
            var preloadedNodes = UnityEngine.Resources.LoadAll<ResourceNodeDefinition>("Definitions/Nodes");
            if (preloadedNodes != null)
            {
                for (var i = 0; i < preloadedNodes.Length; i++)
                {
                    if (preloadedNodes[i] != null && !string.IsNullOrWhiteSpace(preloadedNodes[i].NodeCode))
                    {
                        _gatheringService.RegisterNodeDefinition(preloadedNodes[i]);
                    }
                }
            }

            // Register Gathering interaction handler if interaction service is available
            if (context.Services.TryResolve<IInteractionService>(out var interactionService) && interactionService != null)
            {
                var gatheringHandler = new GatheringInteractionHandler(_gatheringService, _logger);
                interactionService.RegisterHandler(gatheringHandler);
                context.RegisterEventSubscription(
                    "Gameplay.Gathering.InteractionHandler",
                    () => interactionService.UnregisterHandler(gatheringHandler),
                    110);
            }

            // Listen for Player spawn to ensure GatheringToolBehaviour is attached to the player
            if (context.Services.TryResolve<IPlayerSpawnService>(out var spawnService) && spawnService != null)
            {
                _spawnService = spawnService;
                _spawnService.PlayerSpawned -= OnPlayerSpawned;
                _spawnService.PlayerSpawned += OnPlayerSpawned;

                context.RegisterEventSubscription(
                    "Gameplay.Gathering.PlayerSpawned",
                    () =>
                    {
                        if (_spawnService != null)
                        {
                            _spawnService.PlayerSpawned -= OnPlayerSpawned;
                        }
                    },
                    110);

                if (_spawnService.HasActivePlayer)
                {
                    TryAttachToolBehaviour(_spawnService.ActivePlayer);
                }
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            context.RegisterEventSubscription(
                "Gameplay.Gathering.SceneLoaded",
                () => SceneManager.sceneLoaded -= OnSceneLoaded,
                110);

            context.RecordRuntimeState("gathering.serviceLifetime", ServiceLifetime.Scoped.ToString());
            context.RecordRuntimeState(
                "gathering.registeredNodeCount",
                _gatheringService.RegisteredNodeCount.ToString(CultureInfo.InvariantCulture));
            context.RecordRuntimeState(
                "gathering.activeNodeCount",
                _gatheringService.ActiveNodeCount.ToString(CultureInfo.InvariantCulture));
            context.RegisterRuntimeResource("Gameplay.Gathering.RuntimeCache", new GatheringRuntimeCache());

            _logger?.Info(
                "Gameplay.Gathering",
                $"Gathering gameplay module initialized with {_gatheringService.RegisteredNodeCount} node definitions.");
        }

        public void Shutdown(GameSessionContext context)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (_spawnService != null)
            {
                _spawnService.PlayerSpawned -= OnPlayerSpawned;
                _spawnService = null;
            }

            if (_gatheringService != null)
            {
                context.RecordRuntimeState(
                    "gathering.registeredNodeCount",
                    _gatheringService.RegisteredNodeCount.ToString(CultureInfo.InvariantCulture));
                context.RecordRuntimeState(
                    "gathering.activeNodeCount",
                    _gatheringService.ActiveNodeCount.ToString(CultureInfo.InvariantCulture));
                context.RecordRuntimeState(
                    "gathering.totalGatherCount",
                    _gatheringService.TotalGatherCount.ToString(CultureInfo.InvariantCulture));
                context.RecordRuntimeState(
                    "gathering.totalDepletedCount",
                    _gatheringService.TotalDepletedCount.ToString(CultureInfo.InvariantCulture));

                if (_gatheringService is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                _gatheringService = null;
            }

            _logger = null;
            context.RecordRuntimeState("gathering.serviceLifetime", ServiceLifetime.Scoped.ToString());
        }

        private void OnPlayerSpawned(GameObject player)
        {
            TryAttachToolBehaviour(player);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryAttachToolBehaviourToActivePlayer();
        }

        private void TryAttachToolBehaviourToActivePlayer()
        {
            if (_spawnService != null && _spawnService.HasActivePlayer)
            {
                TryAttachToolBehaviour(_spawnService.ActivePlayer);
                return;
            }

            var player = GameObject.Find("Worldforge.Player") ?? GameObject.FindWithTag("Player");
            if (player != null)
            {
                TryAttachToolBehaviour(player);
            }
        }

        private void TryAttachToolBehaviour(GameObject playerObject)
        {
            if (playerObject == null)
            {
                return;
            }

            try
            {
                var toolBehaviour = playerObject.GetComponent<GatheringToolBehaviour>();
                if (toolBehaviour == null)
                {
                    toolBehaviour = playerObject.AddComponent<GatheringToolBehaviour>();
                }

                var inventory = playerObject.GetComponent<PlayerInventoryBehaviour>();
                if (inventory != null && !toolBehaviour.HasEquippedTool)
                {
                    inventory.AutoEquipTool();
                }
            }
            catch (Exception ex)
            {
                _logger?.Error("Gameplay.Gathering", "Failed to attach or configure GatheringToolBehaviour.", ex);
            }
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
