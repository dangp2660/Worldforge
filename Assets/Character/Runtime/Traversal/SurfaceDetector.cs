using UnityEngine;
using Worldforge.Character.Movement;

namespace Worldforge.Character.Traversal
{
    public sealed class SurfaceDetector
    {
        private static readonly Collider[] OverlapBuffer = new Collider[8];
        private static readonly RaycastHit[] RaycastBuffer = new RaycastHit[4];

        /// <summary>
        /// Detects the surface type under the character.
        /// Priority:
        /// 1. Directly inspect the ground hit collider from GroundCheckResult.
        /// 2. Query colliders overlapping the ground hit point.
        /// 3. Raycast down specifically at the hit point (avoiding the character body).
        /// </summary>
        public SurfaceType Detect(
            GroundCheckResult ground,
            Vector3 characterPosition,
            LayerMask groundLayers,
            Collider directContactCollider = null)
        {
            // 0. Direct controller contact from OnControllerColliderHit
            if (directContactCollider != null)
            {
                var surface = GetSurfaceTypeFromCollider(directContactCollider);
                if (surface != SurfaceType.Default)
                {
                    return surface;
                }
            }

            // 1. Direct collider check from ground hit
            if (ground.HitCollider != null)
            {
                var surface = GetSurfaceTypeFromCollider(ground.HitCollider);
                if (surface != SurfaceType.Default)
                {
                    return surface;
                }
            }

            // 2. Overlap check around character feet (handles raised surfaces, triggers, and close contact)
            var feetOverlapCount = Physics.OverlapSphereNonAlloc(
                characterPosition + Vector3.up * 0.2f,
                0.6f,
                OverlapBuffer,
                groundLayers,
                QueryTriggerInteraction.Collide);

            for (var i = 0; i < feetOverlapCount; i++)
            {
                var surface = GetSurfaceTypeFromCollider(OverlapBuffer[i]);
                if (surface != SurfaceType.Default)
                {
                    return surface;
                }
            }

            // 3. Overlap check at ground hit point if grounded
            if (ground.IsGrounded)
            {
                var overlapCount = Physics.OverlapSphereNonAlloc(
                    ground.HitPoint + Vector3.up * 0.1f,
                    0.3f,
                    OverlapBuffer,
                    groundLayers,
                    QueryTriggerInteraction.Collide);

                for (var i = 0; i < overlapCount; i++)
                {
                    var surface = GetSurfaceTypeFromCollider(OverlapBuffer[i]);
                    if (surface != SurfaceType.Default)
                    {
                        return surface;
                    }
                }
            }

            // 4. Downward raycast targeted directly at feet
            var rayOrigin = characterPosition + Vector3.up * 0.3f;
            var hitCount = Physics.RaycastNonAlloc(
                rayOrigin,
                Vector3.down,
                RaycastBuffer,
                0.8f,
                groundLayers,
                QueryTriggerInteraction.Collide);

            for (var i = 0; i < hitCount; i++)
            {
                var surface = GetSurfaceTypeFromCollider(RaycastBuffer[i].collider);
                if (surface != SurfaceType.Default)
                {
                    return surface;
                }
            }

            return SurfaceType.Default;
        }

        private static SurfaceType GetSurfaceTypeFromCollider(Collider col)
        {
            if (col == null || IsPlayerCollider(col))
            {
                return SurfaceType.Default;
            }

            var tag = col.GetComponent<SurfaceTag>()
                ?? col.GetComponentInParent<SurfaceTag>()
                ?? col.GetComponentInChildren<SurfaceTag>();

            if (tag != null)
            {
                return tag.SurfaceType;
            }

            return SurfaceType.Default;
        }

        private static bool IsPlayerCollider(Collider col)
        {
            if (col == null)
            {
                return true;
            }

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


