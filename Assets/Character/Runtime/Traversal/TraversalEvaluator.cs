using System;
using UnityEngine;
using Worldforge.Character.Movement;

namespace Worldforge.Character.Traversal
{
    public sealed class TraversalEvaluator
    {
        private TraversalConfiguration _configuration;

        public TraversalEvaluator(TraversalConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Evaluates whether the character can traverse the current surface at the current slope.
        /// The intendedMovement parameter is included for future directional slope checks
        /// (e.g. uphill vs downhill modifiers) but is not used in v0.1.
        /// </summary>
        public TraversalCheckResult Evaluate(
            GroundCheckResult ground,
            Vector3 intendedMovement,
            SurfaceType surfaceType)
        {
            var rule = _configuration.GetRuleForSurface(surfaceType);

            var isTraversable = rule != null ? rule.IsTraversable : _configuration.DefaultSurfaceTraversable;

            if (!isTraversable)
            {
                return TraversalCheckResult.Denied(surfaceType, TraversalDenialReason.NonTraversableSurface);
            }

            if (ground.IsGrounded)
            {
                var effectiveMaxSlope = _configuration.GetEffectiveMaxSlope(surfaceType);

                if (ground.SlopeAngle > effectiveMaxSlope)
                {
                    var slopeDirection = Vector3.ProjectOnPlane(Vector3.down, ground.HitNormal).normalized;
                    var isMovingUphill = intendedMovement.sqrMagnitude > 0.001f
                        && Vector3.Dot(intendedMovement.normalized, -slopeDirection) > 0.1f;

                    if (isMovingUphill)
                    {
                        return TraversalCheckResult.Denied(surfaceType, TraversalDenialReason.TooSteep);
                    }
                }
            }

            var surfaceSpeedMultiplier = rule != null
                ? rule.SpeedMultiplier
                : _configuration.DefaultSurfaceSpeedMultiplier;

            var slopeSpeedFactor = ground.IsGrounded
                ? _configuration.EvaluateSlopeSpeedFactor(ground.SlopeAngle)
                : 1f;

            return TraversalCheckResult.Allowed(surfaceType, surfaceSpeedMultiplier, slopeSpeedFactor);
        }

        public void UpdateConfiguration(TraversalConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
    }
}
