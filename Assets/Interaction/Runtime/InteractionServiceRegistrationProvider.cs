using System;
using UnityEngine;
using Worldforge.Character.State;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Interaction
{
    /// <summary>
    /// Registers <see cref="IInteractionService"/> into the DI container.
    /// Order 135 — runs after CharacterState (130).
    /// Auto-discovered via reflection by <c>ServiceRegistrationDiscovery</c>.
    /// </summary>
    public sealed class InteractionServiceRegistrationProvider : IServiceRegistrationProvider
    {
        private const string ConfigurationResourcePath = "InteractionConfiguration";
        private const string SubfolderConfigurationResourcePath = "Interaction/InteractionConfiguration";

        public int Order
        {
            get { return 135; }
        }

        public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
        {
            services.AddSingleton<IInteractionService>(resolver =>
            {
                var logger = resolver.TryResolve<ILogService>(out var resolvedLogger)
                    ? resolvedLogger
                    : null;

                var stateService = resolver.TryResolve<ICharacterStateService>(out var resolvedStateService)
                    ? resolvedStateService
                    : null;

                var configuration = Resources.Load<InteractionConfiguration>(ConfigurationResourcePath);
                if (configuration == null)
                {
                    configuration = Resources.Load<InteractionConfiguration>(SubfolderConfigurationResourcePath);
                }

                if (configuration == null)
                {
                    var allConfigs = Resources.FindObjectsOfTypeAll<InteractionConfiguration>();
                    if (allConfigs != null && allConfigs.Length > 0)
                    {
                        configuration = allConfigs[0];
                    }
                }

                if (configuration == null)
                {
                    logger?.Warning(
                        "Gameplay.Interaction",
                        $"InteractionConfiguration asset not found at Resources/{ConfigurationResourcePath}. Using runtime default instance.");
                    configuration = ScriptableObject.CreateInstance<InteractionConfiguration>();
                }
                else
                {
                    logger?.Info(
                        "Gameplay.Interaction",
                        "Interaction configuration loaded successfully.");
                }

                logger?.Info("Gameplay.Interaction", "Interaction service registered.");

                return new RuntimeInteractionService(stateService, configuration, logger);
            });
        }
    }
}
