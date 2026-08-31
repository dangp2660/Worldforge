using UnityEngine;

namespace Worldforge.Building
{
    // Configuration data for building placement rules, layers, distances, and preview styling.
    // ScriptableObject configuration separating policy from runtime behavior.
    [CreateAssetMenu(
        fileName = "BuildingPlacementConfiguration",
        menuName = "Worldforge/Building/Building Placement Configuration")]
    public sealed class BuildingPlacementConfiguration : ScriptableObject
    {
        [Header("Layer Masks")]
        [Tooltip("Layer mask for valid ground terrain surfaces")]
        [SerializeField] private LayerMask _groundLayerMask = ~0;

        [Tooltip("Layer mask for obstacles that block structure placement")]
        [SerializeField] private LayerMask _obstructionLayerMask = 0;

        [Tooltip("Layer mask for foundation structures")]
        [SerializeField] private LayerMask _foundationLayerMask = 0;

        [Header("Placement Distance & Raycast")]
        [Tooltip("Maximum distance from camera or player to place structures")]
        [SerializeField, Min(1f)] private float _maxPlacementDistance = 25f;

        [Tooltip("Raycast distance downward to check for ground contact")]
        [SerializeField, Min(0.1f)] private float _groundCheckRayDistance = 5f;

        [Header("Preview Presentation")]
        [Tooltip("Color tint when placement position is valid")]
        [SerializeField] private Color _validPreviewColor = new Color(0.2f, 0.85f, 0.2f, 0.6f);

        [Tooltip("Color tint when placement position is invalid")]
        [SerializeField] private Color _invalidPreviewColor = new Color(0.9f, 0.2f, 0.2f, 0.6f);

        [Header("Grid & Snapping")]
        [Tooltip("Default grid cell size in world units for snapped structures")]
        [SerializeField, Min(0.1f)] private float _defaultGridSize = 1f;

        [Tooltip("Default rotation step in degrees when rotating preview")]
        [SerializeField] private float _defaultRotationStep = 90f;

        public LayerMask GroundLayerMask
        {
            get { return _groundLayerMask; }
        }

        public LayerMask ObstructionLayerMask
        {
            get { return _obstructionLayerMask; }
        }

        public LayerMask FoundationLayerMask
        {
            get { return _foundationLayerMask; }
        }

        public float MaxPlacementDistance
        {
            get { return _maxPlacementDistance; }
        }

        public float GroundCheckRayDistance
        {
            get { return _groundCheckRayDistance; }
        }

        public Color ValidPreviewColor
        {
            get { return _validPreviewColor; }
        }

        public Color InvalidPreviewColor
        {
            get { return _invalidPreviewColor; }
        }

        public float DefaultGridSize
        {
            get { return _defaultGridSize; }
        }

        public float DefaultRotationStep
        {
            get { return _defaultRotationStep; }
        }

        private void OnValidate()
        {
            _maxPlacementDistance = Mathf.Max(1f, _maxPlacementDistance);
            _groundCheckRayDistance = Mathf.Max(0.1f, _groundCheckRayDistance);
            _defaultGridSize = Mathf.Max(0.1f, _defaultGridSize);
        }
    }
}
