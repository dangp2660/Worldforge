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
        private const string ConfigurationResourcePath = "CameraFollowConfiguration";

        public int Order
        {
            get { return 130; }
        }

        public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
        {
            var followConfiguration = Resources.Load<CameraFollowConfiguration>(ConfigurationResourcePath);

            services.AddSingleton<ICameraRuntimeService>(resolver =>
            {
                var playerSpawnService = resolver.Resolve<IPlayerSpawnService>();
                var logger = resolver.TryResolve<ILogService>(out var resolvedLogger) ? resolvedLogger : null;

                if (followConfiguration == null)
                {
                    logger?.Warning(
                        "Infrastructure.Camera",
                        $"Camera follow configuration was not found at Resources/{ConfigurationResourcePath}. Using default values.");
                }

                return new RuntimeCameraService(playerSpawnService, followConfiguration, logger);
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
        private readonly IPlayerSpawnService _playerSpawnService;
        private readonly ILogService _logger;

        private CameraFollowConfiguration _followConfiguration;
        private Camera _activeCamera;
        private RuntimeCameraController _controller;
        private GameObject _ownedCameraObject;

        public RuntimeCameraService(
            IPlayerSpawnService playerSpawnService,
            CameraFollowConfiguration followConfiguration,
            ILogService logger)
        {
            _playerSpawnService = playerSpawnService ?? throw new ArgumentNullException(nameof(playerSpawnService));
            _followConfiguration = followConfiguration;
            _logger = logger;

            _playerSpawnService.PlayerSpawned += OnPlayerSpawned;
            _playerSpawnService.PlayerDespawning += OnPlayerDespawning;

            if (_playerSpawnService.HasActivePlayer)
            {
                OnPlayerSpawned(_playerSpawnService.ActivePlayer);
            }
        }

        public Camera ActiveCamera
        {
            get { return _activeCamera; }
        }

        public Transform FollowTarget
        {
            get { return _controller != null ? _controller.FollowTarget : null; }
        }

        public Transform SecondaryTarget
        {
            get { return _controller != null ? _controller.SecondaryTarget : null; }
        }

        public CameraMode CurrentMode
        {
            get { return _controller != null ? _controller.CurrentMode : CameraMode.ThirdPersonFollow; }
        }

        public Vector3 TargetOffset
        {
            get { return _controller != null ? _controller.TargetOffset : Vector3.zero; }
        }

        public Vector3 CameraForward
        {
            get { return _controller != null ? _controller.CameraForward : Vector3.forward; }
        }

        public Vector3 CameraRight
        {
            get { return _controller != null ? _controller.CameraRight : Vector3.right; }
        }

        public bool IsPrepared
        {
            get { return _activeCamera != null && _controller != null; }
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

            _activeCamera = sceneCamera;
            _controller = EnsureController(sceneCamera);
            _controller.SetLogger(_logger);
            _controller.ApplyConfiguration(_followConfiguration);
            _controller.SetTargetProvider(ResolveFollowTarget);

            if (_playerSpawnService.HasActivePlayer)
            {
                BindToTarget(_playerSpawnService.ActivePlayer.transform);
            }
        }

        public void BindToTarget(Transform target)
        {
            _controller?.SetFollowTarget(target);
        }

        public void ClearTarget()
        {
            _controller?.ClearFollowTarget();
        }

        public void SetMode(CameraMode mode, Transform secondaryTarget = null)
        {
            _controller?.SetMode(mode, secondaryTarget);
        }

        public void SetTargetOffset(Vector3 offset)
        {
            _controller?.SetTargetOffset(offset);
        }

        public void ResetTargetOffset()
        {
            _controller?.ResetTargetOffset();
        }

        public void AddImpulse(Vector3 impulse, float duration)
        {
            _controller?.AddImpulse(impulse, duration);
        }

        public void AddShake(float intensity, float duration)
        {
            _controller?.AddShake(intensity, duration);
        }

        public void SetFieldOfView(float fieldOfView)
        {
            _controller?.SetFieldOfView(fieldOfView);
        }

        public void ResetFieldOfView()
        {
            _controller?.ResetFieldOfView();
        }

        public Vector3 GetCameraRelativeDirection(Vector2 inputDirection)
        {
            return _controller != null
                ? _controller.GetCameraRelativeDirection(inputDirection)
                : Vector3.zero;
        }

        public void ApplyConfiguration(CameraFollowConfiguration configuration)
        {
            if (configuration == null)
            {
                return;
            }

            _followConfiguration = configuration;
            _controller?.ApplyConfiguration(_followConfiguration);
        }

        public void SetOrbitAngles(float pitch, float yaw)
        {
            _controller?.SetOrbitAngles(pitch, yaw);
        }

        public void SetZoomDistance(float distance)
        {
            _controller?.SetZoomDistance(distance);
        }

        public void ResetPose()
        {
            _controller?.ResetPose();
        }

        public void SetCursorLock(bool isLocked)
        {
            RuntimeCameraController.SetCursorLock(isLocked);
        }

        public void Dispose()
        {
            if (_playerSpawnService != null)
            {
                _playerSpawnService.PlayerSpawned -= OnPlayerSpawned;
                _playerSpawnService.PlayerDespawning -= OnPlayerDespawning;
            }

            ClearTarget();

            if (_ownedCameraObject != null)
            {
                DestroyObject(_ownedCameraObject);
            }

            _ownedCameraObject = null;
            _activeCamera = null;
            _controller = null;
        }

        private void OnPlayerSpawned(GameObject player)
        {
            if (player == null)
            {
                return;
            }

            BindToTarget(player.transform);

            _logger?.Info(
                "Infrastructure.Camera",
                $"Camera system bound target to newly spawned player '{player.name}'.");
        }

        private void OnPlayerDespawning(GameObject player)
        {
            ClearTarget();

            _logger?.Info(
                "Infrastructure.Camera",
                "Camera system cleared target due to player despawning.");
        }

        private Transform ResolveFollowTarget()
        {
            return _playerSpawnService.ActivePlayer != null
                ? _playerSpawnService.ActivePlayer.transform
                : null;
        }

        private void ReleaseOwnedCameraIfNeeded(Scene targetScene)
        {
            if (_ownedCameraObject == null)
            {
                return;
            }

            if (_ownedCameraObject.scene == targetScene)
            {
                return;
            }

            DestroyObject(_ownedCameraObject);
            _ownedCameraObject = null;
            _activeCamera = null;
            _controller = null;
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
            _ownedCameraObject = new GameObject("Worldforge.RuntimeCamera");
            SceneManager.MoveGameObjectToScene(_ownedCameraObject, scene);

            _activeCamera = _ownedCameraObject.AddComponent<Camera>();
            _ownedCameraObject.AddComponent<AudioListener>();
            _ownedCameraObject.tag = "MainCamera";

            _activeCamera.nearClipPlane = 0.03f;
            _activeCamera.farClipPlane = 1000f;
            _activeCamera.fieldOfView = 60f;

            _logger?.Warning(
                "Infrastructure.Camera",
                $"Scene '{scene.path}' does not define a camera. Created a runtime fallback camera.");

            return _activeCamera;
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
