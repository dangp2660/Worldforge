using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Worldforge.Core.Services;

namespace Worldforge.Core.Bootstrap
{
    public sealed class BootstrapManager : MonoBehaviour
    {
        private ApplicationStartupFlow startupFlow;
        private ApplicationBootstrapContext bootstrapContext;
        private bool isInitialized;
        private bool isShuttingDown;

        public static BootstrapManager Instance { get; private set; }

        public static bool HasInstance
        {
            get { return Instance != null; }
        }

        public IServiceResolver Services
        {
            get { return bootstrapContext?.Services; }
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
        }

        private void OnApplicationQuit()
        {
            Shutdown("ApplicationQuit");
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Shutdown("Destroy");
            ResetInstance();
        }

        public void Initialize(ApplicationStartupFlow flow)
        {
            if (isInitialized)
            {
                return;
            }

            startupFlow = flow ?? throw new ArgumentNullException(nameof(flow));
            bootstrapContext = new ApplicationBootstrapContext(this);
            startupFlow.Initialize(bootstrapContext);
            isInitialized = true;
        }

        public T Resolve<T>()
        {
            if (Services == null)
            {
                throw new InvalidOperationException("Worldforge services are not available before bootstrap completes.");
            }

            return Services.Resolve<T>();
        }

        public static T ResolveRequired<T>()
        {
            if (!HasInstance)
            {
                throw new InvalidOperationException("BootstrapManager has not been created.");
            }

            return Instance.Resolve<T>();
        }

        public static bool TryResolve<T>(out T service)
        {
            if (!HasInstance || Instance.Services == null)
            {
                service = default;
                return false;
            }

            return Instance.Services.TryResolve(out service);
        }

        internal static void ResetInstance()
        {
            Instance = null;
        }

        public void RequestShutdownAndQuit()
        {
            Shutdown("QuitRequested");
            Application.Quit();
        }

        private void Shutdown(string reason)
        {
            if (isShuttingDown || !isInitialized || startupFlow == null)
            {
                return;
            }

            isShuttingDown = true;

            try
            {
                startupFlow.Shutdown(reason);
            }
            finally
            {
                startupFlow = null;
                bootstrapContext = null;
                isInitialized = false;
                isShuttingDown = false;
            }
        }
    }

    public sealed class ApplicationStartupFlow
    {
        private readonly List<IApplicationSystem> declaredSystems;
        private readonly List<IApplicationSystem> executionPlan = new();

        private ApplicationBootstrapContext context;
        private bool isInitialized;

        public ApplicationStartupFlow(params IApplicationSystem[] systems)
        {
            declaredSystems = new List<IApplicationSystem>(systems ?? Array.Empty<IApplicationSystem>());
        }

        public static ApplicationStartupFlow CreateDefault()
        {
            var systems = new List<IApplicationSystem>
            {
                new InputBootstrapSystem(),
                new SceneBootstrapSystem(),
                new GameSessionBootstrapSystem()
            };

            systems.AddRange(ApplicationSystemDiscovery.DiscoverSystems());
            return new ApplicationStartupFlow(systems.ToArray());
        }

        public IReadOnlyList<IApplicationSystem> Systems
        {
            get { return executionPlan.Count > 0 ? executionPlan : declaredSystems; }
        }

        public IServiceResolver Services
        {
            get { return context?.Services; }
        }

        public void Initialize(ApplicationBootstrapContext bootstrapContext)
        {
            if (isInitialized)
            {
                return;
            }

            context = bootstrapContext ?? throw new ArgumentNullException(nameof(bootstrapContext));
            try
            {
                RegisterServices(context);
                var logger = context.GetLoggerOrNull();
                executionPlan.Clear();
                executionPlan.AddRange(BuildExecutionPlan(declaredSystems, logger));

                foreach (var system in executionPlan)
                {
                    if (context.IsSystemLoaded(system.Name))
                    {
                        continue;
                    }

                    system.Initialize(context);
                    context.MarkSystemLoaded(system);
                }
            }
            catch (Exception exception)
            {
                context.GetLoggerOrNull()?.Error("Bootstrap.Initialize", "Application bootstrap failed.", exception);
                context.DisposeServices();
                executionPlan.Clear();
                context = null;
                throw;
            }

            isInitialized = true;

            context.GetLoggerOrNull()?.Info(
                "Bootstrap.Initialize",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Bootstrap complete. Loaded {0} application systems.",
                    context.LoadedSystems.Count));
        }

        public void Shutdown(string reason = "ApplicationExit")
        {
            if (!isInitialized || context == null)
            {
                return;
            }

            var shutdownErrors = new List<string>();
            var shutdownContext = context;

            shutdownContext.BeginShutdown(reason);
            var executedSaveOperations = shutdownContext.ExecuteSaveOperations(shutdownErrors);

            for (var i = executionPlan.Count - 1; i >= 0; i--)
            {
                var system = executionPlan[i];

                try
                {
                    system.Shutdown(shutdownContext);
                }
                catch (Exception exception)
                {
                    var message =
                        $"Shutdown system '{system.Name}' failed: {exception.Message}";
                    shutdownErrors.Add(message);
                    shutdownContext.GetLoggerOrNull()?.Error("Bootstrap.Shutdown", message, exception);
                }
            }

            var executedCleanupOperations = shutdownContext.ExecuteCleanupOperations(shutdownErrors);
            var releasedRuntimeResources = shutdownContext.ReleaseRuntimeResources(shutdownErrors);
            var destroyedTemporaryObjects = shutdownContext.DestroyTemporaryObjects(shutdownErrors);

            var snapshot = shutdownContext.CreateShutdownSnapshot(
                executedSaveOperations,
                executedCleanupOperations,
                releasedRuntimeResources,
                destroyedTemporaryObjects,
                shutdownErrors);
            shutdownContext.PersistShutdownSnapshot(snapshot, shutdownErrors);
            shutdownContext.DisposeServices(shutdownErrors);
            shutdownContext.ClearShutdownRegistrations();

            executionPlan.Clear();
            context = null;
            isInitialized = false;

            if (shutdownErrors.Count == 0)
            {
                shutdownContext.GetLoggerOrNull()?.Info("Bootstrap.Shutdown", "Application shutdown completed gracefully.");
                return;
            }

            shutdownContext.GetLoggerOrNull()?.Warning(
                "Bootstrap.Shutdown",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Application shutdown completed with {0} issue(s). Review logs for details.",
                    shutdownErrors.Count));
        }

        private static IReadOnlyList<IApplicationSystem> BuildExecutionPlan(
            IReadOnlyList<IApplicationSystem> systems,
            ILogService logger)
        {
            var uniqueSystems = new Dictionary<string, IApplicationSystem>(StringComparer.Ordinal);

            for (var i = 0; i < systems.Count; i++)
            {
                var system = systems[i];
                if (system == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(system.Name))
                {
                    throw new InvalidOperationException("Application system name must be a non-empty value.");
                }

                if (uniqueSystems.ContainsKey(system.Name))
                {
                    logger?.Warning(
                        "Bootstrap.ExecutionPlan",
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Ignoring duplicate application system '{0}'.",
                            system.Name));
                    continue;
                }

                uniqueSystems.Add(system.Name, system);
            }

            var resolved = new List<IApplicationSystem>(uniqueSystems.Count);
            var visitStates = new Dictionary<string, VisitState>(StringComparer.Ordinal);

            var candidates = new List<IApplicationSystem>(uniqueSystems.Values);
            candidates.Sort(ApplicationSystemDiscovery.CompareSystems);

            for (var i = 0; i < candidates.Count; i++)
            {
                VisitSystem(candidates[i], uniqueSystems, visitStates, resolved);
            }

            return resolved;
        }

        private static void VisitSystem(
            IApplicationSystem system,
            IReadOnlyDictionary<string, IApplicationSystem> systems,
            IDictionary<string, VisitState> visitStates,
            IList<IApplicationSystem> resolved)
        {
            if (visitStates.TryGetValue(system.Name, out var state))
            {
                if (state == VisitState.Visited)
                {
                    return;
                }

                if (state == VisitState.Visiting)
                {
                    throw new InvalidOperationException(
                        $"Circular application system dependency detected at '{system.Name}'.");
                }
            }

            visitStates[system.Name] = VisitState.Visiting;

            var dependencies = system.Dependencies ?? Array.Empty<string>();
            for (var i = 0; i < dependencies.Count; i++)
            {
                var dependencyName = dependencies[i];
                if (string.IsNullOrWhiteSpace(dependencyName))
                {
                    continue;
                }

                if (!systems.TryGetValue(dependencyName, out var dependency))
                {
                    throw new InvalidOperationException(
                        $"Application system '{system.Name}' depends on missing system '{dependencyName}'.");
                }

                VisitSystem(dependency, systems, visitStates, resolved);
            }

            visitStates[system.Name] = VisitState.Visited;
            resolved.Add(system);
        }

        private static void RegisterServices(ApplicationBootstrapContext bootstrapContext)
        {
            var providers = ServiceRegistrationDiscovery.DiscoverProviders();
            var services = new ServiceCollection();

            foreach (var provider in providers)
            {
                provider.RegisterServices(bootstrapContext, services);
            }

            bootstrapContext.SetServices(new ServiceContainer(services.Descriptors));

            bootstrapContext.GetLoggerOrNull()?.Info(
                "Bootstrap.Services",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Registered {0} runtime services from {1} providers.",
                    services.Count,
                    providers.Count));
        }
    }

    public sealed class ApplicationBootstrapContext
    {
        private readonly List<string> loadedSystems = new();
        private readonly List<string> loadedGameplayModules = new();
        private readonly HashSet<string> loadedSystemNames = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> runtimeState = new(StringComparer.Ordinal);
        private readonly List<ShutdownActionRegistration> saveOperations = new();
        private readonly List<ShutdownActionRegistration> cleanupOperations = new();
        private readonly List<RuntimeResourceRegistration> runtimeResources = new();
        private readonly List<TemporaryObjectRegistration> temporaryObjects = new();

        private int registrationSequence;

        public ApplicationBootstrapContext(BootstrapManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            Manager = manager;
        }

        public BootstrapManager Manager { get; }

        public IReadOnlyList<string> LoadedSystems
        {
            get { return loadedSystems; }
        }

        public IReadOnlyList<string> LoadedGameplayModules
        {
            get { return loadedGameplayModules; }
        }

        public IServiceResolver Services { get; private set; }

        public InputActionAsset ProjectWideInputActions { get; private set; }

        public bool IsShuttingDown { get; private set; }

        public string ShutdownReason { get; private set; }

        public string StartupScenePath
        {
            get { return SceneUtility.GetScenePathByBuildIndex(0); }
        }

        public string ActiveScenePath
        {
            get { return SceneManager.GetActiveScene().path; }
        }

        public void SetProjectWideInputActions(InputActionAsset inputActions)
        {
            ProjectWideInputActions = inputActions;
        }

        public void RegisterSaveOperation(string name, Action<ApplicationBootstrapContext> saveOperation, int order = 0)
        {
            if (saveOperation == null)
            {
                throw new ArgumentNullException(nameof(saveOperation));
            }

            saveOperations.Add(
                new ShutdownActionRegistration(
                    SanitizeRegistrationName(name, "SaveOperation"),
                    saveOperation,
                    order,
                    registrationSequence++));
        }

        public void RegisterCleanupOperation(string name, Action<ApplicationBootstrapContext> cleanupOperation, int order = 0)
        {
            if (cleanupOperation == null)
            {
                throw new ArgumentNullException(nameof(cleanupOperation));
            }

            cleanupOperations.Add(
                new ShutdownActionRegistration(
                    SanitizeRegistrationName(name, "CleanupOperation"),
                    cleanupOperation,
                    order,
                    registrationSequence++));
        }

        public void RegisterEventSubscription(string name, Action unsubscribeAction, int order = 0)
        {
            if (unsubscribeAction == null)
            {
                throw new ArgumentNullException(nameof(unsubscribeAction));
            }

            RegisterCleanupOperation(
                SanitizeRegistrationName(name, "EventSubscription"),
                _ => unsubscribeAction(),
                order);
        }

        public void RegisterRuntimeResource(string name, IDisposable resource)
        {
            if (resource == null)
            {
                return;
            }

            runtimeResources.Add(
                new RuntimeResourceRegistration(
                    SanitizeRegistrationName(name, "RuntimeResource"),
                    resource,
                    registrationSequence++));
        }

        public void RegisterTemporaryObject(string name, UnityEngine.Object temporaryObject)
        {
            if (temporaryObject == null)
            {
                return;
            }

            temporaryObjects.Add(
                new TemporaryObjectRegistration(
                    SanitizeRegistrationName(name, "TemporaryObject"),
                    temporaryObject,
                    registrationSequence++));
        }

        public void RecordRuntimeState(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Runtime state key must be a non-empty value.", nameof(key));
            }

            runtimeState[key] = value ?? string.Empty;
        }

        internal ILogService GetLoggerOrNull()
        {
            if (Services == null)
            {
                return null;
            }

            return Services.TryResolve<ILogService>(out var logger) ? logger : null;
        }

        internal void SetServices(ServiceContainer serviceContainer)
        {
            Services = serviceContainer ?? throw new ArgumentNullException(nameof(serviceContainer));
        }

        internal void DisposeServices()
        {
            DisposeServices(null);
        }

        internal void DisposeServices(ICollection<string> shutdownErrors)
        {
            if (Services is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception exception)
                {
                    ReportShutdownError("Dispose runtime services", exception, shutdownErrors);
                }
            }

            Services = null;
        }

        internal bool IsSystemLoaded(string systemName)
        {
            return !string.IsNullOrWhiteSpace(systemName) && loadedSystemNames.Contains(systemName);
        }

        internal void MarkSystemLoaded(IApplicationSystem system)
        {
            if (system == null || string.IsNullOrWhiteSpace(system.Name))
            {
                return;
            }

            if (!loadedSystemNames.Add(system.Name))
            {
                return;
            }

            loadedSystems.Add(system.Name);

            if (system.Category == ApplicationSystemCategory.Gameplay)
            {
                loadedGameplayModules.Add(system.Name);
            }
        }

        internal void BeginShutdown(string reason)
        {
            IsShuttingDown = true;
            ShutdownReason = string.IsNullOrWhiteSpace(reason) ? "ApplicationExit" : reason;

            RecordRuntimeState("application.name", Application.productName);
            RecordRuntimeState("application.version", Application.version);
            RecordRuntimeState("shutdown.reason", ShutdownReason);
            RecordRuntimeState("shutdown.timestampUtc", DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            RecordRuntimeState("runtime.loadedSystemCount", loadedSystems.Count.ToString(CultureInfo.InvariantCulture));
            RecordRuntimeState("runtime.loadedGameplayModuleCount", loadedGameplayModules.Count.ToString(CultureInfo.InvariantCulture));
        }

        internal List<string> ExecuteSaveOperations(ICollection<string> shutdownErrors)
        {
            var executedOperations = new List<string>();
            var orderedOperations = new List<ShutdownActionRegistration>(saveOperations);
            orderedOperations.Sort(CompareSaveOperations);

            for (var i = 0; i < orderedOperations.Count; i++)
            {
                var operation = orderedOperations[i];
                if (!TryExecuteShutdownStep(operation.Name, () => operation.Action(this), shutdownErrors))
                {
                    continue;
                }

                executedOperations.Add(operation.Name);
            }

            return executedOperations;
        }

        internal List<string> ExecuteCleanupOperations(ICollection<string> shutdownErrors)
        {
            var executedOperations = new List<string>();
            var orderedOperations = new List<ShutdownActionRegistration>(cleanupOperations);
            orderedOperations.Sort(CompareCleanupOperations);

            for (var i = 0; i < orderedOperations.Count; i++)
            {
                var operation = orderedOperations[i];
                if (!TryExecuteShutdownStep(operation.Name, () => operation.Action(this), shutdownErrors))
                {
                    continue;
                }

                executedOperations.Add(operation.Name);
            }

            return executedOperations;
        }

        internal List<string> ReleaseRuntimeResources(ICollection<string> shutdownErrors)
        {
            var releasedResources = new List<string>();

            for (var i = runtimeResources.Count - 1; i >= 0; i--)
            {
                var resource = runtimeResources[i];
                if (!TryExecuteShutdownStep(resource.Name, resource.Resource.Dispose, shutdownErrors))
                {
                    continue;
                }

                releasedResources.Add(resource.Name);
            }

            return releasedResources;
        }

        internal List<string> DestroyTemporaryObjects(ICollection<string> shutdownErrors)
        {
            var destroyedObjects = new List<string>();

            for (var i = temporaryObjects.Count - 1; i >= 0; i--)
            {
                var temporaryObject = temporaryObjects[i];
                if (temporaryObject.Object == null)
                {
                    continue;
                }

                if (!TryExecuteShutdownStep(
                        temporaryObject.Name,
                        () => DestroyTemporaryObject(temporaryObject.Object),
                        shutdownErrors))
                {
                    continue;
                }

                destroyedObjects.Add(temporaryObject.Name);
            }

            return destroyedObjects;
        }

        internal ApplicationShutdownSnapshot CreateShutdownSnapshot(
            IReadOnlyList<string> saveSteps,
            IReadOnlyList<string> cleanupSteps,
            IReadOnlyList<string> releasedResources,
            IReadOnlyList<string> destroyedObjects,
            IReadOnlyList<string> shutdownErrors)
        {
            var runtimeEntries = new List<ApplicationShutdownDataEntry>(runtimeState.Count);
            foreach (var pair in runtimeState)
            {
                runtimeEntries.Add(new ApplicationShutdownDataEntry
                {
                    key = pair.Key,
                    value = pair.Value
                });
            }

            runtimeEntries.Sort((left, right) => StringComparer.Ordinal.Compare(left.key, right.key));

            return new ApplicationShutdownSnapshot
            {
                applicationName = Application.productName,
                applicationVersion = Application.version,
                shutdownReason = ShutdownReason,
                shutdownUtcTimestamp = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                startupScenePath = StartupScenePath,
                activeScenePath = ActiveScenePath,
                loadedSystems = loadedSystems.ToArray(),
                loadedGameplayModules = loadedGameplayModules.ToArray(),
                runtimeData = runtimeEntries.ToArray(),
                saveOperations = ToArray(saveSteps),
                cleanupOperations = ToArray(cleanupSteps),
                releasedRuntimeResources = ToArray(releasedResources),
                destroyedTemporaryObjects = ToArray(destroyedObjects),
                errors = ToArray(shutdownErrors)
            };
        }

        internal void PersistShutdownSnapshot(ApplicationShutdownSnapshot snapshot, ICollection<string> shutdownErrors)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (Services == null)
            {
                shutdownErrors?.Add("[Worldforge] Cannot persist shutdown snapshot because services are unavailable.");
                return;
            }

            if (!Services.TryResolve<IApplicationShutdownSnapshotStore>(out var snapshotStore) || snapshotStore == null)
            {
                shutdownErrors?.Add("[Worldforge] Shutdown snapshot store is not registered.");
                return;
            }

            TryExecuteShutdownStep(
                "PersistShutdownSnapshot",
                () => snapshotStore.Save(snapshot),
                shutdownErrors);
        }

        internal void ClearShutdownRegistrations()
        {
            saveOperations.Clear();
            cleanupOperations.Clear();
            runtimeResources.Clear();
            temporaryObjects.Clear();
            runtimeState.Clear();
            ProjectWideInputActions = null;
            ShutdownReason = null;
            IsShuttingDown = false;
        }

        private static int CompareSaveOperations(ShutdownActionRegistration left, ShutdownActionRegistration right)
        {
            var orderComparison = left.Order.CompareTo(right.Order);
            if (orderComparison != 0)
            {
                return orderComparison;
            }

            return left.Sequence.CompareTo(right.Sequence);
        }

        private static int CompareCleanupOperations(ShutdownActionRegistration left, ShutdownActionRegistration right)
        {
            var orderComparison = right.Order.CompareTo(left.Order);
            if (orderComparison != 0)
            {
                return orderComparison;
            }

            return right.Sequence.CompareTo(left.Sequence);
        }

        private static string SanitizeRegistrationName(string name, string fallbackName)
        {
            return string.IsNullOrWhiteSpace(name) ? fallbackName : name.Trim();
        }

        private static void DestroyTemporaryObject(UnityEngine.Object temporaryObject)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(temporaryObject);
                return;
            }

            UnityEngine.Object.DestroyImmediate(temporaryObject);
        }

        private static string[] ToArray(IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return Array.Empty<string>();
            }

            var result = new string[values.Count];
            for (var i = 0; i < values.Count; i++)
            {
                result[i] = values[i];
            }

            return result;
        }

        private bool TryExecuteShutdownStep(string name, Action step, ICollection<string> shutdownErrors)
        {
            try
            {
                step();
                return true;
            }
            catch (Exception exception)
            {
                ReportShutdownError(name, exception, shutdownErrors);
                return false;
            }
        }

        private void ReportShutdownError(string name, Exception exception, ICollection<string> shutdownErrors)
        {
            var message = $"Shutdown step '{name}' failed: {exception.Message}";
            shutdownErrors?.Add(message);
            GetLoggerOrNull()?.Error("Bootstrap.Shutdown", message, exception);
        }
    }

    public enum ApplicationSystemCategory
    {
        Core,
        Gameplay
    }

    public interface IApplicationSystem
    {
        string Name { get; }

        int Order { get; }

        ApplicationSystemCategory Category { get; }

        IReadOnlyList<string> Dependencies { get; }

        void Initialize(ApplicationBootstrapContext context);

        void Shutdown(ApplicationBootstrapContext context);
    }

    public interface IApplicationSystemProvider
    {
        int Order { get; }

        IEnumerable<IApplicationSystem> CreateSystems();
    }

    public interface IApplicationShutdownSnapshotStore
    {
        ApplicationShutdownSnapshot LastSavedSnapshot { get; }

        string SnapshotPath { get; }

        void Save(ApplicationShutdownSnapshot snapshot);
    }

    internal sealed class InputBootstrapSystem : IApplicationSystem
    {
        private static readonly IReadOnlyList<string> NoDependencies = Array.Empty<string>();

        public string Name
        {
            get { return "Input"; }
        }

        public int Order
        {
            get { return 0; }
        }

        public ApplicationSystemCategory Category
        {
            get { return ApplicationSystemCategory.Core; }
        }

        public IReadOnlyList<string> Dependencies
        {
            get { return NoDependencies; }
        }

        public void Initialize(ApplicationBootstrapContext context)
        {
            var logger = context.GetLoggerOrNull();
            var actions = InputSystem.actions;
            if (actions == null)
            {
                logger?.Warning("Bootstrap.Input", "Input bootstrap did not find a project-wide Input Action Asset.");
                return;
            }

            actions.Enable();
            context.SetProjectWideInputActions(actions);
            context.RegisterCleanupOperation("Core.Input.DisableActions", _ => actions.Disable());

            logger?.Info(
                "Bootstrap.Input",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Loaded input actions '{0}'.",
                    actions.name));
        }

        public void Shutdown(ApplicationBootstrapContext context)
        {
        }
    }

    internal sealed class SceneBootstrapSystem : IApplicationSystem
    {
        private static readonly IReadOnlyList<string> DependenciesList = new[] { "Input" };
        private ILogService logger;

        public string Name
        {
            get { return "SceneFlow"; }
        }

        public int Order
        {
            get { return 10; }
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
            logger = context.GetLoggerOrNull();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            context.RegisterEventSubscription("Core.SceneFlow.SceneLoaded", () => SceneManager.sceneLoaded -= OnSceneLoaded);

            var startupScenePath = context.StartupScenePath;
            if (string.IsNullOrEmpty(startupScenePath))
            {
                logger?.Warning("Bootstrap.SceneFlow", "No startup scene is configured in Build Settings.");
                return;
            }

            logger?.Info(
                "Bootstrap.SceneFlow",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Startup scene configured as '{0}'.",
                    startupScenePath));
        }

        public void Shutdown(ApplicationBootstrapContext context)
        {
            logger = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            logger?.Info(
                "Bootstrap.SceneFlow",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Scene loaded: '{0}' ({1}).",
                    scene.path,
                    mode));
        }
    }

    internal sealed class GameSessionBootstrapSystem : IApplicationSystem
    {
        private static readonly IReadOnlyList<string> DependenciesList = new[] { "SceneFlow" };

        public string Name
        {
            get { return "GameSession"; }
        }

        public int Order
        {
            get { return 15; }
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
            var gameSessionManager = context.Services.Resolve<IGameSessionManager>();
            var logger = context.GetLoggerOrNull();

            context.RecordRuntimeState("session.activeState", gameSessionManager.State.ToString());
            logger?.Info("Bootstrap.GameSession", "Game session manager is ready.");
        }

        public void Shutdown(ApplicationBootstrapContext context)
        {
            if (context.Services == null || !context.Services.TryResolve<IGameSessionManager>(out var gameSessionManager))
            {
                return;
            }

            gameSessionManager.ShutdownActiveSession("ApplicationShutdown");
        }
    }

    public enum GameSessionState
    {
        Inactive,
        Starting,
        Running,
        ShuttingDown
    }

    public interface IGameSession
    {
        Guid SessionId { get; }

        GameSessionState State { get; }

        IServiceResolver Services { get; }

        IReadOnlyList<string> LoadedSystems { get; }

        bool IsReadyForPlayerSpawn { get; }

        Vector3 PlayerSpawnPosition { get; }

        Quaternion PlayerSpawnRotation { get; }

        string PlayerSpawnSource { get; }
    }

    public interface IGameSessionManager
    {
        bool HasActiveSession { get; }

        GameSessionState State { get; }

        IGameSession CurrentSession { get; }

        IGameSession StartNewGame();

        void ShutdownActiveSession(string reason = "SessionShutdown");
    }

    public interface IGameSessionSystem
    {
        string Name { get; }

        int Order { get; }

        IReadOnlyList<string> Dependencies { get; }

        void Initialize(GameSessionContext context);

        void Shutdown(GameSessionContext context);
    }

    public interface IGameSessionSystemProvider
    {
        int Order { get; }

        IEnumerable<IGameSessionSystem> CreateSystems();
    }

    public sealed class GameSessionContext : IGameSession
    {
        private readonly List<string> loadedSystems = new();
        private readonly HashSet<string> loadedSystemNames = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> runtimeState = new(StringComparer.Ordinal);
        private readonly List<SessionActionRegistration> cleanupOperations = new();
        private readonly List<SessionResourceRegistration> runtimeResources = new();
        private readonly List<SessionTemporaryObjectRegistration> temporaryObjects = new();

        private int registrationSequence;

        internal GameSessionContext(
            ApplicationBootstrapContext applicationContext,
            GameSessionManager manager,
            ServiceScope sessionScope,
            Guid sessionId)
        {
            ApplicationContext = applicationContext ?? throw new ArgumentNullException(nameof(applicationContext));
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
            SessionScope = sessionScope ?? throw new ArgumentNullException(nameof(sessionScope));
            SessionId = sessionId;
            StartedAtUtc = DateTime.UtcNow;
            State = GameSessionState.Starting;
            PlayerSpawnSource = "Unresolved";
        }

        public ApplicationBootstrapContext ApplicationContext { get; }

        public GameSessionManager Manager { get; }

        public Guid SessionId { get; }

        public DateTime StartedAtUtc { get; }

        public GameSessionState State { get; internal set; }

        public IServiceResolver Services
        {
            get { return SessionScope; }
        }

        public IReadOnlyList<string> LoadedSystems
        {
            get { return loadedSystems; }
        }

        public bool IsReadyForPlayerSpawn { get; private set; }

        public Vector3 PlayerSpawnPosition { get; private set; }

        public Quaternion PlayerSpawnRotation { get; private set; }

        public string PlayerSpawnSource { get; private set; }

        internal ServiceScope SessionScope { get; }

        internal IReadOnlyDictionary<string, string> RuntimeState
        {
            get { return runtimeState; }
        }

        public void RegisterCleanupOperation(string name, Action<GameSessionContext> cleanupOperation, int order = 0)
        {
            if (cleanupOperation == null)
            {
                throw new ArgumentNullException(nameof(cleanupOperation));
            }

            cleanupOperations.Add(
                new SessionActionRegistration(
                    SanitizeRegistrationName(name, "CleanupOperation"),
                    cleanupOperation,
                    order,
                    registrationSequence++));
        }

        public void RegisterEventSubscription(string name, Action unsubscribeAction, int order = 0)
        {
            if (unsubscribeAction == null)
            {
                throw new ArgumentNullException(nameof(unsubscribeAction));
            }

            RegisterCleanupOperation(
                SanitizeRegistrationName(name, "EventSubscription"),
                _ => unsubscribeAction(),
                order);
        }

        public void RegisterRuntimeResource(string name, IDisposable resource)
        {
            if (resource == null)
            {
                return;
            }

            runtimeResources.Add(
                new SessionResourceRegistration(
                    SanitizeRegistrationName(name, "RuntimeResource"),
                    resource,
                    registrationSequence++));
        }

        public void RegisterTemporaryObject(string name, UnityEngine.Object temporaryObject)
        {
            if (temporaryObject == null)
            {
                return;
            }

            temporaryObjects.Add(
                new SessionTemporaryObjectRegistration(
                    SanitizeRegistrationName(name, "TemporaryObject"),
                    temporaryObject,
                    registrationSequence++));
        }

        public void RecordRuntimeState(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Runtime state key must be a non-empty value.", nameof(key));
            }

            runtimeState[key] = value ?? string.Empty;
        }

        public void PreparePlayerSpawn(Vector3 spawnPosition, Quaternion spawnRotation, string spawnSource)
        {
            PlayerSpawnPosition = spawnPosition;
            PlayerSpawnRotation = spawnRotation;
            PlayerSpawnSource = string.IsNullOrWhiteSpace(spawnSource) ? "Unknown" : spawnSource.Trim();
            IsReadyForPlayerSpawn = true;

            RecordRuntimeState(
                "playerSpawn.position",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0:F3},{1:F3},{2:F3}",
                    spawnPosition.x,
                    spawnPosition.y,
                    spawnPosition.z));
            RecordRuntimeState(
                "playerSpawn.rotation",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0:F3},{1:F3},{2:F3},{3:F3}",
                    spawnRotation.x,
                    spawnRotation.y,
                    spawnRotation.z,
                    spawnRotation.w));
            RecordRuntimeState("playerSpawn.source", PlayerSpawnSource);
            RecordRuntimeState("playerSpawn.isPrepared", bool.TrueString);
        }

        internal ILogService GetLoggerOrNull()
        {
            return Services.TryResolve<ILogService>(out var logger) ? logger : null;
        }

        internal bool IsSystemLoaded(string systemName)
        {
            return !string.IsNullOrWhiteSpace(systemName) && loadedSystemNames.Contains(systemName);
        }

        internal void MarkSystemLoaded(IGameSessionSystem system)
        {
            if (system == null || string.IsNullOrWhiteSpace(system.Name))
            {
                return;
            }

            if (!loadedSystemNames.Add(system.Name))
            {
                return;
            }

            loadedSystems.Add(system.Name);
        }

        internal List<string> ExecuteCleanupOperations(ICollection<string> shutdownErrors)
        {
            var executedOperations = new List<string>();
            var orderedOperations = new List<SessionActionRegistration>(cleanupOperations);
            orderedOperations.Sort(CompareCleanupOperations);

            for (var i = 0; i < orderedOperations.Count; i++)
            {
                var operation = orderedOperations[i];
                if (!TryExecuteShutdownStep(operation.Name, () => operation.Action(this), shutdownErrors))
                {
                    continue;
                }

                executedOperations.Add(operation.Name);
            }

            return executedOperations;
        }

        internal List<string> ReleaseRuntimeResources(ICollection<string> shutdownErrors)
        {
            var releasedResources = new List<string>();

            for (var i = runtimeResources.Count - 1; i >= 0; i--)
            {
                var resource = runtimeResources[i];
                if (!TryExecuteShutdownStep(resource.Name, resource.Resource.Dispose, shutdownErrors))
                {
                    continue;
                }

                releasedResources.Add(resource.Name);
            }

            return releasedResources;
        }

        internal List<string> DestroyTemporaryObjects(ICollection<string> shutdownErrors)
        {
            var destroyedObjects = new List<string>();

            for (var i = temporaryObjects.Count - 1; i >= 0; i--)
            {
                var temporaryObject = temporaryObjects[i];
                if (temporaryObject.Object == null)
                {
                    continue;
                }

                if (!TryExecuteShutdownStep(
                        temporaryObject.Name,
                        () => DestroyTemporaryObject(temporaryObject.Object),
                        shutdownErrors))
                {
                    continue;
                }

                destroyedObjects.Add(temporaryObject.Name);
            }

            return destroyedObjects;
        }

        internal void ClearRegistrations()
        {
            cleanupOperations.Clear();
            runtimeResources.Clear();
            temporaryObjects.Clear();
            loadedSystems.Clear();
            loadedSystemNames.Clear();
            runtimeState.Clear();
            IsReadyForPlayerSpawn = false;
            PlayerSpawnSource = "Unresolved";
            PlayerSpawnPosition = Vector3.zero;
            PlayerSpawnRotation = Quaternion.identity;
            State = GameSessionState.Inactive;
        }

        internal void DisposeScope()
        {
            SessionScope.Dispose();
        }

        private static int CompareCleanupOperations(SessionActionRegistration left, SessionActionRegistration right)
        {
            var orderComparison = right.Order.CompareTo(left.Order);
            if (orderComparison != 0)
            {
                return orderComparison;
            }

            return right.Sequence.CompareTo(left.Sequence);
        }

        private static string SanitizeRegistrationName(string name, string fallbackName)
        {
            return string.IsNullOrWhiteSpace(name) ? fallbackName : name.Trim();
        }

        private static void DestroyTemporaryObject(UnityEngine.Object temporaryObject)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(temporaryObject);
                return;
            }

            UnityEngine.Object.DestroyImmediate(temporaryObject);
        }

        private bool TryExecuteShutdownStep(string name, Action step, ICollection<string> shutdownErrors)
        {
            try
            {
                step();
                return true;
            }
            catch (Exception exception)
            {
                var message = $"Game session shutdown step '{name}' failed: {exception.Message}";
                shutdownErrors?.Add(message);
                GetLoggerOrNull()?.Error("GameSession.Shutdown", message, exception);
                return false;
            }
        }
    }

    public sealed class GameSessionManager : IGameSessionManager
    {
        private readonly ApplicationBootstrapContext applicationContext;
        private readonly List<IGameSessionSystem> executionPlan = new();

        private GameSessionContext activeSession;

        public GameSessionManager(ApplicationBootstrapContext applicationContext)
        {
            this.applicationContext = applicationContext ?? throw new ArgumentNullException(nameof(applicationContext));
        }

        public bool HasActiveSession
        {
            get { return activeSession != null; }
        }

        public GameSessionState State
        {
            get { return activeSession != null ? activeSession.State : GameSessionState.Inactive; }
        }

        public IGameSession CurrentSession
        {
            get { return activeSession; }
        }

        public IGameSession StartNewGame()
        {
            if (applicationContext.Services == null)
            {
                throw new InvalidOperationException("Application services are not available for game sessions.");
            }

            if (activeSession != null)
            {
                ShutdownActiveSession("StartNewGame.Restart");
            }

            var sessionContext = new GameSessionContext(
                applicationContext,
                this,
                applicationContext.Services.CreateScope(),
                Guid.NewGuid());

            activeSession = sessionContext;
            executionPlan.Clear();

            try
            {
                var logger = sessionContext.GetLoggerOrNull();
                var declaredSystems = GameSessionSystemDiscovery.DiscoverSystems();
                executionPlan.AddRange(BuildExecutionPlan(declaredSystems, logger));

                for (var i = 0; i < executionPlan.Count; i++)
                {
                    var system = executionPlan[i];
                    if (sessionContext.IsSystemLoaded(system.Name))
                    {
                        continue;
                    }

                    system.Initialize(sessionContext);
                    sessionContext.MarkSystemLoaded(system);
                }

                ResolvePlayerSpawn(sessionContext);
                sessionContext.State = GameSessionState.Running;
                RecordSessionStart(sessionContext);

                logger?.Info(
                    "GameSession.Start",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Started game session '{0}' with {1} session system(s).",
                        sessionContext.SessionId,
                        sessionContext.LoadedSystems.Count));

                return sessionContext;
            }
            catch (Exception exception)
            {
                sessionContext.GetLoggerOrNull()?.Error("GameSession.Start", "Failed to start game session.", exception);
                ShutdownSessionInternal(sessionContext, "SessionStartFailed", false);
                throw;
            }
        }

        public void ShutdownActiveSession(string reason = "SessionShutdown")
        {
            if (activeSession == null)
            {
                return;
            }

            ShutdownSessionInternal(activeSession, reason, true);
        }

        private static IReadOnlyList<IGameSessionSystem> BuildExecutionPlan(
            IReadOnlyList<IGameSessionSystem> systems,
            ILogService logger)
        {
            var uniqueSystems = new Dictionary<string, IGameSessionSystem>(StringComparer.Ordinal);

            for (var i = 0; i < systems.Count; i++)
            {
                var system = systems[i];
                if (system == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(system.Name))
                {
                    throw new InvalidOperationException("Game session system name must be a non-empty value.");
                }

                if (uniqueSystems.ContainsKey(system.Name))
                {
                    logger?.Warning(
                        "GameSession.ExecutionPlan",
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Ignoring duplicate game session system '{0}'.",
                            system.Name));
                    continue;
                }

                uniqueSystems.Add(system.Name, system);
            }

            var resolved = new List<IGameSessionSystem>(uniqueSystems.Count);
            var visitStates = new Dictionary<string, VisitState>(StringComparer.Ordinal);

            var candidates = new List<IGameSessionSystem>(uniqueSystems.Values);
            candidates.Sort(GameSessionSystemDiscovery.CompareSystems);

            for (var i = 0; i < candidates.Count; i++)
            {
                VisitSystem(candidates[i], uniqueSystems, visitStates, resolved);
            }

            return resolved;
        }

        private static void VisitSystem(
            IGameSessionSystem system,
            IReadOnlyDictionary<string, IGameSessionSystem> systems,
            IDictionary<string, VisitState> visitStates,
            IList<IGameSessionSystem> resolved)
        {
            if (visitStates.TryGetValue(system.Name, out var state))
            {
                if (state == VisitState.Visited)
                {
                    return;
                }

                if (state == VisitState.Visiting)
                {
                    throw new InvalidOperationException(
                        $"Circular game session system dependency detected at '{system.Name}'.");
                }
            }

            visitStates[system.Name] = VisitState.Visiting;

            var dependencies = system.Dependencies ?? Array.Empty<string>();
            for (var i = 0; i < dependencies.Count; i++)
            {
                var dependencyName = dependencies[i];
                if (string.IsNullOrWhiteSpace(dependencyName))
                {
                    continue;
                }

                if (!systems.TryGetValue(dependencyName, out var dependency))
                {
                    throw new InvalidOperationException(
                        $"Game session system '{system.Name}' depends on missing system '{dependencyName}'.");
                }

                VisitSystem(dependency, systems, visitStates, resolved);
            }

            visitStates[system.Name] = VisitState.Visited;
            resolved.Add(system);
        }

        private void ShutdownSessionInternal(GameSessionContext sessionContext, string reason, bool preserveRecordedState)
        {
            if (sessionContext == null || sessionContext.State == GameSessionState.ShuttingDown)
            {
                return;
            }

            sessionContext.State = GameSessionState.ShuttingDown;

            var shutdownErrors = new List<string>();

            for (var i = executionPlan.Count - 1; i >= 0; i--)
            {
                var system = executionPlan[i];

                try
                {
                    system.Shutdown(sessionContext);
                }
                catch (Exception exception)
                {
                    var message = $"Game session system '{system.Name}' failed during shutdown: {exception.Message}";
                    shutdownErrors.Add(message);
                    sessionContext.GetLoggerOrNull()?.Error("GameSession.Shutdown", message, exception);
                }
            }

            var cleanupOperations = sessionContext.ExecuteCleanupOperations(shutdownErrors);
            var releasedResources = sessionContext.ReleaseRuntimeResources(shutdownErrors);
            var destroyedObjects = sessionContext.DestroyTemporaryObjects(shutdownErrors);

            if (preserveRecordedState)
            {
                RecordSessionShutdown(
                    sessionContext,
                    reason,
                    shutdownErrors.Count,
                    cleanupOperations.Count,
                    releasedResources.Count,
                    destroyedObjects.Count);
            }

            sessionContext.DisposeScope();
            sessionContext.ClearRegistrations();
            executionPlan.Clear();

            if (ReferenceEquals(activeSession, sessionContext))
            {
                activeSession = null;
            }
        }

        private void ResolvePlayerSpawn(GameSessionContext sessionContext)
        {
            var spawnPoints = UnityEngine.Object.FindObjectsByType<GameSessionSpawnPoint>(FindObjectsInactive.Exclude);
            GameSessionSpawnPoint selectedSpawnPoint = null;

            for (var i = 0; i < spawnPoints.Length; i++)
            {
                var candidate = spawnPoints[i];
                if (candidate == null || !candidate.isActiveAndEnabled)
                {
                    continue;
                }

                if (selectedSpawnPoint == null || candidate.Priority > selectedSpawnPoint.Priority)
                {
                    selectedSpawnPoint = candidate;
                }
            }

            if (selectedSpawnPoint != null)
            {
                sessionContext.PreparePlayerSpawn(
                    selectedSpawnPoint.transform.position,
                    selectedSpawnPoint.transform.rotation,
                    selectedSpawnPoint.name);
                return;
            }

            sessionContext.GetLoggerOrNull()?.Warning(
                "GameSession.Spawn",
                "No GameSessionSpawnPoint was found in the active scene. Falling back to world origin.");
            sessionContext.PreparePlayerSpawn(Vector3.zero, Quaternion.identity, "FallbackOrigin");
        }

        private void RecordSessionStart(GameSessionContext sessionContext)
        {
            applicationContext.RecordRuntimeState("session.activeState", sessionContext.State.ToString());
            applicationContext.RecordRuntimeState("session.activeSessionId", sessionContext.SessionId.ToString("D"));
            applicationContext.RecordRuntimeState(
                "session.activeLoadedSystemCount",
                sessionContext.LoadedSystems.Count.ToString(CultureInfo.InvariantCulture));
            applicationContext.RecordRuntimeState(
                "session.activePlayerSpawnPrepared",
                sessionContext.IsReadyForPlayerSpawn.ToString());
        }

        private void RecordSessionShutdown(
            GameSessionContext sessionContext,
            string reason,
            int shutdownErrorCount,
            int cleanupOperationCount,
            int releasedResourceCount,
            int destroyedObjectCount)
        {
            applicationContext.RecordRuntimeState("session.activeState", GameSessionState.Inactive.ToString());
            applicationContext.RecordRuntimeState("session.lastSessionId", sessionContext.SessionId.ToString("D"));
            applicationContext.RecordRuntimeState("session.lastShutdownReason", reason ?? "SessionShutdown");
            applicationContext.RecordRuntimeState(
                "session.lastLoadedSystemCount",
                sessionContext.LoadedSystems.Count.ToString(CultureInfo.InvariantCulture));
            applicationContext.RecordRuntimeState(
                "session.lastShutdownErrorCount",
                shutdownErrorCount.ToString(CultureInfo.InvariantCulture));
            applicationContext.RecordRuntimeState(
                "session.lastCleanupOperationCount",
                cleanupOperationCount.ToString(CultureInfo.InvariantCulture));
            applicationContext.RecordRuntimeState(
                "session.lastReleasedResourceCount",
                releasedResourceCount.ToString(CultureInfo.InvariantCulture));
            applicationContext.RecordRuntimeState(
                "session.lastDestroyedObjectCount",
                destroyedObjectCount.ToString(CultureInfo.InvariantCulture));
            applicationContext.RecordRuntimeState(
                "session.lastPlayerSpawnPrepared",
                sessionContext.IsReadyForPlayerSpawn.ToString());
            applicationContext.RecordRuntimeState(
                "session.lastPlayerSpawnSource",
                sessionContext.PlayerSpawnSource ?? string.Empty);

            foreach (var pair in sessionContext.RuntimeState)
            {
                applicationContext.RecordRuntimeState($"session.{pair.Key}", pair.Value);
            }
        }
    }

    public sealed class GameSessionSpawnPoint : MonoBehaviour
    {
        [SerializeField] private int priority;

        public int Priority
        {
            get { return priority; }
        }
    }

    internal static class GameSessionSystemDiscovery
    {
        public static IReadOnlyList<IGameSessionSystem> DiscoverSystems()
        {
            var providers = DiscoverProviders();
            var systems = new List<IGameSessionSystem>();

            for (var i = 0; i < providers.Count; i++)
            {
                var createdSystems = providers[i].CreateSystems();
                if (createdSystems == null)
                {
                    continue;
                }

                foreach (var system in createdSystems)
                {
                    if (system != null)
                    {
                        systems.Add(system);
                    }
                }
            }

            systems.Sort(CompareSystems);
            return systems;
        }

        internal static int CompareSystems(IGameSessionSystem left, IGameSessionSystem right)
        {
            var orderComparison = left.Order.CompareTo(right.Order);
            if (orderComparison != 0)
            {
                return orderComparison;
            }

            return StringComparer.Ordinal.Compare(left.Name, right.Name);
        }

        private static IReadOnlyList<IGameSessionSystemProvider> DiscoverProviders()
        {
            var providers = new List<IGameSessionSystemProvider>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (var i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                if (assembly == null || assembly.IsDynamic)
                {
                    continue;
                }

                var types = GetLoadableTypes(assembly);
                for (var j = 0; j < types.Length; j++)
                {
                    var type = types[j];
                    if (type == null ||
                        type.IsAbstract ||
                        type.IsInterface ||
                        type.ContainsGenericParameters ||
                        !typeof(IGameSessionSystemProvider).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    if (type.GetConstructor(
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                            null,
                            Type.EmptyTypes,
                            null) == null)
                    {
                        continue;
                    }

                    if (Activator.CreateInstance(type, true) is IGameSessionSystemProvider provider)
                    {
                        providers.Add(provider);
                    }
                }
            }

            providers.Sort(CompareProviders);
            return providers;
        }

        private static int CompareProviders(IGameSessionSystemProvider left, IGameSessionSystemProvider right)
        {
            var orderComparison = left.Order.CompareTo(right.Order);
            if (orderComparison != 0)
            {
                return orderComparison;
            }

            var leftName = left.GetType().FullName ?? left.GetType().Name;
            var rightName = right.GetType().FullName ?? right.GetType().Name;
            return StringComparer.Ordinal.Compare(leftName, rightName);
        }

        private static Type[] GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return Array.FindAll(exception.Types, type => type != null);
            }
        }
    }

    internal sealed class SessionActionRegistration
    {
        public SessionActionRegistration(string name, Action<GameSessionContext> action, int order, int sequence)
        {
            Name = name;
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Order = order;
            Sequence = sequence;
        }

        public string Name { get; }

        public Action<GameSessionContext> Action { get; }

        public int Order { get; }

        public int Sequence { get; }
    }

    internal sealed class SessionResourceRegistration
    {
        public SessionResourceRegistration(string name, IDisposable resource, int sequence)
        {
            Name = name;
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            Sequence = sequence;
        }

        public string Name { get; }

        public IDisposable Resource { get; }

        public int Sequence { get; }
    }

    internal sealed class SessionTemporaryObjectRegistration
    {
        public SessionTemporaryObjectRegistration(string name, UnityEngine.Object temporaryObject, int sequence)
        {
            if (temporaryObject == null)
            {
                throw new ArgumentNullException(nameof(temporaryObject));
            }

            Name = name;
            Object = temporaryObject;
            Sequence = sequence;
        }

        public string Name { get; }

        public UnityEngine.Object Object { get; }

        public int Sequence { get; }
    }

    internal static class ApplicationSystemDiscovery
    {
        public static IReadOnlyList<IApplicationSystem> DiscoverSystems()
        {
            var providers = DiscoverProviders();
            var systems = new List<IApplicationSystem>();

            for (var i = 0; i < providers.Count; i++)
            {
                var createdSystems = providers[i].CreateSystems();
                if (createdSystems == null)
                {
                    continue;
                }

                foreach (var system in createdSystems)
                {
                    if (system != null)
                    {
                        systems.Add(system);
                    }
                }
            }

            systems.Sort(CompareSystems);
            return systems;
        }

        private static IReadOnlyList<IApplicationSystemProvider> DiscoverProviders()
        {
            var providers = new List<IApplicationSystemProvider>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (var i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                if (assembly == null || assembly.IsDynamic)
                {
                    continue;
                }

                var types = GetLoadableTypes(assembly);
                for (var j = 0; j < types.Length; j++)
                {
                    var type = types[j];
                    if (type == null ||
                        type.IsAbstract ||
                        type.IsInterface ||
                        type.ContainsGenericParameters ||
                        !typeof(IApplicationSystemProvider).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    if (type.GetConstructor(
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                            null,
                            Type.EmptyTypes,
                            null) == null)
                    {
                        continue;
                    }

                    if (Activator.CreateInstance(type, true) is IApplicationSystemProvider provider)
                    {
                        providers.Add(provider);
                    }
                }
            }

            providers.Sort(CompareProviders);
            return providers;
        }

        private static int CompareProviders(IApplicationSystemProvider left, IApplicationSystemProvider right)
        {
            var orderComparison = left.Order.CompareTo(right.Order);
            if (orderComparison != 0)
            {
                return orderComparison;
            }

            var leftName = left.GetType().FullName ?? left.GetType().Name;
            var rightName = right.GetType().FullName ?? right.GetType().Name;
            return StringComparer.Ordinal.Compare(leftName, rightName);
        }

        internal static int CompareSystems(IApplicationSystem left, IApplicationSystem right)
        {
            var orderComparison = left.Order.CompareTo(right.Order);
            if (orderComparison != 0)
            {
                return orderComparison;
            }

            return StringComparer.Ordinal.Compare(left.Name, right.Name);
        }

        private static Type[] GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return Array.FindAll(exception.Types, type => type != null);
            }
        }
    }

    internal enum VisitState
    {
        Visiting,
        Visited
    }

    internal sealed class ShutdownActionRegistration
    {
        public ShutdownActionRegistration(string name, Action<ApplicationBootstrapContext> action, int order, int sequence)
        {
            Name = name;
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Order = order;
            Sequence = sequence;
        }

        public string Name { get; }

        public Action<ApplicationBootstrapContext> Action { get; }

        public int Order { get; }

        public int Sequence { get; }
    }

    internal sealed class RuntimeResourceRegistration
    {
        public RuntimeResourceRegistration(string name, IDisposable resource, int sequence)
        {
            Name = name;
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            Sequence = sequence;
        }

        public string Name { get; }

        public IDisposable Resource { get; }

        public int Sequence { get; }
    }

    internal sealed class TemporaryObjectRegistration
    {
        public TemporaryObjectRegistration(string name, UnityEngine.Object temporaryObject, int sequence)
        {
            if (temporaryObject == null)
            {
                throw new ArgumentNullException(nameof(temporaryObject));
            }

            Name = name;
            Object = temporaryObject;
            Sequence = sequence;
        }

        public string Name { get; }

        public UnityEngine.Object Object { get; }

        public int Sequence { get; }
    }
}
