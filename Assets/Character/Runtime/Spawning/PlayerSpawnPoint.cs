using UnityEngine;

namespace Worldforge.Character.Spawning
{
    public sealed class PlayerSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string spawnId = "default";
        [SerializeField] private bool isDefault = true;

        public string SpawnId
        {
            get { return spawnId; }
        }

        public bool IsDefault
        {
            get { return isDefault; }
        }

        public PlayerSpawnLocation CreateLocation()
        {
            return new PlayerSpawnLocation(
                spawnId,
                transform.position,
                transform.rotation);
        }

        private void OnValidate()
        {
            spawnId = spawnId?.Trim();
        }
    }
}
