using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Worldforge.Character.Spawning;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;
using Worldforge.Item;

namespace Worldforge.Inventory.Services
{
    /// <summary>
    /// Service contract for managing, querying, and routing inventory containers across the game runtime.
    /// </summary>
    public interface IInventoryService
    {
        int RegisteredContainerCount { get; }

        IInventoryContainer PlayerInventory { get; }

        event Action<InventoryItemAddedEvent> GlobalItemAdded;
        event Action<InventoryItemRemovedEvent> GlobalItemRemoved;
        event Action<InventoryChangedEvent> GlobalInventoryChanged;
        event Action<InventoryEncumbranceChangedEvent> GlobalEncumbranceChanged;

        void RegisterContainer(IInventoryContainer container);
        void UnregisterContainer(string containerId);
        bool TryGetContainer(string containerId, out IInventoryContainer container);
        IInventoryContainer GetContainer(string containerId);
        IReadOnlyList<IInventoryContainer> GetAllContainers();
        bool TransferItem(IInventoryContainer source, IInventoryContainer target, int sourceSlotIndex, int amount);
    }

    public interface IInventorySessionService
    {
        Guid SessionId { get; }
    }

    /// <summary>
    /// Runtime implementation of <see cref="IInventoryService"/>.
    /// Handles container registrations, event aggregation, and inter-container transfers.
    /// </summary>
    public sealed class RuntimeInventoryService : IInventoryService, IDisposable
    {
        private const string DefaultPlayerContainerId = "PlayerInventory";

        private readonly Dictionary<string, IInventoryContainer> _containerMap = new(StringComparer.Ordinal);
        private readonly List<IInventoryContainer> _containerList = new();

        public event Action<InventoryItemAddedEvent> GlobalItemAdded;
        public event Action<InventoryItemRemovedEvent> GlobalItemRemoved;
        public event Action<InventoryChangedEvent> GlobalInventoryChanged;
        public event Action<InventoryEncumbranceChangedEvent> GlobalEncumbranceChanged;

        public int RegisteredContainerCount
        {
            get { return _containerMap.Count; }
        }

        public IInventoryContainer PlayerInventory
        {
            get
            {
                if (_containerMap.TryGetValue(DefaultPlayerContainerId, out var container))
                {
                    return container;
                }

                return _containerList.Count > 0 ? _containerList[0] : null;
            }
        }

        public void RegisterContainer(IInventoryContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            var id = container.ContainerId;
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Container id must be a non-empty value.", nameof(container));
            }

            if (_containerMap.TryGetValue(id, out var existing))
            {
                if (ReferenceEquals(existing, container))
                {
                    return;
                }

                UnsubscribeFromContainer(existing);
                _containerList.Remove(existing);
            }

            _containerMap[id] = container;
            _containerList.Add(container);
            SubscribeToContainer(container);
        }

        public void UnregisterContainer(string containerId)
        {
            if (string.IsNullOrWhiteSpace(containerId))
            {
                return;
            }

            if (_containerMap.TryGetValue(containerId, out var container))
            {
                UnsubscribeFromContainer(container);
                _containerMap.Remove(containerId);
                _containerList.Remove(container);
            }
        }

        public bool TryGetContainer(string containerId, out IInventoryContainer container)
        {
            if (string.IsNullOrWhiteSpace(containerId))
            {
                container = null;
                return false;
            }

            return _containerMap.TryGetValue(containerId, out container);
        }

        public IInventoryContainer GetContainer(string containerId)
        {
            TryGetContainer(containerId, out var container);
            return container;
        }

        public IReadOnlyList<IInventoryContainer> GetAllContainers()
        {
            return _containerList;
        }

        public bool TransferItem(IInventoryContainer source, IInventoryContainer target, int sourceSlotIndex, int amount)
        {
            if (source == null || target == null || amount <= 0)
            {
                return false;
            }

            var slot = source.GetSlot(sourceSlotIndex);
            if (slot == null || slot.IsEmpty || slot.Item == null)
            {
                return false;
            }

            if (!target.CanAcceptItem(slot.Item, Mathf.Min(slot.Quantity, amount)))
            {
                return false;
            }

            if (!source.RemoveItemAt(sourceSlotIndex, amount, out var removedStack) || removedStack == null)
            {
                return false;
            }

            var added = target.AddItem(removedStack);
            if (added < removedStack.Quantity)
            {
                // Return overflow back to source
                var overflow = removedStack.Quantity - added;
                source.AddItem(removedStack.Item, overflow);
            }

            return added > 0;
        }

        public void Dispose()
        {
            for (var i = 0; i < _containerList.Count; i++)
            {
                UnsubscribeFromContainer(_containerList[i]);
            }

            _containerMap.Clear();
            _containerList.Clear();
        }

        private void SubscribeToContainer(IInventoryContainer container)
        {
            if (container == null) return;

            container.ItemAdded += ForwardItemAdded;
            container.ItemRemoved += ForwardItemRemoved;
            container.InventoryChanged += ForwardInventoryChanged;
            container.EncumbranceChanged += ForwardEncumbranceChanged;
        }

        private void UnsubscribeFromContainer(IInventoryContainer container)
        {
            if (container == null) return;

            container.ItemAdded -= ForwardItemAdded;
            container.ItemRemoved -= ForwardItemRemoved;
            container.InventoryChanged -= ForwardInventoryChanged;
            container.EncumbranceChanged -= ForwardEncumbranceChanged;
        }

        private void ForwardItemAdded(InventoryItemAddedEvent evt)
        {
            GlobalItemAdded?.Invoke(evt);
        }

        private void ForwardItemRemoved(InventoryItemRemovedEvent evt)
        {
            GlobalItemRemoved?.Invoke(evt);
        }

        private void ForwardInventoryChanged(InventoryChangedEvent evt)
        {
            GlobalInventoryChanged?.Invoke(evt);
        }

        private void ForwardEncumbranceChanged(InventoryEncumbranceChangedEvent evt)
        {
            GlobalEncumbranceChanged?.Invoke(evt);
        }
    }

    public sealed class InventorySessionService : IInventorySessionService
    {
        public InventorySessionService()
        {
            SessionId = Guid.NewGuid();
        }

        public Guid SessionId { get; }
    }

    public sealed class InventoryServiceRegistrationProvider : IServiceRegistrationProvider
    {
        public int Order
        {
            get { return 100; }
        }

        public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
        {
            services.AddSingleton<IInventoryService>(_ => new RuntimeInventoryService());
            services.AddScoped<IInventorySessionService>(_ => new InventorySessionService());
        }
    }

    public sealed class InventoryApplicationSystemProvider : IApplicationSystemProvider
    {
        public int Order
        {
            get { return 136; }
        }

        public IEnumerable<IApplicationSystem> CreateSystems()
        {
            return new IApplicationSystem[]
            {
                new InventoryApplicationInitializationSystem()
            };
        }
    }

    public sealed class InventoryGameSessionSystemProvider : IGameSessionSystemProvider
    {
        public int Order
        {
            get { return 100; }
        }

        public IEnumerable<IGameSessionSystem> CreateSystems()
        {
            return new IGameSessionSystem[]
            {
                new InventoryGameSessionInitializationSystem()
            };
        }
    }

    /// <summary>
    /// Application-level initialization system ensuring player inventory is attached and registered.
    /// Order 136 — runs right after CharacterState (130) and Interaction (135).
    /// </summary>
    internal sealed class InventoryApplicationInitializationSystem : IApplicationSystem
    {
        private static readonly IReadOnlyList<string> DependenciesList =
            new[] { "Gameplay.PlayerSpawn", "Gameplay.CharacterState" };

        private IInventoryService _inventoryService;
        private IPlayerSpawnService _spawnService;
        private ILogService _logger;

        public string Name
        {
            get { return "Gameplay.Inventory.Application"; }
        }

        public int Order
        {
            get { return 136; }
        }

        public ApplicationSystemCategory Category
        {
            get { return ApplicationSystemCategory.Gameplay; }
        }

        public IReadOnlyList<string> Dependencies
        {
            get { return DependenciesList; }
        }

        public void Initialize(ApplicationBootstrapContext context)
        {
            _inventoryService = context.Services.Resolve<IInventoryService>();

            if (context.Services.TryResolve<IPlayerSpawnService>(out var resolvedSpawnService))
            {
                _spawnService = resolvedSpawnService;
            }

            if (context.Services.TryResolve<ILogService>(out var resolvedLogger))
            {
                _logger = resolvedLogger;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (_spawnService != null)
            {
                _spawnService.PlayerSpawned -= OnPlayerSpawned;
                _spawnService.PlayerSpawned += OnPlayerSpawned;
            }

            context.RegisterEventSubscription(
                "Gameplay.Inventory.SceneLoaded",
                () => SceneManager.sceneLoaded -= OnSceneLoaded,
                136);

            TryAttachAndRegisterActivePlayer();

            _logger?.Info("Gameplay.Inventory", "Inventory application system initialized.");
        }

        public void Shutdown(ApplicationBootstrapContext context)
        {
            if (_spawnService != null)
            {
                _spawnService.PlayerSpawned -= OnPlayerSpawned;
            }

            if (_inventoryService is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _inventoryService = null;
            _spawnService = null;
            _logger = null;
        }

        private void OnPlayerSpawned(GameObject player)
        {
            TryAttachAndRegisterPlayer(player);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryAttachAndRegisterActivePlayer();
        }

        private void TryAttachAndRegisterActivePlayer()
        {
            if (_spawnService != null && _spawnService.HasActivePlayer)
            {
                TryAttachAndRegisterPlayer(_spawnService.ActivePlayer);
                return;
            }

            var player = GameObject.Find("Worldforge.Player") ?? GameObject.FindWithTag("Player");
            if (player != null)
            {
                TryAttachAndRegisterPlayer(player);
            }
        }

        private void TryAttachAndRegisterPlayer(GameObject playerObject)
        {
            if (playerObject == null || _inventoryService == null)
            {
                return;
            }

            try
            {
                var invBehaviour = playerObject.GetComponent<PlayerInventoryBehaviour>();
                if (invBehaviour == null)
                {
                    invBehaviour = playerObject.AddComponent<PlayerInventoryBehaviour>();
                }

                invBehaviour.InitializeContainer();

                if (invBehaviour.Container != null)
                {
                    _inventoryService.RegisterContainer(invBehaviour.Container);
                    _logger?.Info(
                        "Gameplay.Inventory",
                        $"Player inventory '{invBehaviour.ContainerId}' registered with {_inventoryService.RegisteredContainerCount} total containers.");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error("Gameplay.Inventory", "Failed to attach or register PlayerInventoryBehaviour.", ex);
            }
        }
    }

    /// <summary>
    /// Game session level lifecycle system satisfying session dependencies such as Gameplay.Gathering.
    /// </summary>
    internal sealed class InventoryGameSessionInitializationSystem : IGameSessionSystem
    {
        private static readonly IReadOnlyList<string> NoDependencies = Array.Empty<string>();

        private GameObject _runtimeRoot;
        private ILogService _logger;

        public string Name
        {
            get { return "Gameplay.Inventory"; }
        }

        public int Order
        {
            get { return 100; }
        }

        public IReadOnlyList<string> Dependencies
        {
            get { return NoDependencies; }
        }

        public void Initialize(GameSessionContext context)
        {
            var inventoryService = context.Services.Resolve<IInventoryService>();
            context.Services.Resolve<IInventorySessionService>();
            _logger = context.Services.TryResolve<ILogService>(out var resolvedLogger) ? resolvedLogger : null;

            _runtimeRoot = new GameObject("Worldforge.Inventory.RuntimeRoot")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            context.RegisterTemporaryObject("Gameplay.Inventory.RuntimeRoot", _runtimeRoot);
            TryAttachToExistingPlayer(inventoryService);

            context.RecordRuntimeState(
                "inventory.registeredContainerCount",
                inventoryService.RegisteredContainerCount.ToString(CultureInfo.InvariantCulture));

            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            context.RegisterEventSubscription(
                "Gameplay.Inventory.ActiveSceneChanged",
                () => SceneManager.activeSceneChanged -= OnActiveSceneChanged,
                100);

            _logger?.Info("Gameplay.Inventory", "Inventory game session module initialized.");
        }

        public void Shutdown(GameSessionContext context)
        {
            if (context.Services.TryResolve<IInventoryService>(out var inventoryService) && inventoryService != null)
            {
                context.RecordRuntimeState(
                    "inventory.registeredContainerCount",
                    inventoryService.RegisteredContainerCount.ToString(CultureInfo.InvariantCulture));
            }

            _runtimeRoot = null;
            _logger = null;
        }

        private void OnActiveSceneChanged(Scene previousScene, Scene nextScene)
        {
            _logger?.Info(
                "Gameplay.Inventory",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Inventory runtime observed active scene change: '{0}' -> '{1}'.",
                    previousScene.path,
                    nextScene.path));
        }

        private void TryAttachToExistingPlayer(IInventoryService inventoryService)
        {
            var player = GameObject.Find("Worldforge.Player") ?? GameObject.FindWithTag("Player");
            if (player != null)
            {
                var invBehaviour = player.GetComponent<PlayerInventoryBehaviour>();
                if (invBehaviour == null)
                {
                    invBehaviour = player.AddComponent<PlayerInventoryBehaviour>();
                }

                invBehaviour.InitializeContainer();

                if (invBehaviour.Container != null)
                {
                    inventoryService.RegisterContainer(invBehaviour.Container);
                }
            }
        }
    }
}
