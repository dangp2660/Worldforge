using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Infrastructure.Development
{

    public interface IDevelopmentDebugCommand
    {
        string Name { get; }

        string Description { get; }

        bool Execute(DevelopmentDebugManager manager);
    }

    public sealed class DevelopmentDebugManager : MonoBehaviour
    {
        private const string RootName = "Worldforge.DevelopmentDebug";
        private const string SettingsResourcePath = "WorldforgeDevelopmentDebugSettings";
        private const float OverlayWidth = 420f;
        private const float OverlayHeight = 120f;

        private readonly Dictionary<string, IDevelopmentDebugCommand> commands =
            new(StringComparer.OrdinalIgnoreCase);

        private ApplicationBootstrapContext bootstrapContext;
        private DevelopmentDebugSettings settings;
        private GUIStyle overlayStyle;
        private bool builtInCommandsRegistered;

        public static DevelopmentDebugManager Instance { get; private set; }

        public DevelopmentDebugSettings Settings
        {
            get
            {
                EnsureSettingsLoaded();
                return settings;
            }
        }

        public IReadOnlyCollection<string> CommandNames
        {
            get { return commands.Keys; }
        }

        public static DevelopmentDebugManager EnsureCreated(ApplicationBootstrapContext context)
        {
#if UNITY_EDITOR || WORLDFORGE_DEVELOPMENT_BUILD || WORLDFORGE_DEBUG_TOOLS
            if (Instance != null)
            {
                Instance.Initialize(context);
                return Instance;
            }

            var root = new GameObject(RootName);
            DontDestroyOnLoad(root);

            var manager = root.AddComponent<DevelopmentDebugManager>();
            manager.Initialize(context);
            return manager;
#else
            return null;
#endif
        }

        public static void DestroyInstance()
        {
            if (Instance == null)
            {
                return;
            }

            var manager = Instance;
            Instance = null;

            if (manager == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(manager.gameObject);
                return;
            }

            DestroyImmediate(manager.gameObject);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSettingsLoaded();
            RegisterBuiltInCommands();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (!AreDebugToolsEnabled() || !Settings.EnableShortcuts)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            HandleShortcut(keyboard, Settings.ToggleOverlayKey, "debug.overlay.toggle");
            HandleShortcut(keyboard, Settings.ReportBootstrapStateKey, "bootstrap.report");
            HandleShortcut(keyboard, Settings.ReloadSceneKey, "scene.reload");
            HandleShortcut(keyboard, Settings.ToggleGizmosKey, "debug.gizmos.toggle");
            HandleShortcut(keyboard, Settings.ToggleMethodTesterKey, "debug.methodtester.toggle");
            HandleShortcut(keyboard, Key.F12, "debug.methodtester.toggle");
            HandleShortcut(keyboard, Settings.QuitSessionKey, "application.quit");
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || !AreDebugToolsEnabled())
            {
                return;
            }

            // Always draw MethodTesterGUI if open
            MethodTester.MethodTesterGUI.Instance.DrawGUI();

            if (!Settings.EnableOnScreenOverlay)
            {
                return;
            }

            overlayStyle ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 12,
                richText = true,
                wordWrap = true,
                padding = new RectOffset(12, 12, 10, 10)
            };

            GUI.Box(new Rect(12f, 12f, OverlayWidth, OverlayHeight), GUIContent.none, overlayStyle);
            GUILayout.BeginArea(new Rect(24f, 20f, OverlayWidth - 24f, OverlayHeight - 20f));
            GUILayout.Label(BuildOverlayText());
            GUILayout.EndArea();
        }

        private void OnDrawGizmos()
        {
            if (!AreDebugToolsEnabled() || !Settings.EnableGizmos)
            {
                return;
            }

            DrawEnvironmentBoundsGizmo();
            DrawSpawnAndMarkerGizmos();
            DrawCameraGizmo();
        }

        public void Initialize(ApplicationBootstrapContext context)
        {
            bootstrapContext = context;
            EnsureSettingsLoaded();
            RegisterBuiltInCommands();
        }

        public void RegisterCommand(IDevelopmentDebugCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (string.IsNullOrWhiteSpace(command.Name))
            {
                throw new InvalidOperationException("Debug command name must be a non-empty value.");
            }

            commands[command.Name] = command;
        }

        public bool ExecuteCommand(string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                return false;
            }

            if (!commands.TryGetValue(commandName, out var command))
            {
                LogWarning("Development.Debug", $"Unknown debug command '{commandName}'.");
                return false;
            }

            try
            {
                var result = command.Execute(this);
                if (result && Settings.EnableCommandLogging)
                {
                    LogInfo("Development.Debug", $"Executed debug command '{command.Name}'.");
                }

                return result;
            }
            catch (Exception exception)
            {
                LogError("Development.Debug", $"Debug command '{command.Name}' failed.", exception);
                return false;
            }
        }

        public string BuildBootstrapSummary()
        {
            var startupScenePath = bootstrapContext?.StartupScenePath;
            var activeScenePath = SceneManager.GetActiveScene().path;
            var hasActiveGameSession = false;
            var gameSessionState = Worldforge.Core.Bootstrap.GameSessionState.Inactive;
            var activeGameSessionId = "None";
            var playerSpawnSource = "Unknown";
            var loadedSessionSystems = "None";

            if (BootstrapManager.TryResolve<IApplicationInfoService>(out var applicationInfo) && applicationInfo != null)
            {
                startupScenePath = applicationInfo.StartupScenePath;
                activeScenePath = applicationInfo.ActiveScenePath;
                hasActiveGameSession = applicationInfo.HasActiveGameSession;
                gameSessionState = applicationInfo.GameSessionState;
                activeGameSessionId = ValueOrFallback(applicationInfo.ActiveGameSessionId);
                playerSpawnSource = ValueOrFallback(applicationInfo.PlayerSpawnSource);
                loadedSessionSystems = applicationInfo.LoadedGameSessionSystems != null &&
                                       applicationInfo.LoadedGameSessionSystems.Count > 0
                    ? string.Join(", ", applicationInfo.LoadedGameSessionSystems)
                    : "None";
            }

            var systems = bootstrapContext?.LoadedSystems != null && bootstrapContext.LoadedSystems.Count > 0
                ? string.Join(", ", bootstrapContext.LoadedSystems)
                : "None";
            var modules = bootstrapContext?.LoadedGameplayModules != null && bootstrapContext.LoadedGameplayModules.Count > 0
                ? string.Join(", ", bootstrapContext.LoadedGameplayModules)
                : "None";

            return $"Startup scene: {ValueOrFallback(startupScenePath)}, active scene: {ValueOrFallback(activeScenePath)}, " +
                   $"systems: [{systems}], gameplay modules: [{modules}], session active: {hasActiveGameSession}, " +
                   $"session state: {gameSessionState}, session id: {activeGameSessionId}, " +
                   $"session systems: [{loadedSessionSystems}], spawn source: {playerSpawnSource}.";
        }

        public void ToggleOverlay()
        {
            Settings.EnableOnScreenOverlay = !Settings.EnableOnScreenOverlay;
            LogInfo("Development.Debug", $"Overlay {(Settings.EnableOnScreenOverlay ? "enabled" : "disabled")}.");
        }

        public void ToggleGizmos()
        {
            Settings.EnableGizmos = !Settings.EnableGizmos;
            LogInfo("Development.Debug", $"Gizmos {(Settings.EnableGizmos ? "enabled" : "disabled")}.");
        }

        public void LogInfo(string category, string message)
        {
            if (TryResolveLogger(out var logger))
            {
                logger.Info(category, message);
                return;
            }

            Debug.Log($"[Worldforge] [Info] [{category}] {message}");
        }

        public void LogWarning(string category, string message)
        {
            if (TryResolveLogger(out var logger))
            {
                logger.Warning(category, message);
                return;
            }

            Debug.LogWarning($"[Worldforge] [Warning] [{category}] {message}");
        }

        public void LogError(string category, string message, Exception exception = null)
        {
            if (TryResolveLogger(out var logger))
            {
                logger.Error(category, message, exception);
                return;
            }

            Debug.LogError($"[Worldforge] [Error] [{category}] {message}");
            if (exception != null)
            {
                Debug.LogException(exception);
            }
        }

        private void EnsureSettingsLoaded()
        {
            if (settings != null)
            {
                return;
            }

            var configuredSettings = Resources.Load<DevelopmentDebugSettings>(SettingsResourcePath);
            settings = configuredSettings != null
                ? Instantiate(configuredSettings)
                : CreateRuntimeFallbackSettings();
            settings.hideFlags = HideFlags.DontSave;
        }

        private bool AreDebugToolsEnabled()
        {
            return Settings.EnableDebugTools &&
                   (Application.isEditor || Debug.isDebugBuild);
        }

        private void RegisterBuiltInCommands()
        {
            if (builtInCommandsRegistered)
            {
                return;
            }

            RegisterCommand(new ReportBootstrapStateCommand());
            RegisterCommand(new ReloadSceneCommand());
            RegisterCommand(new ToggleOverlayCommand());
            RegisterCommand(new ToggleGizmosCommand());
            RegisterCommand(new ToggleMethodTesterCommand());
            RegisterCommand(new QuitApplicationCommand());

            builtInCommandsRegistered = true;
        }

        private void HandleShortcut(Keyboard keyboard, Key key, string commandName)
        {
            if (key == Key.None)
            {
                return;
            }

            var control = keyboard[key];
            if (control != null && control.wasPressedThisFrame)
            {
                ExecuteCommand(commandName);
            }
        }

        private string BuildOverlayText()
        {
            var overlayLines = new[]
            {
                "<b>Worldforge Development Debug</b>",
                BuildBootstrapSummary(),
                $"Commands: {commands.Count} | Gizmos: {(Settings.EnableGizmos ? "On" : "Off")} | Overlay: {(Settings.EnableOnScreenOverlay ? "On" : "Off")}",
                $"{Settings.ToggleOverlayKey} Overlay | {Settings.ToggleMethodTesterKey} Method Tester | {Settings.ReloadSceneKey} Reload | {Settings.QuitSessionKey} Quit"
            };

            return string.Join(Environment.NewLine, overlayLines);
        }

        private void DrawEnvironmentBoundsGizmo()
        {
            var environmentManager = FindAnyObjectByType<DevelopmentEnvironmentManager>();
            if (environmentManager == null)
            {
                return;
            }

            Gizmos.color = Settings.EnvironmentBoundsColor;
            var root = environmentManager.EnvironmentRoot;
            var center = root.position + Vector3.up * (Settings.EnvironmentBoundsSize.y * 0.5f);
            Gizmos.DrawWireCube(center, Settings.EnvironmentBoundsSize);
        }

        private void DrawSpawnAndMarkerGizmos()
        {
            var environmentManager = FindAnyObjectByType<DevelopmentEnvironmentManager>();
            if (environmentManager == null)
            {
                return;
            }

            var root = environmentManager.EnvironmentRoot;
            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child.name.IndexOf("PlayerSpawn", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Gizmos.color = Settings.SpawnPointColor;
                    Gizmos.DrawSphere(child.position, Settings.MarkerSphereRadius);
                    Gizmos.DrawRay(child.position, Vector3.up * 2.5f);
                    continue;
                }

                if (child.name.IndexOf("Marker", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Gizmos.color = Settings.MarkerColor;
                    Gizmos.DrawWireSphere(child.position, Settings.MarkerSphereRadius);
                }
            }
        }

        private void DrawCameraGizmo()
        {
            var cameraToDraw = Camera.main;
            if (cameraToDraw == null)
            {
                return;
            }

            Gizmos.color = Settings.CameraForwardColor;
            Gizmos.DrawRay(cameraToDraw.transform.position, cameraToDraw.transform.forward * Settings.CameraForwardLength);
        }

        private bool TryResolveLogger(out ILogService logger)
        {
            if (BootstrapManager.TryResolve<ILogService>(out logger) && logger != null)
            {
                return true;
            }

            logger = null;
            return false;
        }

        private static string ValueOrFallback(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Unknown" : value;
        }

        private static DevelopmentDebugSettings CreateRuntimeFallbackSettings()
        {
            var fallbackSettings = DevelopmentDebugSettings.CreateDefaultInstance();
            fallbackSettings.hideFlags = HideFlags.DontSave;
            return fallbackSettings;
        }

        private static bool TryStopPlayModeInEditor()
        {
            var editorApplicationType = Type.GetType("UnityEditor.EditorApplication, UnityEditor");
            if (editorApplicationType == null)
            {
                return false;
            }

            var isPlayingProperty = editorApplicationType.GetProperty(
                "isPlaying",
                BindingFlags.Public | BindingFlags.Static);
            if (isPlayingProperty == null || !isPlayingProperty.CanWrite)
            {
                return false;
            }

            isPlayingProperty.SetValue(null, false);
            return true;
        }

        private sealed class ReportBootstrapStateCommand : IDevelopmentDebugCommand
        {
            public string Name
            {
                get { return "bootstrap.report"; }
            }

            public string Description
            {
                get { return "Log the current bootstrap state."; }
            }

            public bool Execute(DevelopmentDebugManager manager)
            {
                manager.LogInfo("Development.Debug", manager.BuildBootstrapSummary());
                return true;
            }
        }

        private sealed class ReloadSceneCommand : IDevelopmentDebugCommand
        {
            public string Name
            {
                get { return "scene.reload"; }
            }

            public string Description
            {
                get { return "Reload the active scene."; }
            }

            public bool Execute(DevelopmentDebugManager manager)
            {
                var activeScene = SceneManager.GetActiveScene();
                if (!activeScene.IsValid() || !activeScene.isLoaded)
                {
                    manager.LogWarning("Development.Debug", "There is no active loaded scene to reload.");
                    return false;
                }

                if (activeScene.buildIndex >= 0)
                {
                    SceneManager.LoadScene(activeScene.buildIndex);
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(activeScene.path))
                {
                    SceneManager.LoadScene(activeScene.path);
                    return true;
                }

                manager.LogWarning("Development.Debug", "The active scene cannot be reloaded because it has no build index or path.");
                return false;
            }
        }

        private sealed class ToggleOverlayCommand : IDevelopmentDebugCommand
        {
            public string Name
            {
                get { return "debug.overlay.toggle"; }
            }

            public string Description
            {
                get { return "Toggle the on-screen debug overlay."; }
            }

            public bool Execute(DevelopmentDebugManager manager)
            {
                manager.ToggleOverlay();
                return true;
            }
        }

        private sealed class ToggleGizmosCommand : IDevelopmentDebugCommand
        {
            public string Name
            {
                get { return "debug.gizmos.toggle"; }
            }

            public string Description
            {
                get { return "Toggle debug gizmos."; }
            }

            public bool Execute(DevelopmentDebugManager manager)
            {
                manager.ToggleGizmos();
                return true;
            }
        }

        private sealed class ToggleMethodTesterCommand : IDevelopmentDebugCommand
        {
            public string Name
            {
                get { return "debug.methodtester.toggle"; }
            }

            public string Description
            {
                get { return "Toggle In-Game Method Tester tool."; }
            }

            public bool Execute(DevelopmentDebugManager manager)
            {
                MethodTester.MethodTesterManager.Instance.ToggleWindow();
                return true;
            }
        }

        private sealed class QuitApplicationCommand : IDevelopmentDebugCommand
        {
            public string Name
            {
                get { return "application.quit"; }
            }

            public string Description
            {
                get { return "Stop play mode in the editor or request application quit."; }
            }

            public bool Execute(DevelopmentDebugManager manager)
            {
                if (Application.isEditor && TryStopPlayModeInEditor())
                {
                    return true;
                }

                if (!BootstrapManager.HasInstance)
                {
                    manager.LogWarning("Development.Debug", "BootstrapManager is not available for shutdown.");
                    return false;
                }

                BootstrapManager.Instance.RequestShutdownAndQuit();
                return true;
            }
        }
    }
}

namespace Worldforge.Core.Bootstrap
{
    using System.Collections.Generic;
    using Worldforge.Core.Services;
    using Worldforge.Infrastructure.Development;

    internal sealed class DevelopmentDebugSystemProvider : IApplicationSystemProvider
    {
        public int Order
        {
            get { return 200; }
        }

        public IEnumerable<IApplicationSystem> CreateSystems()
        {
            yield return new DevelopmentDebugBootstrapSystem();
        }
    }

    internal sealed class DevelopmentDebugBootstrapSystem : IApplicationSystem
    {
        private static readonly IReadOnlyList<string> DependenciesList = new[] { "SceneFlow" };

        public string Name
        {
            get { return "Development.Debug"; }
        }

        public int Order
        {
            get { return 200; }
        }

        public ApplicationSystemCategory Category
        {
            get { return ApplicationSystemCategory.Core; }
        }

        public IReadOnlyList<string> Dependencies
        {
            get { return DependenciesList; }
        }

        public void Initialize(ApplicationBootstrapContext context)
        {
#if UNITY_EDITOR || WORLDFORGE_DEVELOPMENT_BUILD || WORLDFORGE_DEBUG_TOOLS
            DevelopmentDebugManager.EnsureCreated(context);
            context.RegisterCleanupOperation("Development.Debug.DestroyManager", _ => DevelopmentDebugManager.DestroyInstance());

            if (context.Services != null &&
                context.Services.TryResolve<ILogService>(out var logger) &&
                logger != null)
            {
                logger.Info("Development.Debug", "Development debug manager is ready.");
            }
#endif
        }

        public void Shutdown(ApplicationBootstrapContext context)
        {
        }
    }
}
