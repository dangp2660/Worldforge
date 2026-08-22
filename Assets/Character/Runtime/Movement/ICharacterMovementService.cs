using UnityEngine;
using Worldforge.Character.Traversal;

namespace Worldforge.Character.Movement
{
    public interface ICharacterMovementService
    {
        bool IsAttached { get; }

        bool IsGrounded { get; }

        bool IsSprinting { get; }

        Vector3 CurrentVelocity { get; }

        /// <summary>
        /// Returns the traversal service if available, or null if traversal is not configured.
        /// </summary>
        ITraversalService Traversal { get; }

        void AttachToPlayer(GameObject playerObject);

        void DetachFromPlayer();
    }
}

