using System;
using UnityEngine;
using Worldforge.Character.Player;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Character.Spawning
{
    public sealed class PlayerSpawnServiceRegistrationProvider : IServiceRegistrationProvider
    {
        private const string ConfigurationResourcePath = "PlayerSpawnConfiguration";
        public int Order { get { return 120; } }

        public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
        {
            var configuration = Resources.Load<PlayerSpawnConfiguration>(ConfigurationResourcePath);
            if (configuration == null)
            {
                throw new InvalidOperationException(
                    $"Player spawn configuration was not found at Resources/{ConfigurationResourcePath}.");
            }

            var playerFactory = new RuntimePlayerFactory();
            var playerPrefab = configuration.PlayerPrefab;

            services.AddSingleton<IPlayerSpawnService>(resolver =>
            {
                if (playerPrefab == null &&
                    resolver.TryResolve<ILogService>(out var logger) &&
                    logger != null)
                {
                    logger.Warning(
                        "Gameplay.PlayerSpawn",
                        "Player spawn configuration has no prefab. A development capsule will be created instead.");
                }

                return new RuntimePlayerSpawnService(
                    () => playerFactory.CreatePlayer(playerPrefab),
                    configuration.DefaultSpawnId);
            });
        }
    }
}
