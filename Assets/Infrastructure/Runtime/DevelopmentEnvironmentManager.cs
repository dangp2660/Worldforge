using UnityEngine;
using Worldforge.Character.Spawning;

namespace Worldforge.Infrastructure.Development
{
    public sealed class DevelopmentEnvironmentManager : MonoBehaviour
    {
        private const string GeneratedObjectPrefix = "DevGenerated_";
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");

        [SerializeField] private Transform environmentRoot;
        [SerializeField] private Vector3 floorScale = new(4f, 1f, 4f);
        [SerializeField] private float floorHeight;
        [SerializeField] private Vector3 markerScale = new(1f, 2f, 1f);
        [SerializeField] private float markerOffset = 18f;
        [SerializeField] private Color floorColor = new(0.34f, 0.38f, 0.45f, 1f);
        [SerializeField] private Color markerColor = new(0.2f, 0.24f, 0.32f, 1f);
        [SerializeField, Range(0f, 1f)] private float floorSmoothness = 0f;
        [SerializeField, Range(0f, 1f)] private float markerSmoothness = 0.05f;
        [SerializeField] private bool generateOnAwake = true;

        public Transform EnvironmentRoot
        {
            get { return environmentRoot != null ? environmentRoot : transform; }
        }

        private void Awake()
        {
            if (!generateOnAwake)
            {
                return;
            }

            ClearGeneratedChildren();
            CreateFloor();
            CreateCornerMarkers();
            CreatePlayerSpawnAnchor();
        }

        private void ClearGeneratedChildren()
        {
            var root = EnvironmentRoot;
            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
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
            floor.transform.SetParent(EnvironmentRoot, false);
            floor.transform.SetLocalPositionAndRotation(new Vector3(0f, floorHeight, 0f), Quaternion.identity);
            floor.transform.localScale = floorScale;
            ApplyGeneratedMaterial(floor, floorColor, floorSmoothness);
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
            marker.transform.SetParent(EnvironmentRoot, false);
            marker.transform.SetLocalPositionAndRotation(localPosition, Quaternion.identity);
            marker.transform.localScale = markerScale;
            ApplyGeneratedMaterial(marker, markerColor, markerSmoothness);
        }

        private void CreatePlayerSpawnAnchor()
        {
            var spawnAnchor = new GameObject($"{GeneratedObjectPrefix}PlayerSpawn");
            spawnAnchor.transform.SetParent(EnvironmentRoot, false);
            spawnAnchor.transform.SetLocalPositionAndRotation(new Vector3(0f, floorHeight, 0f), Quaternion.identity);
            spawnAnchor.transform.localScale = Vector3.one;
            spawnAnchor.AddComponent<PlayerSpawnPoint>();
            spawnAnchor.AddComponent<Worldforge.Core.Bootstrap.GameSessionSpawnPoint>();
        }

        private static void ApplyGeneratedMaterial(GameObject target, Color color, float smoothness)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
            {
                return;
            }

            var material = new Material(shader)
            {
                color = color
            };

            if (material.HasProperty(BaseColorId))
            {
                material.SetColor(BaseColorId, color);
            }

            if (material.HasProperty(ColorId))
            {
                material.SetColor(ColorId, color);
            }

            if (material.HasProperty(MetallicId))
            {
                material.SetFloat(MetallicId, 0f);
            }

            if (material.HasProperty(SmoothnessId))
            {
                material.SetFloat(SmoothnessId, smoothness);
            }

            renderer.sharedMaterial = material;
        }
    }
}
