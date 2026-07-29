using UnityEngine;

namespace Worldforge.Character.Spawning
{
    [CreateAssetMenu(
    fileName = "PlayerSpawnConfiguration",
    menuName = "Worldforge/Character/Player Spawn Configuration")]
    public sealed class PlayerSpawnConfiguration : ScriptableObject
    {
        public GameObject PlayerPrefab;
    }
}
