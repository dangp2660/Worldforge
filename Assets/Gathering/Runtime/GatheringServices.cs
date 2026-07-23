using System;
using Worldforge.Core.Bootstrap;

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
            services.AddTransient<IGatheringService>(_ => new RuntimeGatheringService());
        }
    }
}
