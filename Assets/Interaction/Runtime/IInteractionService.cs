using System;
using Worldforge.Interaction.Detection;
using Worldforge.Interaction.Events;

namespace Worldforge.Interaction
{
    /// <summary>
    /// Public API for the interaction system, exposed to other modules.
    /// External systems (UI, AI, Quest) use this to observe and request interactions.
    /// </summary>
    public interface IInteractionService
    {
        bool IsInteracting { get; }

        InteractionTarget CurrentTarget { get; }

        InteractionResult RequestInteraction(UnityEngine.GameObject interactor, IInteractable target);

        void CancelInteraction();

        void RegisterHandler(IInteractionHandler handler);

        void UnregisterHandler(IInteractionHandler handler);

        event Action<InteractionStartedEvent> InteractionStarted;

        event Action<InteractionCompletedEvent> InteractionCompleted;

        event Action<InteractionCancelledEvent> InteractionCancelled;
    }
}
