using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
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
        [SerializeField] private bool normalizeDirectionalLightOnStart = true;
        [SerializeField, Min(0f)] private float targetDirectionalLightIntensity = 1.2f;
        [SerializeField] private Color directionalLightColor = new(1f, 0.95686275f, 0.9019608f, 1f);
        [SerializeField, Range(0f, 1f)] private float directionalShadowStrength = 0.82f;
        [SerializeField] private bool applyAmbientLightingOnStart = true;
        [SerializeField] private Color ambientSkyColor = new(0.32f, 0.39f, 0.5f, 1f);
        [SerializeField] private Color ambientEquatorColor = new(0.22f, 0.27f, 0.34f, 1f);
        [SerializeField] private Color ambientGroundColor = new(0.13f, 0.15f, 0.18f, 1f);
        [SerializeField] private bool createFillLightOnStart = true;
        [SerializeField, Min(0f)] private float fillLightIntensity = 0.32f;
        [SerializeField] private Color fillLightColor = new(0.48f, 0.56f, 0.68f, 1f);
        [SerializeField] private Vector3 fillLightEulerAngles = new(28f, -132f, 0f);
        [SerializeField] private bool autoStartNewGame = true;

        private void Start()
        {
            ValidateSceneReferences();
            ApplyDevelopmentLighting();
            EnsureGameSessionStarted();
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
            var loadedSessionSystems = applicationInfo.LoadedGameSessionSystems == null
                ? "None"
                : string.Join(", ", applicationInfo.LoadedGameSessionSystems);

            Debug.Log(
                $"[Worldforge] [Info] [Development.Scene] Startup scene '{applicationInfo.StartupScenePath}', " +
                $"active scene '{applicationInfo.ActiveScenePath}', systems [{loadedSystems}], gameplay modules [{loadedModules}], " +
                $"session active {applicationInfo.HasActiveGameSession}, session state {applicationInfo.GameSessionState}, " +
                $"session systems [{loadedSessionSystems}], spawn prepared {applicationInfo.IsPlayerSpawnPrepared} from '{applicationInfo.PlayerSpawnSource}'.");
        }

        private void EnsureGameSessionStarted()
        {
            if (!autoStartNewGame || !BootstrapManager.HasInstance)
            {
                return;
            }

            if (!BootstrapManager.TryResolve<Worldforge.Core.Bootstrap.IGameSessionManager>(out var gameSessionManager) ||
                gameSessionManager == null)
            {
                Debug.LogWarning("[Worldforge] [Warning] [Development.Scene] Game session manager is not available.");
                return;
            }

            if (gameSessionManager.HasActiveSession)
            {
                return;
            }

            gameSessionManager.StartNewGame();
        }

        private void ApplyDevelopmentLighting()
        {
            ConfigureDirectionalLight();
            ConfigureAmbientLighting();
            EnsureFillLight();
        }

        private void ConfigureDirectionalLight()
        {
            if (!normalizeDirectionalLightOnStart ||
                mainDirectionalLight == null ||
                mainDirectionalLight.type != LightType.Directional)
            {
                return;
            }

            var originalIntensity = mainDirectionalLight.intensity;
            mainDirectionalLight.intensity = targetDirectionalLightIntensity;
            mainDirectionalLight.color = directionalLightColor;
            mainDirectionalLight.shadowStrength = directionalShadowStrength;

            if (!Mathf.Approximately(originalIntensity, targetDirectionalLightIntensity))
            {
                Debug.Log(
                    $"[Worldforge] [Info] [Development.Scene] Adjusted directional light intensity from {originalIntensity:0.###} to {targetDirectionalLightIntensity:0.###}.");
            }
        }

        private void ConfigureAmbientLighting()
        {
            if (!applyAmbientLightingOnStart)
            {
                return;
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
        }

        private void EnsureFillLight()
        {
            if (!createFillLightOnStart)
            {
                return;
            }

            const string fillLightName = "Development.FillLight";
            var fillLightTransform = transform.Find(fillLightName);
            Light fillLight = null;

            if (fillLightTransform == null)
            {
                var fillLightObject = new GameObject(fillLightName);
                fillLightObject.transform.SetParent(transform, false);
                fillLightTransform = fillLightObject.transform;
                fillLight = fillLightObject.AddComponent<Light>();
            }
            else
            {
                fillLight = fillLightTransform.GetComponent<Light>();
                if (fillLight == null)
                {
                    fillLight = fillLightTransform.gameObject.AddComponent<Light>();
                }
            }

            fillLightTransform.localPosition = Vector3.zero;
            fillLightTransform.localRotation = Quaternion.Euler(fillLightEulerAngles);

            fillLight.type = LightType.Directional;
            fillLight.color = fillLightColor;
            fillLight.intensity = fillLightIntensity;
            fillLight.shadows = LightShadows.None;
            fillLight.renderMode = LightRenderMode.ForcePixel;
        }
    }
}
