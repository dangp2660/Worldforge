using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Worldforge.Core.Bootstrap
{
    public sealed class BootstrapManager : MonoBehaviour
    {
        private ApplicationStartupFlow startupFlow;
        private bool isInitialized;

        public static BootstrapManager Instance { get; private set; }

        public static bool HasInstance
        {
            get { return Instance != null; }
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
            startupFlow.Initialize(new ApplicationBootstrapContext(this));
            isInitialized = true;
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
            isInitialized = false;
        }
    }

    public sealed class ApplicationStartupFlow
    {
        private readonly List<IApplicationSystem> systems;

        private ApplicationBootstrapContext context;
        private bool isInitialized;

        public ApplicationStartupFlow(params IApplicationSystem[] systems)
        {
            this.systems = new List<IApplicationSystem>(systems ?? Array.Empty<IApplicationSystem>());
        }

        public IReadOnlyList<IApplicationSystem> Systems
        {
            get { return systems; }
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

            foreach (var system in systems)
            {
                system.Initialize(context);
                context.MarkSystemLoaded(system.Name);
            }

            isInitialized = true;

            Debug.LogFormat(
                "[Worldforge] Bootstrap complete. Loaded {0} core systems.",
                context.LoadedSystems.Count);
        }

        public void Shutdown()
        {
            if (!isInitialized || context == null)
            {
                return;
            }

            for (var i = systems.Count - 1; i >= 0; i--)
            {
                systems[i].Shutdown(context);
            }

            context = null;
            isInitialized = false;
        }
    }

    public sealed class ApplicationBootstrapContext
    {
        private readonly List<string> loadedSystems = new List<string>();

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

        internal void MarkSystemLoaded(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
            {
                return;
            }

            loadedSystems.Add(systemName);
        }
    }

    public interface IApplicationSystem
    {
        string Name { get; }

        void Initialize(ApplicationBootstrapContext context);

        void Shutdown(ApplicationBootstrapContext context);
    }

    internal sealed class InputBootstrapSystem : IApplicationSystem
    {
        public string Name
        {
            get { return "Input"; }
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
        public string Name
        {
            get { return "SceneFlow"; }
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
}
