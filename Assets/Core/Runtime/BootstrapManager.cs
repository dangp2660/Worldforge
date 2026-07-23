using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Worldforge.Core.Bootstrap
{
    public sealed class BootstrapManager : MonoBehaviour
    {
        private ApplicationStartupFlow startupFlow;
        private ApplicationBootstrapContext bootstrapContext;
        private bool isInitialized;

        public static BootstrapManager Instance { get; private set; }

        public static bool HasInstance
        {
            get { return Instance != null; }
        }

        public IServiceResolver Services
        {
            get { return bootstrapContext != null ? bootstrapContext.Services : null; }
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
            Shutdown();
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Shutdown();
            ResetInstance();
        }

        public void Initialize(ApplicationStartupFlow flow)
        {
            if (isInitialized)
            {
                return;
            }

            if (flow == null)
            {
                throw new ArgumentNullException(nameof(flow));
            }

            startupFlow = flow;
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

        private void Shutdown()
        {
            if (!isInitialized || startupFlow == null)
            {
                return;
            }

            startupFlow.Shutdown();
            startupFlow = null;
            bootstrapContext = null;
            isInitialized = false;
        }
    }

    public sealed class ApplicationStartupFlow
    {
        private readonly List<IApplicationSystem> declaredSystems;
        private readonly List<IApplicationSystem> executionPlan = new List<IApplicationSystem>();

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
                new SceneBootstrapSystem()
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
            get { return context != null ? context.Services : null; }
        }

        public void Initialize(ApplicationBootstrapContext bootstrapContext)
        {
            if (isInitialized)
            {
                return;
            }

            if (bootstrapContext == null)
            {
                throw new ArgumentNullException(nameof(bootstrapContext));
            }

            context = bootstrapContext;
            try
            {
                RegisterServices(context);
                executionPlan.Clear();
                executionPlan.AddRange(BuildExecutionPlan(declaredSystems));

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
            catch
            {
                context.DisposeServices();
                executionPlan.Clear();
                context = null;
                throw;
            }

            isInitialized = true;

            Debug.LogFormat(
                "[Worldforge] Bootstrap complete. Loaded {0} application systems.",
                context.LoadedSystems.Count);
        }

        public void Shutdown()
        {
            if (!isInitialized || context == null)
            {
                return;
            }

            for (var i = executionPlan.Count - 1; i >= 0; i--)
            {
                executionPlan[i].Shutdown(context);
            }

            context.DisposeServices();
            executionPlan.Clear();
            context = null;
            isInitialized = false;
        }

        private static IReadOnlyList<IApplicationSystem> BuildExecutionPlan(IReadOnlyList<IApplicationSystem> systems)
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
                    Debug.LogWarningFormat(
                        "[Worldforge] Ignoring duplicate application system '{0}'.",
                        system.Name);
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

            Debug.LogFormat(
                "[Worldforge] Registered {0} runtime services from {1} providers.",
                services.Count,
                providers.Count);
        }
    }

    public sealed class ApplicationBootstrapContext
    {
        private readonly List<string> loadedSystems = new List<string>();
        private readonly List<string> loadedGameplayModules = new List<string>();
        private readonly HashSet<string> loadedSystemNames = new HashSet<string>(StringComparer.Ordinal);

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

        internal void SetServices(ServiceContainer serviceContainer)
        {
            Services = serviceContainer ?? throw new ArgumentNullException(nameof(serviceContainer));
        }

        internal void DisposeServices()
        {
            if (Services is IDisposable disposable)
            {
                disposable.Dispose();
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
            var actions = InputSystem.actions;
            if (actions == null)
            {
                Debug.LogWarning("[Worldforge] Input bootstrap did not find a project-wide Input Action Asset.");
                return;
            }

            actions.Enable();
            context.SetProjectWideInputActions(actions);

            Debug.LogFormat("[Worldforge] Loaded input actions '{0}'.", actions.name);
        }

        public void Shutdown(ApplicationBootstrapContext context)
        {
            if (context.ProjectWideInputActions == null)
            {
                return;
            }

            context.ProjectWideInputActions.Disable();
        }
    }

    internal sealed class SceneBootstrapSystem : IApplicationSystem
    {
        private static readonly IReadOnlyList<string> DependenciesList = new[] { "Input" };

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
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            var startupScenePath = context.StartupScenePath;
            if (string.IsNullOrEmpty(startupScenePath))
            {
                Debug.LogWarning("[Worldforge] No startup scene is configured in Build Settings.");
                return;
            }

            Debug.LogFormat("[Worldforge] Startup scene configured as '{0}'.", startupScenePath);
        }

        public void Shutdown(ApplicationBootstrapContext context)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.LogFormat("[Worldforge] Scene loaded: '{0}' ({1}).", scene.path, mode);
        }
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
}
