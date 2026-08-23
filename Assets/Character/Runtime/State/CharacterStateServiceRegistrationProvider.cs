using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Character.State
{
    /// <summary>
    /// Registers <see cref="ICharacterStateService"/> into the DI container.
    /// Order 130 — after PlayerSpawn (120) and CharacterMovement (125).
    /// Auto-discovered via reflection by <c>ServiceRegistrationDiscovery</c>.
    /// </summary>
    public sealed class CharacterStateServiceRegistrationProvider : IServiceRegistrationProvider
    {
        public int Order => 130;

        public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
        {
            services.AddSingleton<ICharacterStateService>(resolver =>
            {
                var logger = resolver.TryResolve<ILogService>(out var resolvedLogger)
                    ? resolvedLogger
                    : null;

                logger?.Info("Gameplay.CharacterState", "Character state service registering.");

                return new RuntimeCharacterStateService(logger);
            });
        }
    }
}
