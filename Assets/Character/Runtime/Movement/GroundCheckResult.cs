using UnityEngine;

namespace Worldforge.Character.Movement
{
    public readonly struct GroundCheckResult
    {
        public GroundCheckResult(
            bool isGrounded,
            Vector3 hitPoint,
            Vector3 hitNormal,
            float slopeAngle,
            float hitDistance)
        {
            IsGrounded = isGrounded;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            SlopeAngle = slopeAngle;
            HitDistance = hitDistance;
        }

        public bool IsGrounded { get; }

        public Vector3 HitPoint { get; }

        public Vector3 HitNormal { get; }

        public float SlopeAngle { get; }

        public float HitDistance { get; }

        public static GroundCheckResult NotGrounded
        {
            get
            {
                return new GroundCheckResult(
                    false,
                    Vector3.zero,
                    Vector3.up,
                    0f,
                    0f);
            }
        }
    }
}
