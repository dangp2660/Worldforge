using UnityEngine;

namespace Worldforge.Character.Spawning
{
    public interface IPlayerSpawnService
    {
        bool HasActivePlayer { get; }

        GameObject ActivePlayer { get; }

        GameObject Spawn(PlayerSpawnRequest request);

        void DespawnActivePlayer();
    }
}
