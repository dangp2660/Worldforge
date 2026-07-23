using System;
using System.IO;
using UnityEngine;
using Worldforge.Core.Bootstrap;

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
            services.AddSingleton<IApplicationShutdownSnapshotStore>(_ => new RuntimeShutdownSnapshotStore());
        }
    }

    internal sealed class RuntimeShutdownSnapshotStore : IApplicationShutdownSnapshotStore
    {
        private const string SnapshotFileName = "worldforge-runtime-shutdown.json";

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
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            LastSavedSnapshot = snapshot;

            var snapshotDirectory = Path.GetDirectoryName(SnapshotPath);
            if (!string.IsNullOrEmpty(snapshotDirectory))
            {
                Directory.CreateDirectory(snapshotDirectory);
            }

            File.WriteAllText(SnapshotPath, JsonUtility.ToJson(snapshot, true));

            Debug.LogFormat("[Worldforge] Saved shutdown snapshot to '{0}'.", SnapshotPath);
        }
    }
}
