using System;
using System.Collections.Generic;
using UnityEngine;
using Worldforge.Character.Player;
using Worldforge.Core.Bootstrap;

namespace Worldforge.Character.Spawning
{
    public sealed class PlayerSpawnInitializationSystemProvider : IApplicationSystemProvider
    {
        public int Order { get { return 120; } }

        public IEnumerable<IApplicationSystem> CreateSystems()
        {
            return new IApplicationSystem[]
            {
                new PlayerSpawnInitializationSystem()
            };
        }
    }
}
