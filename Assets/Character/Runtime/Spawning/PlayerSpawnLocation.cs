using System;
using UnityEngine;

namespace Worldforge.Character.Spawning
{
    public readonly struct PlayerSpawnLocation
    {
        public PlayerSpawnLocation(string spawnId, Vector3 position, Quaternion rotation)
        {
            if (string.IsNullOrWhiteSpace(spawnId))
            {
                throw new ArgumentException("Spawn id must be a non-empty value.", nameof(spawnId));
            }

            SpawnId = spawnId;
            Position = position;
            Rotation = rotation;
        }

        public string SpawnId { get; }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }
    }
}
