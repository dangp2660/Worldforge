using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Inventory.Services
{
    public interface IInventoryService
    {
        int RegisteredContainerCount { get; }

        void RegisterContainer(string containerId);
    }

    public interface IInventorySessionService
    {
        Guid SessionId { get; }
    }

    public sealed class RuntimeInventoryService : IInventoryService
    {
        private readonly HashSet<string> registeredContainers = new(StringComparer.Ordinal);

        public int RegisteredContainerCount
        {
            get { return registeredContainers.Count; }
        }

        public void RegisterContainer(string containerId)
        {
            if (string.IsNullOrWhiteSpace(containerId))
            {
                throw new ArgumentException("Container id must be a non-empty value.", nameof(containerId));
            }

            registeredContainers.Add(containerId);
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

    internal sealed class InventoryServiceRegistrationProvider : IServiceRegistrationProvider
    {
        public int Order
        {
            get { return 100; }
        }

        public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
        {
            services.AddScoped<IInventoryService>(_ => new RuntimeInventoryService());
            services.AddScoped<IInventorySessionService>(_ => new InventorySessionService());
        }
    }

    internal sealed class InventoryInitializationSystemProvider : IGameSessionSystemProvider
    {
        public int Order
        {
            get { return 100; }
        }

        public IEnumerable<IGameSessionSystem> CreateSystems()
        {
            return new IGameSessionSystem[]
            {
                new InventoryInitializationSystem()
            };
        }
    }

    internal sealed class InventoryInitializationSystem : IGameSessionSystem
    {
        private static readonly IReadOnlyList<string> NoDependencies = Array.Empty<string>();

        private GameObject runtimeRoot;
        private ILogService logger;

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
            logger = context.Services.TryResolve<ILogService>(out var resolvedLogger) ? resolvedLogger : null;

            runtimeRoot = new GameObject("Worldforge.Inventory.RuntimeRoot")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            context.RegisterTemporaryObject("Gameplay.Inventory.RuntimeRoot", runtimeRoot);
            context.RecordRuntimeState(
                "inventory.registeredContainerCount",
                inventoryService.RegisteredContainerCount.ToString(CultureInfo.InvariantCulture));

            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            context.RegisterEventSubscription(
                "Gameplay.Inventory.ActiveSceneChanged",
                () => SceneManager.activeSceneChanged -= OnActiveSceneChanged,
                100);

            logger?.Info("Gameplay.Inventory", "Inventory gameplay module initialized.");
        }

        public void Shutdown(GameSessionContext context)
        {
            if (context.Services.TryResolve<IInventoryService>(out var inventoryService) && inventoryService != null)
            {
                context.RecordRuntimeState(
                    "inventory.registeredContainerCount",
                    inventoryService.RegisteredContainerCount.ToString(CultureInfo.InvariantCulture));
            }

            runtimeRoot = null;
            logger = null;
        }

        private void OnActiveSceneChanged(Scene previousScene, Scene nextScene)
        {
            logger?.Info(
                "Gameplay.Inventory",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Inventory runtime observed active scene change: '{0}' -> '{1}'.",
                    previousScene.path,
                    nextScene.path));
        }
    }
}
