using System.Collections.Generic;
using Worldforge.Core.Bootstrap;

namespace Worldforge.Character.State
{
    /// <summary>
    /// Provides <see cref="CharacterStateInitializationSystem"/> to the application bootstrap.
    /// Auto-discovered via reflection by <c>ApplicationSystemDiscovery</c>.
    /// Order 130 — after PlayerSpawn (120) and CharacterMovement (125).
    /// </summary>
    public sealed class CharacterStateInitializationSystemProvider : IApplicationSystemProvider
    {
        public int Order => 130;

        public IEnumerable<IApplicationSystem> CreateSystems()
        {
            return new IApplicationSystem[]
            {
                new CharacterStateInitializationSystem()
            };
        }
    }
}
