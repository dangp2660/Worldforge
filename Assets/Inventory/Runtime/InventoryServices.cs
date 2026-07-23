using System;
using System.Collections.Generic;
using Worldforge.Core.Bootstrap;

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
}
