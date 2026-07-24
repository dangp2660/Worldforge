using UnityEngine;
using UnityEngine.EventSystems;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Infrastructure.Development
{
    public sealed class DevelopmentSceneManager : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Light mainDirectionalLight;
        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private GameObject globalVolumeObject;
        [SerializeField] private Transform environmentRoot;

        private void Start()
        {
            ValidateSceneReferences();
            LogBootstrapState();
        }

        private void ValidateSceneReferences()
        {
            if (mainCamera == null)
            {
                Debug.LogWarning("[Worldforge] [Warning] [Development.Scene] Main Camera is not assigned.");
            }

            if (mainDirectionalLight == null)
            {
                Debug.LogWarning("[Worldforge] [Warning] [Development.Scene] Main directional light is not assigned.");
            }

            if (eventSystem == null)
            {
                Debug.LogWarning("[Worldforge] [Warning] [Development.Scene] EventSystem is not assigned.");
            }

            if (globalVolumeObject == null)
            {
                Debug.LogWarning("[Worldforge] [Warning] [Development.Scene] Global Volume object is not assigned.");
            }

            if (environmentRoot == null)
            {
                Debug.LogWarning("[Worldforge] [Warning] [Development.Scene] Environment root is not assigned.");
            }
        }

        private void LogBootstrapState()
        {
            if (!BootstrapManager.HasInstance)
            {
                Debug.LogWarning("[Worldforge] [Warning] [Development.Scene] BootstrapManager is not available.");
                return;
            }

            if (!BootstrapManager.TryResolve<IApplicationInfoService>(out var applicationInfo) || applicationInfo == null)
            {
                Debug.LogWarning("[Worldforge] [Warning] [Development.Scene] Application info service is not available.");
                return;
            }

            var loadedSystems = applicationInfo.LoadedSystems == null
                ? "None"
                : string.Join(", ", applicationInfo.LoadedSystems);
            var loadedModules = applicationInfo.LoadedGameplayModules == null
                ? "None"
                : string.Join(", ", applicationInfo.LoadedGameplayModules);

            Debug.Log(
                $"[Worldforge] [Info] [Development.Scene] Startup scene '{applicationInfo.StartupScenePath}', " +
                $"active scene '{applicationInfo.ActiveScenePath}', systems [{loadedSystems}], gameplay modules [{loadedModules}].");
        }
    }
}
