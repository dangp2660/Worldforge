using System;
using UnityEngine;

namespace Worldforge.Building
{
    // Placement constraints for a structure.
    // Used by Building System to validate placement positions.
    [Serializable]
    public sealed class StructurePlacementRule
    {
        [Tooltip("Grid footprint size in units")]
        [SerializeField] private Vector2Int _footprint = new Vector2Int(1, 1);

        [Tooltip("Required clearance radius around the structure")]
        [SerializeField, Min(0f)] private float _placementRadius = 1f;

        [Tooltip("Structure must be placed on a foundation")]
        [SerializeField] private bool _requiresFoundation = false;

        [Tooltip("Structure must be placed on ground terrain")]
        [SerializeField] private bool _requiresGround = true;

        [Tooltip("Structure can be rotated during placement")]
        [SerializeField] private bool _canRotate = true;

        [Tooltip("Structure snaps to placement grid")]
        [SerializeField] private bool _snapToGrid = true;

        public StructurePlacementRule()
        {
        }

        public StructurePlacementRule(
            Vector2Int footprint,
            float placementRadius,
            bool requiresFoundation,
            bool requiresGround,
            bool canRotate,
            bool snapToGrid)
        {
            _footprint = new Vector2Int(Mathf.Max(1, footprint.x), Mathf.Max(1, footprint.y));
            _placementRadius = Mathf.Max(0f, placementRadius);
            _requiresFoundation = requiresFoundation;
            _requiresGround = requiresGround;
            _canRotate = canRotate;
            _snapToGrid = snapToGrid;
        }

        public Vector2Int Footprint
        {
            get { return _footprint; }
        }

        public float PlacementRadius
        {
            get { return _placementRadius; }
        }

        public bool RequiresFoundation
        {
            get { return _requiresFoundation; }
        }

        public bool RequiresGround
        {
            get { return _requiresGround; }
        }

        public bool CanRotate
        {
            get { return _canRotate; }
        }

        public bool SnapToGrid
        {
            get { return _snapToGrid; }
        }

        public void Validate()
        {
            _footprint = new Vector2Int(
                Mathf.Max(1, _footprint.x),
                Mathf.Max(1, _footprint.y));
            _placementRadius = Mathf.Max(0f, _placementRadius);
        }
    }
}
