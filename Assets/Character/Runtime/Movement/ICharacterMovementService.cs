using UnityEngine;

namespace Worldforge.Character.Movement
{
    public interface ICharacterMovementService
    {
        bool IsAttached { get; }

        bool IsGrounded { get; }

        bool IsSprinting { get; }

        Vector3 CurrentVelocity { get; }

        void AttachToPlayer(GameObject playerObject);

        void DetachFromPlayer();
    }
}
