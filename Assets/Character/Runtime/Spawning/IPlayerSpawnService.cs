using System;
using UnityEngine;

namespace Worldforge.Character.Spawning
{
    public interface IPlayerSpawnService
    {
        event Action<GameObject> PlayerSpawned;

        event Action<GameObject> PlayerDespawning;

        bool HasActivePlayer { get; }

        GameObject ActivePlayer { get; }

        string SelectedSpawnId { get; set; }

        GameObject Spawn(PlayerSpawnRequest request);

        GameObject SpawnAt(PlayerSpawnLocation location);

        void TeleportActivePlayer(PlayerSpawnLocation location);

        void DespawnActivePlayer();
    }
}
