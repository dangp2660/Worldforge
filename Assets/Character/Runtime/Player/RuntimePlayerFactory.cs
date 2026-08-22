using UnityEngine;

namespace Worldforge.Character.Player
{
    public sealed class RuntimePlayerFactory
    {
        private const string PlayerGameObjectName = "Worldforge.Player";

        public GameObject CreatePlayer(GameObject prefabObject)
        {
            var playerObject = prefabObject != null
                ? Object.Instantiate(prefabObject)
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);

            playerObject.name = PlayerGameObjectName;

            if (prefabObject == null)
            {
                var capsuleCollider = playerObject.GetComponent<CapsuleCollider>();
                if (capsuleCollider != null)
                {
                    Object.Destroy(capsuleCollider);
                }
            }

            if (playerObject.GetComponent<PlayerAvatar>() == null)
            {
                playerObject.AddComponent<PlayerAvatar>();
            }

            return playerObject;
        }
    }
}