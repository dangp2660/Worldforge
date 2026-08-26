using UnityEngine;

namespace Worldforge.Interaction
{
    // Immutable snapshot passed to handlers and interactable callbacks.
    public readonly struct InteractionContext
    {
        public GameObject Interactor { get; }

        public IInteractable Target { get; }

        public InteractionType Type { get; }

        public float Timestamp { get; }

        public InteractionContext(
            GameObject interactor,
            IInteractable target,
            InteractionType type,
            float timestamp)
        {
            Interactor = interactor;
            Target = target;
            Type = type;
            Timestamp = timestamp;
        }

        public override string ToString()
        {
            var targetName = Target != null ? Target.InteractionPrompt : "null";
            return $"[{Type}] {targetName} @ {Timestamp:F2}s";
        }
    }
}
