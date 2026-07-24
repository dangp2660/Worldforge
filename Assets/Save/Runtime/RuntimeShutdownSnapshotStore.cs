using System;
using System.IO;
using UnityEngine;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Save.Runtime
{
    internal sealed class SaveServiceRegistrationProvider : IServiceRegistrationProvider
    {
        public int Order
        {
            get { return 900; }
        }

        public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
        {
            services.AddSingleton<IApplicationShutdownSnapshotStore>(
                resolver => new RuntimeShutdownSnapshotStore(resolver.Resolve<ILogService>()));
        }
    }

    internal sealed class RuntimeShutdownSnapshotStore : IApplicationShutdownSnapshotStore
    {
        private const string SnapshotFileName = "worldforge-runtime-shutdown.json";
        private readonly ILogService logger;

        public RuntimeShutdownSnapshotStore(ILogService logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ApplicationShutdownSnapshot LastSavedSnapshot { get; private set; }

        public string SnapshotPath
        {
            get
            {
                return Path.Combine(Application.persistentDataPath, "Worldforge", SnapshotFileName);
            }
        }

        public void Save(ApplicationShutdownSnapshot snapshot)
        {
            LastSavedSnapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));

            var snapshotDirectory = Path.GetDirectoryName(SnapshotPath);
            if (snapshotDirectory is { Length: > 0 })
            {
                Directory.CreateDirectory(snapshotDirectory);
            }

            File.WriteAllText(SnapshotPath, JsonUtility.ToJson(snapshot, true));

            logger.Info("Save.ShutdownSnapshot", $"Saved shutdown snapshot to '{SnapshotPath}'.");
        }
    }
}
