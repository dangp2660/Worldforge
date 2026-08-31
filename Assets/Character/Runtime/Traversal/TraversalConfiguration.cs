using UnityEngine;

namespace Worldforge.Character.Traversal
{
    [CreateAssetMenu(
        fileName = "TraversalConfiguration",
        menuName = "Worldforge/Character/Traversal Configuration")]
    public sealed class TraversalConfiguration : ScriptableObject
    {
        [Header("Slope Settings")]
        [SerializeField] private float _defaultMaxSlopeAngle = 45f;
        [SerializeField] private bool _slopeSpeedReductionEnabled = true;
        [SerializeField] private AnimationCurve _slopeSpeedCurve = CreateDefaultSlopeCurve();

        [Header("Surface Rules")]
        [SerializeField] private SurfaceTraversalRule[] _surfaceRules = new SurfaceTraversalRule[0];
        [SerializeField] private float _defaultSurfaceSpeedMultiplier = 1f;
        [SerializeField] private bool _defaultSurfaceTraversable = true;

        public float DefaultMaxSlopeAngle
        {
            get { return _defaultMaxSlopeAngle; }
        }

        public bool SlopeSpeedReductionEnabled
        {
            get { return _slopeSpeedReductionEnabled; }
        }

        public AnimationCurve SlopeSpeedCurve
        {
            get { return _slopeSpeedCurve; }
        }

        public SurfaceTraversalRule[] SurfaceRules
        {
            get { return _surfaceRules; }
        }

        public float DefaultSurfaceSpeedMultiplier
        {
            get { return _defaultSurfaceSpeedMultiplier; }
        }

        public bool DefaultSurfaceTraversable
        {
            get { return _defaultSurfaceTraversable; }
        }

        public SurfaceTraversalRule GetRuleForSurface(SurfaceType surfaceType)
        {
            if (_surfaceRules == null)
            {
                return null;
            }

            for (var i = 0; i < _surfaceRules.Length; i++)
            {
                if (_surfaceRules[i] != null && _surfaceRules[i].SurfaceType == surfaceType)
                {
                    return _surfaceRules[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the effective max slope angle for a surface type, considering per-surface overrides.
        /// </summary>
        public float GetEffectiveMaxSlope(SurfaceType surfaceType)
        {
            var rule = GetRuleForSurface(surfaceType);

            if (rule != null && rule.HasSlopeOverride)
            {
                return rule.MaxSlopeOverride;
            }

            return _defaultMaxSlopeAngle;
        }

        /// <summary>
        /// Evaluates the slope speed factor for a given angle using the configured curve.
        /// Returns 1.0 if slope speed reduction is disabled.
        /// </summary>
        public float EvaluateSlopeSpeedFactor(float slopeAngle)
        {
            if (!_slopeSpeedReductionEnabled || _slopeSpeedCurve == null)
            {
                return 1f;
            }

            var normalizedAngle = Mathf.Clamp01(slopeAngle / 90f);
            return Mathf.Clamp01(_slopeSpeedCurve.Evaluate(normalizedAngle));
        }

        private void OnValidate()
        {
            _defaultMaxSlopeAngle = Mathf.Clamp(_defaultMaxSlopeAngle, 0f, 89f);
            _defaultSurfaceSpeedMultiplier = Mathf.Max(0f, _defaultSurfaceSpeedMultiplier);

            if (_slopeSpeedCurve == null || _slopeSpeedCurve.length == 0)
            {
                _slopeSpeedCurve = CreateDefaultSlopeCurve();
            }
        }

        private static AnimationCurve CreateDefaultSlopeCurve()
        {
            // Linear falloff: 1.0 at 0° (normalized 0.0) → 0.3 at 45° (normalized 0.5) → 0.0 at 90° (normalized 1.0)
            return new AnimationCurve(
                new Keyframe(0f, 1f, 0f, -1.4f),
                new Keyframe(0.5f, 0.3f, -1.4f, -0.6f),
                new Keyframe(1f, 0f, -0.6f, 0f));
        }
    }
}
