using UnityEngine;

namespace Worldforge.Character.Movement
{
    public sealed class GroundDetector
    {
        private static readonly RaycastHit[] HitBuffer = new RaycastHit[4];

        public GroundCheckResult Detect(
            Vector3 position,
            float radius,
            float distance,
            LayerMask groundLayers)
        {
            var origin = position + Vector3.up * radius;
            var castDistance = distance + radius;

            var hitCount = Physics.SphereCastNonAlloc(
                origin,
                radius,
                Vector3.down,
                HitBuffer,
                castDistance,
                groundLayers,
                QueryTriggerInteraction.Ignore);

            if (hitCount == 0)
            {
                return GroundCheckResult.NotGrounded;
            }

            var closestIndex = 0;
            var closestDistance = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                if (HitBuffer[i].distance < closestDistance)
                {
                    closestDistance = HitBuffer[i].distance;
                    closestIndex = i;
                }
            }

            var hit = HitBuffer[closestIndex];
            var slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            return new GroundCheckResult(
                true,
                hit.point,
                hit.normal,
                slopeAngle,
                hit.distance);
        }
    }
}
