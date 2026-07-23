using System;

namespace Worldforge.Core.Bootstrap
{
    [Serializable]
    public sealed class ApplicationShutdownSnapshot
    {
        public string applicationName;
        public string applicationVersion;
        public string shutdownReason;
        public string shutdownUtcTimestamp;
        public string startupScenePath;
        public string activeScenePath;
        public string[] loadedSystems;
        public string[] loadedGameplayModules;
        public ApplicationShutdownDataEntry[] runtimeData;
        public string[] saveOperations;
        public string[] cleanupOperations;
        public string[] releasedRuntimeResources;
        public string[] destroyedTemporaryObjects;
        public string[] errors;
    }

    [Serializable]
    public sealed class ApplicationShutdownDataEntry
    {
        public string key;
        public string value;
    }

}
