using UnityEngine;

namespace Worldforge.Character.Movement
{
    public sealed class SlopeHandler
    {
        public Vector3 AdjustMovement(Vector3 movement, GroundCheckResult ground, float maxSlopeAngle)
        {
            if (!ground.IsGrounded)
            {
                return movement;
            }

            if (ground.SlopeAngle <= 0.01f)
            {
                return movement;
            }

            if (ground.SlopeAngle > maxSlopeAngle)
            {
                var slopeDirection = Vector3.ProjectOnPlane(Vector3.down, ground.HitNormal).normalized;
                var horizontalMovement = new Vector3(movement.x, 0f, movement.z);
                var dotProduct = Vector3.Dot(horizontalMovement.normalized, slopeDirection);

                if (dotProduct > 0f)
                {
                    movement.x = 0f;
                    movement.z = 0f;
                }

                return movement;
            }

            var horizontalComponent = new Vector3(movement.x, 0f, movement.z);

            if (horizontalComponent.sqrMagnitude < 0.0001f)
            {
                return movement;
            }

            var projectedMovement = Vector3.ProjectOnPlane(horizontalComponent, ground.HitNormal);
            projectedMovement = projectedMovement.normalized * horizontalComponent.magnitude;

            return new Vector3(projectedMovement.x, projectedMovement.y + movement.y, projectedMovement.z);
        }
    }
}
