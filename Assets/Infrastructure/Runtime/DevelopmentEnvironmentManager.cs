using UnityEngine;

namespace Worldforge.Infrastructure.Development
{
    public sealed class DevelopmentEnvironmentManager : MonoBehaviour
    {
        private const string GeneratedObjectPrefix = "DevGenerated_";

        [SerializeField] private Transform environmentRoot;
        [SerializeField] private Vector3 floorScale = new Vector3(4f, 1f, 4f);
        [SerializeField] private float floorHeight;
        [SerializeField] private Vector3 markerScale = new Vector3(1f, 2f, 1f);
        [SerializeField] private float markerOffset = 18f;
        [SerializeField] private bool generateOnAwake = true;

        private void Awake()
        {
            if (!generateOnAwake)
            {
                return;
            }

            if (environmentRoot == null)
            {
                environmentRoot = transform;
            }

            ClearGeneratedChildren();
            CreateFloor();
            CreateCornerMarkers();
            CreatePlayerSpawnAnchor();
        }

        private void ClearGeneratedChildren()
        {
            for (var i = environmentRoot.childCount - 1; i >= 0; i--)
            {
                var child = environmentRoot.GetChild(i);
                if (!child.name.StartsWith(GeneratedObjectPrefix, System.StringComparison.Ordinal))
                {
                    continue;
                }

                Destroy(child.gameObject);
            }
        }

        private void CreateFloor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = $"{GeneratedObjectPrefix}Floor";
            floor.transform.SetParent(environmentRoot, false);
            floor.transform.localPosition = new Vector3(0f, floorHeight, 0f);
            floor.transform.localRotation = Quaternion.identity;
            floor.transform.localScale = floorScale;
        }

        private void CreateCornerMarkers()
        {
            CreateMarker("NorthWest", new Vector3(-markerOffset, markerScale.y * 0.5f, markerOffset));
            CreateMarker("NorthEast", new Vector3(markerOffset, markerScale.y * 0.5f, markerOffset));
            CreateMarker("SouthWest", new Vector3(-markerOffset, markerScale.y * 0.5f, -markerOffset));
            CreateMarker("SouthEast", new Vector3(markerOffset, markerScale.y * 0.5f, -markerOffset));
        }

        private void CreateMarker(string markerName, Vector3 localPosition)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = $"{GeneratedObjectPrefix}{markerName}Marker";
            marker.transform.SetParent(environmentRoot, false);
            marker.transform.localPosition = localPosition;
            marker.transform.localRotation = Quaternion.identity;
            marker.transform.localScale = markerScale;
        }

        private void CreatePlayerSpawnAnchor()
        {
            var spawnAnchor = new GameObject($"{GeneratedObjectPrefix}PlayerSpawn");
            spawnAnchor.transform.SetParent(environmentRoot, false);
            spawnAnchor.transform.localPosition = new Vector3(0f, floorHeight, 0f);
            spawnAnchor.transform.localRotation = Quaternion.identity;
            spawnAnchor.transform.localScale = Vector3.one;
        }
    }
}
