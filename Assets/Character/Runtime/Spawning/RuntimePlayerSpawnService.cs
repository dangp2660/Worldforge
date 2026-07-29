using System;
using UnityEngine;

namespace Worldforge.Character.Spawning
{
    public sealed class RuntimePlayerSpawnService : IPlayerSpawnService, IDisposable
    {
        private readonly Func<GameObject> playerFactory;

        private GameObject activePlayer;

        public RuntimePlayerSpawnService(Func<GameObject> playerFactory)
        {
            this.playerFactory = playerFactory ?? throw new ArgumentNullException(nameof(playerFactory));
        }

        public bool HasActivePlayer
        {
            get { return activePlayer != null; }
        }

        public GameObject ActivePlayer
        {
            get { return activePlayer; }
        }

        public GameObject Spawn(PlayerSpawnRequest request)
        {
            if (HasActivePlayer)
            {
                throw new InvalidOperationException(
                    "A player has already been spawned for this session.");
            }

            var player = playerFactory();
            if (player == null)
            {
                throw new InvalidOperationException("Player factory returned null.");
            }

            player.transform.SetPositionAndRotation(
                request.SpawnLocation.Position,
                request.SpawnLocation.Rotation);

            activePlayer = player;
            return activePlayer;
        }

        public void DespawnActivePlayer()
        {
            if (activePlayer == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(activePlayer);
            activePlayer = null;
        }

        public void Dispose()
        {
            DespawnActivePlayer();
        }
    }
}
