using System.Collections.Generic;
using Worldforge.Core.Bootstrap;

namespace Worldforge.Character.Movement
{
    public sealed class CharacterMovementInitializationSystemProvider : IApplicationSystemProvider
    {
        public int Order
        {
            get { return 125; }
        }

        public IEnumerable<IApplicationSystem> CreateSystems()
        {
            return new IApplicationSystem[]
            {
                new CharacterMovementInitializationSystem()
            };
        }
    }
}
