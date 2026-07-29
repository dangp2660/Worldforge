using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Worldforge.Character.Spawning;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Infrastructure.Cameras
{
    public sealed class CameraRuntimeServiceRegistrationProvider : IServiceRegistrationProvider
    {
        public int Order
        {
            get { return 130; }
        }

        public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
        {
            services.AddSingleton<ICameraRuntimeService>(resolver =>
            {
                var playerSpawnService = resolver.Resolve<IPlayerSpawnService>();
                var logger = resolver.TryResolve<ILogService>(out var resolvedLogger) ? resolvedLogger : null;
                return new RuntimeCameraService(playerSpawnService, logger);
            });
        }
    }

    internal sealed class CameraRuntimeSystemProvider : IApplicationSystemProvider
    {
        public int Order
        {
            get { return 130; }
        }

        public IEnumerable<IApplicationSystem> CreateSystems()
        {
            yield return new CameraRuntimeBootstrapSystem();
        }
    }

    internal sealed class CameraRuntimeBootstrapSystem : IApplicationSystem
    {
        private static readonly IReadOnlyList<string> DependenciesList =
            new[] { "Gameplay.PlayerSpawn" };

        private ICameraRuntimeService cameraRuntimeService;
        private ILogService logger;

        public string Name
        {
            get { return "Infrastructure.Camera"; }
        }

        public int Order
        {
            get { return 130; }
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
            cameraRuntimeService = context.Services.Resolve<ICameraRuntimeService>();
            logger = context.Services.TryResolve<ILogService>(out var resolvedLogger) ? resolvedLogger : null;

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            context.RegisterEventSubscription(
                "Infrastructure.Camera.SceneLoaded",
                () => SceneManager.sceneLoaded -= OnSceneLoaded,
                130);
            context.RegisterEventSubscription(
                "Infrastructure.Camera.ActiveSceneChanged",
                () => SceneManager.activeSceneChanged -= OnActiveSceneChanged,
                130);
            context.RegisterSaveOperation("Infrastructure.Camera.State", RecordRuntimeState, 130);

            cameraRuntimeService.PrepareForScene(SceneManager.GetActiveScene());
            RecordRuntimeState(context);

            logger?.Info("Infrastructure.Camera", "Runtime camera system is ready.");
        }

        public void Shutdown(ApplicationBootstrapContext context)
        {
            RecordRuntimeState(context);
            cameraRuntimeService?.ClearTarget();
            cameraRuntimeService = null;
            logger = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            cameraRuntimeService?.PrepareForScene(scene);

            logger?.Info(
                "Infrastructure.Camera",
                $"Prepared runtime camera for scene '{scene.path}' ({mode}).");
        }

        private void OnActiveSceneChanged(Scene previousScene, Scene nextScene)
        {
            cameraRuntimeService?.PrepareForScene(nextScene);
        }

        private void RecordRuntimeState(ApplicationBootstrapContext context)
        {
            if (cameraRuntimeService == null)
            {
                return;
            }

            context.RecordRuntimeState(
                "camera.isPrepared",
                cameraRuntimeService.IsPrepared.ToString());
            context.RecordRuntimeState(
                "camera.activeCameraName",
                cameraRuntimeService.ActiveCamera != null ? cameraRuntimeService.ActiveCamera.name : string.Empty);
            context.RecordRuntimeState(
                "camera.followTargetName",
                cameraRuntimeService.FollowTarget != null ? cameraRuntimeService.FollowTarget.name : string.Empty);
        }
    }

    internal sealed class RuntimeCameraService : ICameraRuntimeService, IDisposable
    {
        private readonly IPlayerSpawnService playerSpawnService;
        private readonly ILogService logger;

        private Camera activeCamera;
        private RuntimeCameraController controller;
        private GameObject ownedCameraObject;

        public RuntimeCameraService(IPlayerSpawnService playerSpawnService, ILogService logger)
        {
            this.playerSpawnService = playerSpawnService ?? throw new ArgumentNullException(nameof(playerSpawnService));
            this.logger = logger;
        }

        public Camera ActiveCamera
        {
            get { return activeCamera; }
        }

        public Transform FollowTarget
        {
            get { return controller != null ? controller.FollowTarget : null; }
        }

        public bool IsPrepared
        {
            get { return activeCamera != null && controller != null; }
        }

        public void PrepareForScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            ReleaseOwnedCameraIfNeeded(scene);

            var sceneCamera = FindSceneCamera(scene);
            if (sceneCamera == null)
            {
                sceneCamera = CreateFallbackCamera(scene);
            }

            activeCamera = sceneCamera;
            controller = EnsureController(sceneCamera);
            controller.SetTargetProvider(ResolveFollowTarget);
        }

        public void BindToTarget(Transform target)
        {
            controller?.SetFollowTarget(target);
        }

        public void ClearTarget()
        {
            controller?.ClearFollowTarget();
        }

        public void Dispose()
        {
            ClearTarget();

            if (ownedCameraObject != null)
            {
                DestroyObject(ownedCameraObject);
            }

            ownedCameraObject = null;
            activeCamera = null;
            controller = null;
        }

        private Transform ResolveFollowTarget()
        {
            return playerSpawnService.ActivePlayer != null
                ? playerSpawnService.ActivePlayer.transform
                : null;
        }

        private void ReleaseOwnedCameraIfNeeded(Scene targetScene)
        {
            if (ownedCameraObject == null)
            {
                return;
            }

            if (ownedCameraObject.scene == targetScene)
            {
                return;
            }

            DestroyObject(ownedCameraObject);
            ownedCameraObject = null;
            activeCamera = null;
            controller = null;
        }

        private static Camera FindSceneCamera(Scene scene)
        {
            var rootObjects = scene.GetRootGameObjects();
            Camera fallbackCamera = null;

            for (var i = 0; i < rootObjects.Length; i++)
            {
                var cameras = rootObjects[i].GetComponentsInChildren<Camera>(true);
                for (var j = 0; j < cameras.Length; j++)
                {
                    var camera = cameras[j];
                    if (camera == null)
                    {
                        continue;
                    }

                    if (camera.CompareTag("MainCamera"))
                    {
                        return camera;
                    }

                    fallbackCamera ??= camera;
                }
            }

            return fallbackCamera;
        }

        private Camera CreateFallbackCamera(Scene scene)
        {
            ownedCameraObject = new GameObject("Worldforge.RuntimeCamera");
            SceneManager.MoveGameObjectToScene(ownedCameraObject, scene);

            activeCamera = ownedCameraObject.AddComponent<Camera>();
            ownedCameraObject.AddComponent<AudioListener>();
            ownedCameraObject.tag = "MainCamera";

            activeCamera.nearClipPlane = 0.03f;
            activeCamera.farClipPlane = 1000f;
            activeCamera.fieldOfView = 60f;

            logger?.Warning(
                "Infrastructure.Camera",
                $"Scene '{scene.path}' does not define a camera. Created a runtime fallback camera.");

            return activeCamera;
        }

        private static RuntimeCameraController EnsureController(Camera sceneCamera)
        {
            var runtimeController = sceneCamera.GetComponent<RuntimeCameraController>();
            if (runtimeController == null)
            {
                runtimeController = sceneCamera.gameObject.AddComponent<RuntimeCameraController>();
            }

            return runtimeController;
        }

        private static void DestroyObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
                return;
            }

            UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
