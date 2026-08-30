using UnityEngine;
using UnityEngine.InputSystem;

namespace Worldforge.Infrastructure.Development
{
    [CreateAssetMenu(
        fileName = "WorldforgeDevelopmentDebugSettings",
        menuName = "Worldforge/Development/Debug Settings")]
    public sealed class DevelopmentDebugSettings : ScriptableObject
    {
        [Header("Runtime")]
        [SerializeField] private bool enableDebugTools = true;
        [SerializeField] private bool enableCommandLogging = true;
        [SerializeField] private bool enableOnScreenOverlay = true;
        [SerializeField] private bool enableGizmos = true;
        [SerializeField] private bool enableShortcuts = true;

        [Header("Shortcuts")]
        [SerializeField] private Key toggleOverlayKey = Key.F1;
        [SerializeField] private Key reportBootstrapStateKey = Key.F2;
        [SerializeField] private Key reloadSceneKey = Key.F3;
        [SerializeField] private Key toggleGizmosKey = Key.F4;
        [SerializeField] private Key toggleMethodTesterKey = Key.Backquote;
        [SerializeField] private Key quitSessionKey = Key.F10;

        [Header("Gizmos")]
        [SerializeField] private Color environmentBoundsColor = new(0.22f, 0.78f, 1f, 0.95f);
        [SerializeField] private Color spawnPointColor = new(0.32f, 1f, 0.45f, 0.95f);
        [SerializeField] private Color markerColor = new(1f, 0.78f, 0.18f, 0.95f);
        [SerializeField] private Color cameraForwardColor = new(1f, 0.35f, 0.35f, 0.95f);
        [SerializeField] private Vector3 environmentBoundsSize = new(40f, 0.25f, 40f);
        [SerializeField] private float markerSphereRadius = 0.45f;
        [SerializeField] private float cameraForwardLength = 8f;

        public bool EnableDebugTools
        {
            get { return enableDebugTools; }
        }

        public bool EnableCommandLogging
        {
            get { return enableCommandLogging; }
        }

        public bool EnableOnScreenOverlay
        {
            get { return enableOnScreenOverlay; }
            set { enableOnScreenOverlay = value; }
        }

        public bool EnableGizmos
        {
            get { return enableGizmos; }
            set { enableGizmos = value; }
        }

        public bool EnableShortcuts
        {
            get { return enableShortcuts; }
        }

        public Key ToggleOverlayKey
        {
            get { return toggleOverlayKey; }
        }

        public Key ReportBootstrapStateKey
        {
            get { return reportBootstrapStateKey; }
        }

        public Key ReloadSceneKey
        {
            get { return reloadSceneKey; }
        }

        public Key ToggleGizmosKey
        {
            get { return toggleGizmosKey; }
        }

        public Key ToggleMethodTesterKey
        {
            get { return toggleMethodTesterKey; }
        }

        public Key QuitSessionKey
        {
            get { return quitSessionKey; }
        }

        public Color EnvironmentBoundsColor
        {
            get { return environmentBoundsColor; }
        }

        public Color SpawnPointColor
        {
            get { return spawnPointColor; }
        }

        public Color MarkerColor
        {
            get { return markerColor; }
        }

        public Color CameraForwardColor
        {
            get { return cameraForwardColor; }
        }

        public Vector3 EnvironmentBoundsSize
        {
            get { return environmentBoundsSize; }
        }

        public float MarkerSphereRadius
        {
            get { return markerSphereRadius; }
        }

        public float CameraForwardLength
        {
            get { return cameraForwardLength; }
        }

        public static DevelopmentDebugSettings CreateDefaultInstance()
        {
            var instance = CreateInstance<DevelopmentDebugSettings>();
            instance.name = nameof(DevelopmentDebugSettings);
            return instance;
        }
    }
}
