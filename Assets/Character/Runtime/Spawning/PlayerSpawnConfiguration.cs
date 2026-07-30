using UnityEngine;

namespace Worldforge.Character.Spawning
{
    [CreateAssetMenu(
    fileName = "PlayerSpawnConfiguration",
    menuName = "Worldforge/Character/Player Spawn Configuration")]
    public sealed class PlayerSpawnConfiguration : ScriptableObject
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private string defaultSpawnId = "default";

        public GameObject PlayerPrefab
        {
            get { return playerPrefab; }
        }

        public string DefaultSpawnId
        {
            get { return defaultSpawnId; }
        }

        private void OnValidate()
        {
            defaultSpawnId = defaultSpawnId?.Trim();
        }
    }
}
