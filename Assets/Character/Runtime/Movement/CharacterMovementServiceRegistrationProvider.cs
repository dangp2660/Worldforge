using System;
using UnityEngine;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Character.Movement
{
    public sealed class CharacterMovementServiceRegistrationProvider : IServiceRegistrationProvider
    {
        private const string ConfigurationResourcePath = "CharacterMovementConfiguration";

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

                logger?.Info(
                    "Gameplay.CharacterMovement",
                    "Character movement service registered with configuration.");

                return new RuntimeCharacterMovementService(configuration, logger);
            });
        }
    }
}
