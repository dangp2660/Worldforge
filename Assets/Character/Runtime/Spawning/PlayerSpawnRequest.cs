using System;

namespace Worldforge.Character.Spawning
{
    public readonly struct PlayerSpawnRequest
    {
        public PlayerSpawnRequest(string playerId, PlayerSpawnLocation spawnLocation)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                throw new ArgumentException("Player id must be a non-empty value.", nameof(playerId));
            }

            PlayerId = playerId;
            SpawnLocation = spawnLocation;
        }

        public string PlayerId { get; }

        public PlayerSpawnLocation SpawnLocation { get; }
    }
}
