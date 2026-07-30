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

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = isDefault ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawRay(transform.position, transform.forward * 1f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.7f);
            Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
        }
#endif
    }
}
