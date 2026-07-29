using System;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;
using Worldforge.Inventory.Services;

namespace Worldforge.Gathering.Services
{
    public interface IGatheringService
    {
        Guid InstanceId { get; }

        bool CanGather(string nodeId);
    }

    public sealed class RuntimeGatheringService : IGatheringService
    {
        public RuntimeGatheringService()
        {
            InstanceId = Guid.NewGuid();
        }

        public Guid InstanceId { get; }

        public bool CanGather(string nodeId)
        {
            return !string.IsNullOrWhiteSpace(nodeId);
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

    internal sealed class GatheringInitializationSystemProvider : Worldforge.Core.Bootstrap.IGameSessionSystemProvider
    {
        public int Order
        {
            get { return 110; }
        }

        public System.Collections.Generic.IEnumerable<Worldforge.Core.Bootstrap.IGameSessionSystem> CreateSystems()
        {
            return new Worldforge.Core.Bootstrap.IGameSessionSystem[]
            {
                new GatheringInitializationSystem()
            };
        }
    }

    internal sealed class GatheringInitializationSystem : Worldforge.Core.Bootstrap.IGameSessionSystem
    {
        private static readonly System.Collections.Generic.IReadOnlyList<string> DependenciesList =
            new[] { "Gameplay.Inventory" };

        public string Name
        {
            get { return "Gameplay.Gathering"; }
        }

        public int Order
        {
            get { return 110; }
        }

        public System.Collections.Generic.IReadOnlyList<string> Dependencies
        {
            get { return DependenciesList; }
        }

        public void Initialize(Worldforge.Core.Bootstrap.GameSessionContext context)
        {
            context.Services.Resolve<IInventoryService>();
            context.Services.Resolve<IGatheringService>();
            var logger = context.Services.TryResolve<ILogService>(out var resolvedLogger) ? resolvedLogger : null;
            context.RecordRuntimeState("gathering.serviceLifetime", ServiceLifetime.Scoped.ToString());
            context.RegisterRuntimeResource("Gameplay.Gathering.RuntimeCache", new GatheringRuntimeCache());

            logger?.Info("Gameplay.Gathering", "Gathering gameplay module initialized.");
        }

        public void Shutdown(Worldforge.Core.Bootstrap.GameSessionContext context)
        {
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
