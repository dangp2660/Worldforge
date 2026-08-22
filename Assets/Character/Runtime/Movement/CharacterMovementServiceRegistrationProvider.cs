using System;
using UnityEngine;
using Worldforge.Character.Traversal;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Character.Movement
{
    public sealed class CharacterMovementServiceRegistrationProvider : IServiceRegistrationProvider
    {
        private const string ConfigurationResourcePath = "CharacterMovementConfiguration";
        private const string TraversalConfigurationResourcePath = "TraversalConfiguration";

        public int Order
        {
            get { return 125; }
        }

        public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
        {
            var configuration = Resources.Load<CharacterMovementConfiguration>(ConfigurationResourcePath);

            if (configuration == null)
            {
                throw new InvalidOperationException(
                    $"Character movement configuration was not found at Resources/{ConfigurationResourcePath}.");
            }

            services.AddSingleton<ICharacterMovementService>(resolver =>
            {
                var logger = resolver.TryResolve<ILogService>(out var resolvedLogger)
                    ? resolvedLogger
                    : null;

                var traversalConfiguration = Resources.Load<TraversalConfiguration>(TraversalConfigurationResourcePath);

                if (traversalConfiguration == null)
                {
                    var allConfigs = Resources.FindObjectsOfTypeAll<TraversalConfiguration>();
                    if (allConfigs != null && allConfigs.Length > 0)
                    {
                        traversalConfiguration = allConfigs[0];
                    }
                }

                logger?.Info(
                    "Gameplay.CharacterMovement",
                    "Character movement service registered with configuration.");

                if (traversalConfiguration != null)
                {
                    logger?.Info(
                        "Gameplay.CharacterMovement",
                        $"Traversal configuration loaded successfully (Rules: {traversalConfiguration.SurfaceRules?.Length ?? 0}). Traversal system will be active.");
                }
                else
                {
                    logger?.Warning(
                        "Gameplay.CharacterMovement",
                        $"Traversal configuration not found at Resources/{TraversalConfigurationResourcePath}.");
                }

                return new RuntimeCharacterMovementService(configuration, logger, traversalConfiguration);
            });
        }
    }
}

