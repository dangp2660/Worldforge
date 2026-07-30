using System;
using UnityEngine;

namespace Worldforge.Character.Spawning
{
    public sealed class RuntimePlayerSpawnService : IPlayerSpawnService, IDisposable
    {
        private readonly Func<GameObject> playerFactory;

        private GameObject activePlayer;
        private string _selectedSpawnId;

        public RuntimePlayerSpawnService(Func<GameObject> playerFactory, string defaultSpawnId = null)
        {
            this.playerFactory = playerFactory ?? throw new ArgumentNullException(nameof(playerFactory));
            _selectedSpawnId = defaultSpawnId?.Trim();
        }

        public bool HasActivePlayer
        {
            get { return activePlayer != null; }
        }

        public GameObject ActivePlayer
        {
            get { return activePlayer; }
        }

        public string SelectedSpawnId
        {
            get { return _selectedSpawnId; }
            set { _selectedSpawnId = value?.Trim(); }
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

        public GameObject SpawnAt(PlayerSpawnLocation location)
        {
            const string localPlayerId = "local-player";
            var request = new PlayerSpawnRequest(localPlayerId, location);
            return Spawn(request);
        }

        public void TeleportActivePlayer(PlayerSpawnLocation location)
        {
            if (activePlayer == null)
            {
                return;
            }

            activePlayer.transform.SetPositionAndRotation(
                location.Position,
                location.Rotation);
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
