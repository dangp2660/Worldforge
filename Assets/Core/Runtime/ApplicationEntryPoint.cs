using UnityEngine;

namespace Worldforge.Core.Bootstrap
{
    public static class ApplicationEntryPoint
    {
        private const string BootstrapRootName = "Worldforge.Bootstrap";

        private static bool s_IsBootstrapped;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            s_IsBootstrapped = false;
            BootstrapManager.ResetInstance();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (s_IsBootstrapped || BootstrapManager.HasInstance)
            {
                return;
            }

            var bootstrapRoot = new GameObject(BootstrapRootName);
            Object.DontDestroyOnLoad(bootstrapRoot);

            var bootstrapManager = bootstrapRoot.AddComponent<BootstrapManager>();
            bootstrapManager.Initialize(CreateStartupFlow());

            s_IsBootstrapped = true;
        }

        private static ApplicationStartupFlow CreateStartupFlow()
        {
            return new ApplicationStartupFlow(
                new InputBootstrapSystem(),
                new SceneBootstrapSystem());
        }
    }
}
