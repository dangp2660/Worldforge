using UnityEngine;

namespace Worldforge.Interaction.Detection
{
    // Detects the closest valid IInteractable within range.
    // Uses OverlapSphereNonAlloc to avoid GC allocation per Standard §24.
    // Pure C# — no MonoBehaviour dependency.
    internal sealed class InteractionDetector
    {
        private readonly Collider[] _hitBuffer;

        public InteractionDetector(int bufferSize)
        {
            _hitBuffer = new Collider[Mathf.Max(1, bufferSize)];
        }

        public InteractionTarget Detect(Vector3 origin, float maxDistance, LayerMask layerMask)
        {
            var mask = layerMask.value == 0 ? ~0 : layerMask.value;
            var hitCount = Physics.OverlapSphereNonAlloc(origin, maxDistance, _hitBuffer, mask, QueryTriggerInteraction.Collide);

            IInteractable closestInteractable = null;
            var closestDistance = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                var collider = _hitBuffer[i];
                if (collider == null) continue;

                var interactable = collider.GetComponentInParent<IInteractable>()
                    ?? collider.GetComponentInChildren<IInteractable>()
                    ?? collider.GetComponent<IInteractable>();

                if (interactable == null) continue;
                if (!interactable.IsInteractable) continue;

                var interactionPoint = interactable.InteractionPoint;
                if (interactionPoint == null) continue;

                var colliderClosestPoint = collider.ClosestPoint(origin);
                var colliderDistance = Vector3.Distance(origin, colliderClosestPoint);
                var pointDistance = Vector3.Distance(origin, interactionPoint.position);
                var distance = Mathf.Min(colliderDistance, pointDistance);

                if (distance > maxDistance) continue;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }

            // Clear buffer references to avoid holding stale object references.
            for (var i = 0; i < hitCount; i++)
            {
                _hitBuffer[i] = null;
            }

            if (closestInteractable != null)
            {
                return new InteractionTarget(closestInteractable, closestDistance);
            }

            return InteractionTarget.None;
        }
    }
}
