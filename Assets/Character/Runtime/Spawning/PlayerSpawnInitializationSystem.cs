using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Worldforge.Character.Player;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Character.Spawning
{
    internal sealed class PlayerSpawnInitializationSystem : IApplicationSystem
    {
        private static readonly IReadOnlyList<string> DependenciesList =
            new[] { "SceneFlow" };

        private const string LocalPlayerId = "local-player";

        private IPlayerSpawnService spawnService;
        private ILogService logger;

        public string Name
        {
            get { return "Gameplay.PlayerSpawn"; }
        }

        public int Order
        {
            get { return 120; }
        }

        public ApplicationSystemCategory Category
        {
            get { return ApplicationSystemCategory.Gameplay; }
        }

        public IReadOnlyList<string> Dependencies
        {
            get { return DependenciesList; }
        }

        public void Initialize(ApplicationBootstrapContext context)
        {
            spawnService = context.Services.Resolve<IPlayerSpawnService>();

            if (context.Services.TryResolve<ILogService>(out var resolvedLogger))
            {
                logger = resolvedLogger;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            context.RegisterEventSubscription(
                "Gameplay.PlayerSpawn.SceneLoaded",
                () => SceneManager.sceneLoaded -= OnSceneLoaded,
                120);
        }

        public void Shutdown(ApplicationBootstrapContext context)
        {
            spawnService?.DespawnActivePlayer();

            spawnService = null;
            logger = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (spawnService == null || spawnService.HasActivePlayer)
            {
                return;
            }

            var spawnPoint = FindDefaultSpawnPoint(scene);
            if (spawnPoint == null)
            {
                logger?.Warning(
                    "Gameplay.PlayerSpawn",
                    $"No default player spawn point was found in scene '{scene.path}'.");

                return;
            }

            try
            {
                var request = new PlayerSpawnRequest(
                    LocalPlayerId,
                    spawnPoint.CreateLocation());

                var playerObject = spawnService.Spawn(request);
                var playerAvatar = playerObject.GetComponent<PlayerAvatar>();

                if (playerAvatar == null)
                {
                    throw new InvalidOperationException(
                        "Spawned player object does not contain a PlayerAvatar component.");
                }

                playerAvatar.Initialize(request.PlayerId);

                logger?.Info(
                    "Gameplay.PlayerSpawn",
                    $"Spawned player '{request.PlayerId}' at spawn point '{request.SpawnLocation.SpawnId}'.");
            }
            catch (Exception exception)
            {
                spawnService.DespawnActivePlayer();

                logger?.Error(
                    "Gameplay.PlayerSpawn",
                    $"Failed to spawn player in scene '{scene.path}'.",
                    exception);

                throw;
            }
        }

        private PlayerSpawnPoint FindDefaultSpawnPoint(Scene scene)
        {
            var spawnPoints = UnityEngine.Object.FindObjectsByType<PlayerSpawnPoint>(
                FindObjectsInactive.Exclude);

            PlayerSpawnPoint defaultSpawnPoint = null;

            for (var i = 0; i < spawnPoints.Length; i++)
            {
                var spawnPoint = spawnPoints[i];

                if (spawnPoint == null ||
                    spawnPoint.gameObject.scene.handle != scene.handle ||
                    !spawnPoint.IsDefault)
                {
                    continue;
                }

                if (defaultSpawnPoint != null)
                {
                    logger?.Warning(
                        "Gameplay.PlayerSpawn",
                        $"Multiple default player spawn points found in scene '{scene.path}'. " +
                        $"Using '{defaultSpawnPoint.SpawnId}'.");

                    continue;
                }

                defaultSpawnPoint = spawnPoint;
            }

            return defaultSpawnPoint;
        }
    }
}
