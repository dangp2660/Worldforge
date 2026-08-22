using System;
using UnityEngine;

namespace Worldforge.Character.Traversal
{
    [Serializable]
    public sealed class SurfaceTraversalRule
    {
        [SerializeField] private SurfaceType _surfaceType = SurfaceType.Default;
        [SerializeField] private bool _isTraversable = true;
        [SerializeField] private float _speedMultiplier = 1f;
        [SerializeField] private float _maxSlopeOverride = -1f;

        public SurfaceTraversalRule()
        {
        }

        public SurfaceTraversalRule(
            SurfaceType surfaceType,
            bool isTraversable = true,
            float speedMultiplier = 1f,
            float maxSlopeOverride = -1f)
        {
            _surfaceType = surfaceType;
            _isTraversable = isTraversable;
            _speedMultiplier = speedMultiplier;
            _maxSlopeOverride = maxSlopeOverride;
        }

        public SurfaceType SurfaceType
        {
            get { return _surfaceType; }
        }

        public bool IsTraversable
        {
            get { return _isTraversable; }
        }

        public float SpeedMultiplier
        {
            get { return _speedMultiplier; }
        }

        /// <summary>
        /// Per-surface max slope override. A value of -1 means use the global default.
        /// </summary>
        public float MaxSlopeOverride
        {
            get { return _maxSlopeOverride; }
        }

        public bool HasSlopeOverride
        {
            get { return _maxSlopeOverride >= 0f; }
        }
    }
}

