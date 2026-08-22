using UnityEngine;
using Worldforge.Character.Traversal;

namespace Worldforge.Infrastructure.Development
{
    /// <summary>
    /// Generates surface zones in the development scene for traversal demonstration and validation.
    /// Creates colored surface patches with SurfaceTag components:
    ///   - Green  patch (Grass)  — slight speed reduction
    ///   - Brown  patch (Mud)    — heavy speed reduction
    ///   - Cyan   patch (Ice)    — slight speed boost
    ///   - Red    patch (Lava)   — movement blocked
    ///   - Yellow patch (Sand)   — moderate speed reduction
    ///   - Angled ramp           — slope traversal
    /// </summary>
    public sealed class TraversalSurfaceGenerator : MonoBehaviour
    {
        private const string GeneratedPrefix = "Traversal_";

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");

        [Header("Generation")]
        [SerializeField] private bool generateOnAwake = true;
        [SerializeField] private Transform environmentRoot;

        [Header("Layout")]
        [SerializeField] private float patchSize = 4f;
        [SerializeField] private float patchSpacing = 6f;
        [SerializeField] private float startOffsetZ = 5f;

        [Header("Ramp")]
        [SerializeField] private float rampLength = 8f;
        [SerializeField] private float rampAngle = 35f;

        [Header("Surface Colors")]
        [SerializeField] private Color grassColor = new Color(0.2f, 0.6f, 0.2f, 1f);
        [SerializeField] private Color mudColor = new Color(0.4f, 0.25f, 0.1f, 1f);
        [SerializeField] private Color iceColor = new Color(0.6f, 0.85f, 0.95f, 1f);
        [SerializeField] private Color lavaColor = new Color(0.9f, 0.15f, 0.05f, 1f);
        [SerializeField] private Color sandColor = new Color(0.85f, 0.75f, 0.45f, 1f);
        [SerializeField] private Color rampColor = new Color(0.5f, 0.5f, 0.55f, 1f);

        private Transform Root
        {
            get { return environmentRoot != null ? environmentRoot : transform; }
        }

        private void Awake()
        {
            if (!generateOnAwake)
            {
                return;
            }

            Generate();
        }

        public void Generate()
        {
            ClearGenerated();
            CreateSurfacePatches();
            CreateSlopeRamp();
        }

        private void ClearGenerated()
        {
            var root = Root;

            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);

                if (!child.name.StartsWith(GeneratedPrefix, System.StringComparison.Ordinal))
                {
                    continue;
                }

                Destroy(child.gameObject);
            }
        }

        private void CreateSurfacePatches()
        {
            var startX = -2f * patchSpacing;
            var z = startOffsetZ;

            CreateSurfacePatch("Grass", SurfaceType.Grass, grassColor, 0.1f,
                new Vector3(startX, 0.01f, z));

            CreateSurfacePatch("Mud", SurfaceType.Mud, mudColor, 0f,
                new Vector3(startX + patchSpacing, 0.01f, z));

            CreateSurfacePatch("Ice", SurfaceType.Ice, iceColor, 0.9f,
                new Vector3(startX + patchSpacing * 2f, 0.01f, z));

            CreateSurfacePatch("Lava", SurfaceType.Lava, lavaColor, 0.3f,
                new Vector3(startX + patchSpacing * 3f, 0.01f, z));

            CreateSurfacePatch("Sand", SurfaceType.Sand, sandColor, 0f,
                new Vector3(startX + patchSpacing * 4f, 0.01f, z));
        }

        private void CreateSurfacePatch(
            string patchName,
            SurfaceType surfaceType,
            Color color,
            float smoothness,
            Vector3 localPosition)
        {
            var patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            patch.name = $"{GeneratedPrefix}{patchName}";
            patch.transform.SetParent(Root, false);
            patch.transform.localPosition = localPosition;
            patch.transform.localScale = new Vector3(patchSize, 0.02f, patchSize);

            var boxCollider = patch.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.isTrigger = true;
            }

            var surfaceTag = patch.AddComponent<SurfaceTag>();
            surfaceTag.SurfaceType = surfaceType;

            ApplyMaterial(patch, color, smoothness);

            CreateLabel(patchName, surfaceType, localPosition);
        }

        private void CreateLabel(string patchName, SurfaceType surfaceType, Vector3 patchPosition)
        {
            var labelObject = new GameObject($"{GeneratedPrefix}{patchName}_Label");
            labelObject.transform.SetParent(Root, false);
            labelObject.transform.localPosition = patchPosition + new Vector3(0f, 1.5f, -patchSize * 0.5f - 0.3f);

            var textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = $"{patchName}\n({surfaceType})";
            textMesh.characterSize = 0.3f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 48;
            textMesh.color = Color.white;
        }

        private void CreateSlopeRamp()
        {
            var rampObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rampObject.name = $"{GeneratedPrefix}SlopeRamp";
            rampObject.transform.SetParent(Root, false);

            var rampZ = startOffsetZ + patchSpacing + 2f;
            rampObject.transform.localPosition = new Vector3(0f, rampLength * 0.5f * Mathf.Sin(rampAngle * Mathf.Deg2Rad) * 0.5f, rampZ);
            rampObject.transform.localRotation = Quaternion.Euler(-rampAngle, 0f, 0f);
            rampObject.transform.localScale = new Vector3(patchSize, 0.1f, rampLength);

            ApplyMaterial(rampObject, rampColor, 0.05f);

            var labelObject = new GameObject($"{GeneratedPrefix}SlopeRamp_Label");
            labelObject.transform.SetParent(Root, false);
            labelObject.transform.localPosition = new Vector3(0f, 2.5f, rampZ - rampLength * 0.5f - 0.5f);

            var textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = $"Slope Ramp\n({rampAngle}°)";
            textMesh.characterSize = 0.3f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 48;
            textMesh.color = Color.white;
        }

        private static void ApplyMaterial(GameObject target, Color color, float smoothness)
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

            var material = new Material(shader) { color = color };

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
