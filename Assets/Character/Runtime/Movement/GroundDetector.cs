using UnityEngine;

namespace Worldforge.Character.Movement
{
    public sealed class GroundDetector
    {
        private static readonly RaycastHit[] HitBuffer = new RaycastHit[16];

        public GroundCheckResult Detect(
            Vector3 position,
            float radius,
            float distance,
            LayerMask groundLayers)
        {
            const float verticalOffset = 0.15f;
            var origin = position + Vector3.up * (radius + verticalOffset);
            var castDistance = distance + radius + verticalOffset + 0.1f;

            var hitCount = Physics.SphereCastNonAlloc(
                origin,
                radius,
                Vector3.down,
                HitBuffer,
                castDistance,
                groundLayers,
                QueryTriggerInteraction.Ignore);

            var closestIndex = -1;
            var closestDistance = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                var hitCollider = HitBuffer[i].collider;
                if (hitCollider == null || IsPlayerCollider(hitCollider))
                {
                    continue;
                }

                if (HitBuffer[i].distance < closestDistance)
                {
                    closestDistance = HitBuffer[i].distance;
                    closestIndex = i;
                }
            }

            if (closestIndex >= 0)
            {
                var hit = HitBuffer[closestIndex];
                var slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

                return new GroundCheckResult(
                    true,
                    hit.point,
                    hit.normal,
                    slopeAngle,
                    hit.distance,
                    hit.collider);
            }

            // Raycast fallback
            var rayHitCount = Physics.RaycastNonAlloc(
                position + Vector3.up * 0.3f,
                Vector3.down,
                HitBuffer,
                distance + 0.6f,
                groundLayers,
                QueryTriggerInteraction.Ignore);

            for (var i = 0; i < rayHitCount; i++)
            {
                var hitCollider = HitBuffer[i].collider;
                if (hitCollider == null || IsPlayerCollider(hitCollider))
                {
                    continue;
                }

                var hit = HitBuffer[i];
                var slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

                return new GroundCheckResult(
                    true,
                    hit.point,
                    hit.normal,
                    slopeAngle,
                    hit.distance,
                    hit.collider);
            }

            return GroundCheckResult.NotGrounded;
        }

        private static bool IsPlayerCollider(Collider col)
        {
            if (col is CharacterController)
            {
                return true;
            }

            if (col.GetComponentInParent<CharacterMovementController>() != null)
            {
                return true;
            }

            if (col.GetComponentInParent<Player.PlayerAvatar>() != null)
            {
                return true;
            }

            return false;
        }
    }
}


