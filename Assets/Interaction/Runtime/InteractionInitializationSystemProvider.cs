using System.Collections.Generic;
using Worldforge.Core.Bootstrap;

namespace Worldforge.Interaction
{
    /// <summary>
    /// Provides <see cref="InteractionInitializationSystem"/> to the application bootstrap flow.
    /// Auto-discovered via reflection by <c>ApplicationSystemDiscovery</c>.
    /// Order 135 — runs after CharacterState (130).
    /// </summary>
    public sealed class InteractionInitializationSystemProvider : IApplicationSystemProvider
    {
        public int Order
        {
            get { return 135; }
        }

        public IEnumerable<IApplicationSystem> CreateSystems()
        {
            return new IApplicationSystem[]
            {
                new InteractionInitializationSystem()
            };
        }
    }
}
