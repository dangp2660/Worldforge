using System;
using System.Collections.Generic;
using Worldforge.Core.Bootstrap;
using UnityEngine;

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
        private readonly HashSet<string> registeredContainers = new HashSet<string>(StringComparer.Ordinal);

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
            services.AddSingleton<IInventoryService>(_ => new RuntimeInventoryService());
            services.AddScoped<IInventorySessionService>(_ => new InventorySessionService());
        }
    }

    internal sealed class InventoryInitializationSystemProvider : IApplicationSystemProvider
    {
        public int Order
        {
            get { return 100; }
        }

        public IEnumerable<IApplicationSystem> CreateSystems()
        {
            return new IApplicationSystem[]
            {
                new InventoryInitializationSystem()
            };
        }
    }

    internal sealed class InventoryInitializationSystem : IApplicationSystem
    {
        private static readonly IReadOnlyList<string> DependenciesList = new[] { "Input", "SceneFlow" };

        public string Name
        {
            get { return "Gameplay.Inventory"; }
        }

        public int Order
        {
            get { return 100; }
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
            context.Services.Resolve<IInventoryService>();

            Debug.Log("[Worldforge] Inventory gameplay module initialized.");
        }

        public void Shutdown(ApplicationBootstrapContext context)
        {
        }
    }
}
